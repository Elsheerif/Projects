using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Models.Users;
using System.Globalization;
using System.Text;

namespace EstateFlow.src.Services
{
    public class PropertyManager
    {
        private readonly List<Property> _properties = new();

        public IReadOnlyList<Property> Properties => _properties.AsReadOnly();

        public void Add(Property property)
        {
            ArgumentNullException.ThrowIfNull(property);
            ValidateFinancials(property.TotalCost, property.AmountSpent);
            if (!Enum.IsDefined(property.Status))
                throw new ArgumentOutOfRangeException(nameof(property), "Property status is invalid.");
            if (_properties.Any(existing => string.Equals(existing.Id, property.Id, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A property with this ID already exists.");

            _properties.Add(property);
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
                    property.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    property.Address.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    property.GetPropertyType().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    property.Developer.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    property.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
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

            if (status == PropertyStatus.Available && property.Status == PropertyStatus.Sold && actor is not SystemAdmin)
                throw new UnauthorizedAccessException("Only a System Admin can make a sold property available again.");

            property.SetStatus(status);
        }

        public void BuyByName(string name, User buyer)
        {
            ArgumentNullException.ThrowIfNull(buyer);
            if (buyer is not Buyer)
                throw new UnauthorizedAccessException("Only a Buyer account can purchase a property.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Enter the name of the property you want to buy.", nameof(name));

            IReadOnlyList<Property> matches = FindByExactName(name);

            if (matches.Count == 0)
                throw new InvalidOperationException("No property with that name was found.");
            if (matches.Count > 1)
                throw new InvalidOperationException("More than one property has that name. Search by name and use its ID to identify it.");

            BuyById(matches[0].Id, buyer);
        }

        public void BuyById(string id, User buyer)
        {
            ArgumentNullException.ThrowIfNull(buyer);
            if (buyer is not Buyer)
                throw new UnauthorizedAccessException("Only a Buyer account can purchase a property.");

            Property? selected = FindById(id);
            if (selected == null)
                throw new InvalidOperationException("Property was not found.");
            if (selected.Status != PropertyStatus.Available)
                throw new InvalidOperationException("That property has already been sold.");

            selected.SetStatus(PropertyStatus.Sold);
        }

        public void Display()
        {
            DisplayProperties(_properties);
        }

        public void DisplayAvailable()
        {
            DisplayProperties(_properties.Where(property => property.Status == PropertyStatus.Available));
        }

        private static void DisplayProperties(IEnumerable<Property> properties)
        {
            List<Property> results = properties.ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("No properties found.");
                return;
            }

            foreach (Property property in results)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"ID: {property.Id}");
                Console.WriteLine($"Type: {property.GetPropertyType()}");
                Console.WriteLine($"Name: {property.Name}");
                Console.WriteLine($"Address: {property.Address}");
                Console.WriteLine($"Total cost: {property.TotalCost.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Amount spent: {property.AmountSpent.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Remaining: {property.Remaining.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Real estate developer: {property.Developer}");
                Console.WriteLine($"Availability: {property.Status}");
                Console.WriteLine($"Description: {property.Description}");
            }
            Console.WriteLine("----------------------------------------");
        }

        public bool SaveAll(List<Property> properties, string path)
        {
            ArgumentNullException.ThrowIfNull(properties);
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A file path is required.", nameof(path));

            string? temporaryPath = null;
            try
            {
                string fullPath = Path.GetFullPath(path);
                temporaryPath = fullPath + ".tmp";
                using (StreamWriter writer = new(temporaryPath, false, new UTF8Encoding(false)))
                {
                    foreach (Property property in properties)
                    {
                        ArgumentNullException.ThrowIfNull(property);
                        string[] columns =
                        {
                            property.Id,
                            property.GetPropertyType(),
                            property.Name,
                            property.Address,
                            property.TotalCost.ToString(CultureInfo.InvariantCulture),
                            property.AmountSpent.ToString(CultureInfo.InvariantCulture),
                            property.Remaining.ToString(CultureInfo.InvariantCulture),
                            property.Developer,
                            property.Status.ToString(),
                            property.Description
                        };
                        writer.WriteLine(string.Join(",", columns.Select(EscapeCsv)));
                    }
                    writer.Flush();
                }
                File.Move(temporaryPath, fullPath, true);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                Console.WriteLine($"Could not save property data: {ex.Message}");
                return false;
            }
            finally
            {
                if (temporaryPath != null && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Could not remove temporary save file: {ex.Message}");
                    }
                }
            }
        }

        public List<Property> LoadAll(string path)
        {
            _properties.Clear();
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Could not load property data: a file path is required.");
                return new List<Property>(_properties);
            }

            if (!File.Exists(path))
                return new List<Property>(_properties);

            try
            {
                using StreamReader reader = new(path, Encoding.UTF8);
                string? line;
                int lineNumber = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        Property property = ParseProperty(ParseCsvLine(line));
                        Add(property);
                    }
                    catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is InvalidOperationException)
                    {
                        Console.WriteLine($"Skipped invalid property data on line {lineNumber}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                _properties.Clear();
                Console.WriteLine($"Could not load property data: {ex.Message}");
            }

            return new List<Property>(_properties);
        }

        private static Property ParseProperty(IReadOnlyList<string> columns)
        {
            if (columns.Count != 10)
                throw new FormatException("Expected ten comma-separated fields.");

            string id = columns[0];
            string type = columns[1];
            string name = columns[2];
            string address = columns[3];
            decimal totalCost = decimal.Parse(columns[4], NumberStyles.Number, CultureInfo.InvariantCulture);
            decimal amountSpent = decimal.Parse(columns[5], NumberStyles.Number, CultureInfo.InvariantCulture);
            decimal remaining = decimal.Parse(columns[6], NumberStyles.Number, CultureInfo.InvariantCulture);
            if (remaining != totalCost - amountSpent)
                throw new FormatException("Remaining cost does not match total cost minus amount spent.");
            string developer = columns[7];
            if (!Enum.TryParse(columns[8], true, out PropertyStatus status) || !Enum.IsDefined(status))
                throw new FormatException("Availability value is invalid.");
            string description = columns[9];

            return type.Trim().ToLowerInvariant() switch
            {
                "apartment" => new Apartment(id, name, address, totalCost, amountSpent, developer, description, status),
                "villa" => new Villa(id, name, address, totalCost, amountSpent, developer, description, status),
                "duplex" => new Duplex(id, name, address, totalCost, amountSpent, developer, description, status),
                "penthouse" => new Penthouse(id, name, address, totalCost, amountSpent, developer, description, status),
                "office" => new Office(id, name, address, totalCost, amountSpent, developer, description, status),
                "retail shop" => new RetailShop(id, name, address, totalCost, amountSpent, developer, description, status),
                _ => throw new FormatException($"Unknown property type '{type}'.")
            };
        }

        private static string EscapeCsv(string value)
        {
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
                return value;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> columns = new();
            StringBuilder value = new();
            bool insideQuotes = false;

            for (int index = 0; index < line.Length; index++)
            {
                char current = line[index];
                if (current == '"')
                {
                    if (insideQuotes && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (current == ',' && !insideQuotes)
                {
                    columns.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(current);
                }
            }

            if (insideQuotes)
                throw new FormatException("A quoted field was not closed.");

            columns.Add(value.ToString());
            return columns;
        }
    }
}
