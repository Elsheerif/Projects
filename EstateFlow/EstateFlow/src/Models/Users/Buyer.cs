using EstateFlow.src.Models.Enums;
using System.Linq;

namespace EstateFlow.src.Models.Users
{
    public class Buyer : User
    {
        private readonly List<string> _favoritePropertyIds;

        private static readonly IReadOnlyCollection<UserPermission>
            _permissions = Array.AsReadOnly(new[]
            {
                UserPermission.BrowseProperties,
                UserPermission.RequestViewing
            });

        public IReadOnlyList<string> FavoritePropertyIds
        {
            get { return _favoritePropertyIds.AsReadOnly(); }
        }

        public Buyer(string userId, string fullName) : base(userId, fullName)
        {
            _favoritePropertyIds = new List<string>();
        }

        public override IReadOnlyCollection<UserPermission> Permissions
        {
            get { return _permissions; }
        }

        public override string GetRoleName()
        {
            return "Buyer";
        }

        public void AddFavorite(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
            {
                throw new ArgumentException("Invalid Property ID.");
            }
            if (_favoritePropertyIds.Contains(propertyId))
            {
                throw new InvalidOperationException("Property is already in favorites.");
            }

            _favoritePropertyIds.Add(propertyId);
        }

        public void RemoveFavorite(string propertyId)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
            {
                throw new ArgumentException("Invalid Property ID.");
            }
            if (!_favoritePropertyIds.Remove(propertyId))
            {
                throw new InvalidOperationException("Property was not found in favorites.");
            }
        }
    }
}
