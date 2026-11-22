using EstateFlow.src.Models.Enums;

namespace EstateFlow.src.Models.Properties
{
    public abstract class Property
    {
        private readonly string _id;
        private readonly string _name;
        private readonly string _address;
        private readonly decimal _totalCost;
        private readonly decimal _amountSpent;
        private readonly string _developer;
        private readonly string _description;
        private PropertyStatus _status;

        public string Id => _id;
        public string Name => _name;
        public string Address => _address;
        public decimal TotalCost => _totalCost;
        public decimal AmountSpent => _amountSpent;
        public decimal Remaining => _totalCost - _amountSpent;
        public string Developer => _developer;
        public string Description => _description;
        public PropertyStatus Status => _status;

        protected Property(
            string id,
            string name,
            string address,
            decimal totalCost,
            decimal amountSpent,
            string developer,
            string description,
            PropertyStatus status = PropertyStatus.Available)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Property ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Property name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Property address is required.", nameof(address));
            if (totalCost <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalCost), "Total cost must be greater than zero.");
            if (amountSpent < 0 || amountSpent > totalCost)
                throw new ArgumentOutOfRangeException(nameof(amountSpent), "Amount spent must be between zero and the total cost.");
            if (string.IsNullOrWhiteSpace(developer))
                throw new ArgumentException("Real estate developer is required.", nameof(developer));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (!Enum.IsDefined(status))
                throw new ArgumentOutOfRangeException(nameof(status), "Property status is invalid.");

            _id = id.Trim();
            _name = name.Trim();
            _address = address.Trim();
            _totalCost = totalCost;
            _amountSpent = amountSpent;
            _developer = developer.Trim();
            _description = description.Trim();
            _status = status;
        }

        public abstract string GetPropertyType();

        internal void SetStatus(PropertyStatus status)
        {
            _status = status;
        }
    }
}
