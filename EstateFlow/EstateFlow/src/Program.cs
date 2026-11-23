using EstateFlow.src.ConsoleUI;
using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;
using EstateFlow.src.Services.Interfaces;
using System;
using System.IO;

namespace EstateFlow.src
{
    internal class Program
    {
        private static readonly string PropertiesFilePath = Path.Combine(FindProjectDirectory(), "properties.csv");

        private static void Main(string[] args)
        {
            try
            {
                UserManager userManager = CreateUserManager();
                IPropertyRepository propertyRepository = new PropertyCsvRepository();
                PropertyManager propertyManager = new(propertyRepository);
                propertyManager.LoadAll(PropertiesFilePath);

                while (true)
                {
                    User? currentUser = SelectAccount(userManager);
                    if (currentUser == null)
                        break;

                    PropertyMenu propertyMenu = new(propertyManager, currentUser, PropertiesFilePath);
                    if (!propertyMenu.Run())
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EstateFlow could not continue: {ex.Message}");
            }
        }

        private static UserManager CreateUserManager()
        {
            UserManager userManager = new();
            userManager.AddUser(new SystemAdmin("A001", "System Administrator"));
            userManager.AddUser(new Owner("U001", "Ahmed Hassan"));
            userManager.AddUser(new Buyer("U002", "Mariam Ali"));
            return userManager;
        }

        private static User? SelectAccount(UserManager userManager)
        {
            int choice = 0;
            do
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("       Welcome to EstateFlow");
                Console.WriteLine("================================");
                Console.WriteLine("1. Select Account");
                Console.WriteLine("2. Exit");

                if (!ConsoleInput.TryReadNumber("Enter your choice: ", out choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    choice = 0;
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        userManager.DisplayUsers();
                        string userId = ConsoleInput.ReadText("Enter your User ID: ");
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            Console.WriteLine("User ID cannot be empty.");
                            break;
                        }

                        User? selectedUser = userManager.FindUserById(userId.Trim());
                        if (selectedUser == null)
                        {
                            Console.WriteLine("User not found. Please try again.");
                            break;
                        }
                        if (selectedUser.Status == UserStatus.Suspended)
                        {
                            Console.WriteLine("This account is suspended. Access denied.");
                            break;
                        }

                        Console.WriteLine($"\nWelcome, {selectedUser.FullName}! ({selectedUser.GetRoleName()})");
                        return selectedUser;

                    case 2:
                        Console.WriteLine("Thank you for using EstateFlow!");
                        return null;

                    default:
                        Console.WriteLine("Invalid choice. Please select 1 or 2.");
                        break;
                }
            } while (choice != 2);

            return null;
        }

        private static string FindProjectDirectory()
        {
            foreach (string startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                DirectoryInfo? directory = new(startPath);
                while (directory != null)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "EstateFlow.csproj")))
                        return directory.FullName;

                    directory = directory.Parent;
                }
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
