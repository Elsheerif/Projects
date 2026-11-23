using EstateFlow.src.Models.Enums;

namespace EstateFlow.src.Models.Users
{
    public class Buyer : User
    {
        private static readonly IReadOnlyCollection<UserPermission>
            _permissions = Array.AsReadOnly(new[]
            {
                UserPermission.BrowseProperties,
                UserPermission.PurchaseProperties
            });

        public Buyer(string userId, string fullName) : base(userId, fullName)
        {
        }

        public override IReadOnlyCollection<UserPermission> Permissions
        {
            get { return _permissions; }
        }

        public override string GetRoleName()
        {
            return "Buyer";
        }
    }
}
