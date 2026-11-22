using EstateFlow.src.Models.Enums;

namespace EstateFlow.src.Models.Users
{
    public abstract class User
    {
        private string _userId;
        private string _fullName;
        private UserStatus _status;

        public string UserId
        {
            get { return _userId; }
        }

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be empty.");

                _fullName = value;
            }
        }

        public UserStatus Status
        {
            get { return _status; }
        }

        public abstract IReadOnlyCollection<UserPermission> Permissions
        {
            get;
        }

        public User(string userId, string fullName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required.");

            _userId = userId;
            FullName = fullName;
            _status = UserStatus.Active;
        }

        public bool HasPermission(UserPermission permission)
        {
            return Permissions.Contains(permission);
        }

        public virtual void Activate()
        {
            _status = UserStatus.Active;
        }

        public virtual void Suspend()
        {
            _status = UserStatus.Suspended;
        }

        public abstract string GetRoleName();
    }
}
