using EstateFlow.src.Models.Enums;
using System.Linq;

namespace EstateFlow.src.Models.Users
{
    public class Owner : User
    {
        private readonly List<string> _ownedPropertyIds;

        public IReadOnlyList<string> OwnedPropertyIds
        {
            get { return _ownedPropertyIds.AsReadOnly(); }
        }

        private static readonly IReadOnlyCollection<UserPermission>
            _permissions = Array.AsReadOnly(new[]
            {
                UserPermission.BrowseProperties,
                UserPermission.ManageOwnProperties,
                UserPermission.ManageViewingRequests
            });

        public Owner(string userId, string fullName) : base(userId, fullName)
        {
            _ownedPropertyIds = new List<string>();
        }

        public override IReadOnlyCollection<UserPermission> Permissions
        {
            get { return _permissions; }
        }

        public override string GetRoleName()
        {
            return "Owner";
        }

        public void AddOwnedProperty(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
            {
                throw new ArgumentException("Invalid Property ID.");
            }
            if (_ownedPropertyIds.Contains(propertyId))
            {
                throw new InvalidOperationException("Property is already associated with this owner.");

            }
            _ownedPropertyIds.Add(propertyId);
        }

        public void RemoveOwnedProperty(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
            {
                throw new ArgumentException("Invalid Property ID.");
            }
            if (!_ownedPropertyIds.Remove(propertyId))
            {
                throw new InvalidOperationException("Property was not found in owner's properties.");
            }
        }
    }
}
