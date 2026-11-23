using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Models.Users;

namespace EstateFlow.src.Services
{
    public class PropertyManager
    {
        private readonly List<Property> _properties = new();

        public IReadOnlyList<Property> Properties => _properties.AsReadOnly();

        private void Add(Property property)
        {
            ArgumentNullException.ThrowIfNull(property);
            ValidateFinancials(property.TotalCost, property.AmountSpent);
            if (!Enum.IsDefined(property.Status))
                throw new ArgumentOutOfRangeException(nameof(property), "Property status is invalid.");
            if (_properties.Any(existing => string.Equals(existing.Id, property.Id, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A property with this ID already exists.");

            _properties.Add(property);
        }

        public void Add(Property property, User actor)
        {
            ArgumentNullException.ThrowIfNull(actor);
            if (!actor.HasPermission(UserPermission.ManageOwnProperties) &&
                !actor.HasPermission(UserPermission.ManageAllProperties))
            {
                throw new UnauthorizedAccessException("This account cannot add properties.");
            }

            Add(property);
        }

        public void ValidateFinancials(decimal totalCost, decimal amountSpent)
        {
            if (totalCost <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalCost), "Total cost must be greater than zero.");
            if (amountSpent < 0 || amountSpent > totalCost)
                throw new ArgumentOutOfRangeException(nameof(amountSpent), "Amount spent must be between zero and the total cost.");
        }

        public void Remove(string id)
        {
            Property? property = FindById(id);
            if (property == null)
                throw new InvalidOperationException("Property was not found.");

            _properties.Remove(property);
        }

        public Property? Find(string idOrKeyword)
        {
            if (string.IsNullOrWhiteSpace(idOrKeyword))
                throw new ArgumentException("Enter a property ID, name, or keyword.", nameof(idOrKeyword));

            string query = idOrKeyword.Trim();
            return FindById(query) ?? _properties.FirstOrDefault(property =>
                property.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public Property? FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Property ID is required.", nameof(id));

            return _properties.FirstOrDefault(property =>
                string.Equals(property.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Property> FindByExactName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Property name is required.", nameof(name));

            return _properties.Where(property =>
                    string.Equals(property.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<Property> FindAll(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("Enter a property name or keyword.", nameof(keyword));

            string query = keyword.Trim();
            return _properties.Where(property =>
                    property.GetPropertyType().Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        public void UpdateStatus(string id, PropertyStatus status, User actor)
        {
            ArgumentNullException.ThrowIfNull(actor);
            if (!Enum.IsDefined(status))
                throw new ArgumentOutOfRangeException(nameof(status), "Choose a valid property status.");

            Property? property = FindById(id);
            if (property == null)
                throw new InvalidOperationException("Property was not found.");

            if (status == PropertyStatus.Available && property.Status == PropertyStatus.Sold &&
                !actor.HasPermission(UserPermission.ManageAllProperties))
            {
                throw new UnauthorizedAccessException("Only a System Admin can make a sold property available again.");
            }

            property.SetStatus(status);
        }

        public void BuyByName(string name, User buyer)
        {
            EnsureCanBuy(buyer);
            IReadOnlyList<Property> matches = FindByExactName(name);

            if (matches.Count == 0)
                throw new InvalidOperationException("No property with that name was found.");
            if (matches.Count > 1)
                throw new InvalidOperationException("More than one property has that name. Select one by ID.");

            MarkSold(matches[0]);
        }

        public void BuyById(string id, User buyer)
        {
            EnsureCanBuy(buyer);
            Property? property = FindById(id);
            if (property == null)
                throw new InvalidOperationException("Property was not found.");

            MarkSold(property);
        }

        public void Display()
        {
            PropertyConsoleDisplay.Display(_properties);
        }

        public void DisplayAvailable()
        {
            PropertyConsoleDisplay.Display(_properties.Where(property => property.Status == PropertyStatus.Available));
        }

        public void DisplaySold()
        {
            PropertyConsoleDisplay.Display(_properties.Where(property => property.Status == PropertyStatus.Sold));
        }

        public bool SaveAll(List<Property> properties, string path)
        {
            return PropertyCsvRepository.SaveAll(properties, path);
        }

        public void LoadAll(string path)
        {
            _properties.Clear();
            foreach (Property property in PropertyCsvRepository.LoadAll(path))
            {
                try
                {
                    Add(property);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    Console.WriteLine($"Skipped invalid property data: {ex.Message}");
                }
            }
        }

        private static void EnsureCanBuy(User buyer)
        {
            ArgumentNullException.ThrowIfNull(buyer);
            if (!buyer.HasPermission(UserPermission.PurchaseProperties))
                throw new UnauthorizedAccessException("Only a Buyer account can purchase a property.");
        }

        private static void MarkSold(Property property)
        {
            if (property.Status != PropertyStatus.Available)
                throw new InvalidOperationException("That property has already been sold.");

            property.SetStatus(PropertyStatus.Sold);
        }
    }
}
