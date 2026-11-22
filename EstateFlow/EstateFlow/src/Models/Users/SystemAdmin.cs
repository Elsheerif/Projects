using EstateFlow.src.Models.Enums;

namespace EstateFlow.src.Models.Users
{
    public class SystemAdmin : User
    {
        private static readonly IReadOnlyCollection<UserPermission>
            _permissions = Array.AsReadOnly(new[]
            {
                UserPermission.BrowseProperties,
                UserPermission.ManageAllProperties,
                UserPermission.ManageViewingRequests,
                UserPermission.ApproveListings,
                UserPermission.ManageUsers,
                UserPermission.CompleteTransactions,
                UserPermission.ViewReports
            });

        public SystemAdmin(string userId, string fullName) : base(userId, fullName)
        {
        }

        public override IReadOnlyCollection<UserPermission> Permissions
        {
            get { return _permissions; }
        }

        public override string GetRoleName()
        {
            return "System Admin";
        }

        public override void Suspend()
        {
            throw new InvalidOperationException("The system admin account cannot be suspended.");
        }
    }
}
