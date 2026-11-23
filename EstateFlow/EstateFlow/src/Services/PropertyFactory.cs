using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;

namespace EstateFlow.src.Services
{
    public static class PropertyFactory
    {
        public static Property Create(
            PropertyType type,
            string id,
            string name,
            string address,
            decimal totalCost,
            decimal amountSpent,
            string developer,
            string description,
            PropertyStatus status = PropertyStatus.Available)
        {
            return type switch
            {
                PropertyType.Apartment => new Apartment(id, name, address, totalCost, amountSpent, developer, description, status),
                PropertyType.Villa => new Villa(id, name, address, totalCost, amountSpent, developer, description, status),
                PropertyType.Duplex => new Duplex(id, name, address, totalCost, amountSpent, developer, description, status),
                PropertyType.Penthouse => new Penthouse(id, name, address, totalCost, amountSpent, developer, description, status),
                PropertyType.Office => new Office(id, name, address, totalCost, amountSpent, developer, description, status),
                PropertyType.RetailShop => new RetailShop(id, name, address, totalCost, amountSpent, developer, description, status),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Property type is invalid.")
            };
        }

        public static Property CreateFromName(
            string typeName,
            string id,
            string name,
            string address,
            decimal totalCost,
            decimal amountSpent,
            string developer,
            string description,
            PropertyStatus status)
        {
            string normalizedType = string.Concat(typeName.Where(char.IsLetterOrDigit));
            if (!Enum.TryParse(normalizedType, true, out PropertyType type) || !Enum.IsDefined(type))
                throw new FormatException($"Unknown property type '{typeName}'.");

            return Create(type, id, name, address, totalCost, amountSpent, developer, description, status);
        }
    }
}
