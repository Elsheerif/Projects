using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;

namespace EstateFlow.src
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserManager userManager = new UserManager();

            userManager.AddUser(new SystemAdmin("A001", "System Administrator"));
            userManager.AddUser(new Owner("U001", "Ahmed Hassan"));
            userManager.AddUser(new Buyer("U002", "Mariam Ali"));

            User? currentUser = null;
            int choice;

            do
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("       Welcome to EstateFlow");
                Console.WriteLine("================================");
                Console.WriteLine("1. Select Account");
                Console.WriteLine("2. Exit");
                Console.Write("Enter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    choice = 0;
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("\n--- Available Accounts ---");

                        userManager.DisplayUsers();

                        Console.Write("\nEnter your User ID: ");
                        string? selectedUserId = Console.ReadLine()?.Trim().ToLower();

                        if (string.IsNullOrWhiteSpace(selectedUserId))
                        {
                            Console.WriteLine("User ID cannot be empty.");
                            break;
                        }

                        try
                        {
                            currentUser = userManager.FindUserById(selectedUserId);

                            if (currentUser == null)
                            {
                                Console.WriteLine("User not found. Please try again.");
                                break;
                            }

                            if (currentUser.Status == UserStatus.Suspended)
                            {
                                Console.WriteLine(
                                    "This account is suspended. Access denied.");

                                currentUser = null;
                                break;
                            }

                            Console.WriteLine("\nAccount selected successfully!");
                            Console.WriteLine($"Welcome, {currentUser.FullName}!");
                            Console.WriteLine($"Role: {currentUser.GetRoleName()}");

                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }

                        break;

                    case 2:
                        Console.WriteLine("\nThank you for using EstateFlow!");
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please select 1 or 2.");
                        break;
                }

            } while (choice != 2);
        }
    }
}
