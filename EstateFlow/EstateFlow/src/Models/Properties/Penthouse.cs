using EstateFlow.src.Models.Enums;

namespace EstateFlow.src.Models.Properties
{
    public sealed class Penthouse : Property
    {
        public Penthouse(string id, string name, string address, decimal totalCost, decimal amountSpent,
            string developer, string description, PropertyStatus status = PropertyStatus.Available)
            : base(id, name, address, totalCost, amountSpent, developer, description, status) { }

        public override string GetPropertyType() => "Penthouse";
    }
}
