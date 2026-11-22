using EstateFlow.src.Models.Users;
using System.Linq;

namespace EstateFlow.src.Services
{
    public class UserManager
    {
        private readonly List<User> _users;

        public IReadOnlyList<User> Users
        {
            get { return _users.AsReadOnly(); }
        }

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

        public void RemoveUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required.");
            }

            User? user = FindUserById(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User was not found.");
            }
            if (user is SystemAdmin)
            {
                throw new InvalidOperationException("System Admin cannot be removed.");
            }

            _users.Remove(user);
        }

        public User? FindUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required.");
            }

            return _users.FirstOrDefault(u => string.Equals(u.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyCollection<User> FindUsersByFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name is required.");
            }

            return _users.Where(u => string.Equals(
                    u.FullName.Trim(),
                    fullName.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
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
