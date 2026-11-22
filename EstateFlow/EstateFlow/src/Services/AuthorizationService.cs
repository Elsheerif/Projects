using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Users;

namespace EstateFlow.src.Services
{
    public class AuthorizationService
    {
        public bool HasPermission(User user, UserPermission permission)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (user.Status != UserStatus.Active)
            {
                return false;
            }

            return user.Permissions.Contains(permission);
        }
    }
}
