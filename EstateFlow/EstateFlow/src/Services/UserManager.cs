using EstateFlow.src.Models.Users;
namespace EstateFlow.src.Services
{
    public class UserManager
    {
        private readonly List<User> _users;

        public UserManager()
        {
            _users = new List<User>();
        }

        public void AddUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (_users.Any(u => string.Equals(u.UserId, user.UserId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A user with this ID already exists.");
            }

            _users.Add(user);
        }

        public User? FindUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required.");
            }

            return _users.FirstOrDefault(u => string.Equals(u.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        public void DisplayUsers()
        {
            if (_users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }

            foreach (User user in _users)
            {
                Console.WriteLine("----------------------------");
                Console.WriteLine($"ID: {user.UserId}");
                Console.WriteLine($"Name: {user.FullName}");
                Console.WriteLine($"Role: {user.GetRoleName()}");
                Console.WriteLine($"Status: {user.Status}");
            }
            Console.WriteLine("----------------------------");
        }
    }
}
