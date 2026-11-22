using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;
using System;

namespace EstateFlow.src
{
    internal class Program
    {
        private static void ShowUserMenu(User user, AuthorizationService authorizationService)
        {
            while (true)
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("          EstatePro Menu");
                Console.WriteLine("================================");
                Console.WriteLine($"Welcome, {user.FullName}");

                if (user is SystemAdmin)
                {
                    Console.WriteLine("\n--- System Admin Menu ---");
                    Console.WriteLine("1. Browse Properties");
                    Console.WriteLine("2. Manage All Properties");
                    Console.WriteLine("3. Manage Viewing Requests");
                    Console.WriteLine("4. Approve Listings");
                    Console.WriteLine("5. Manage Users");
                    Console.WriteLine("6. Complete Transactions");
                    Console.WriteLine("7. View Reports");
                }
                else if (user is Owner)
                {
                    Console.WriteLine("\n--- Owner Menu ---");
                    Console.WriteLine("1. Browse Properties");
                    Console.WriteLine("2. Manage Own Properties");
                    Console.WriteLine("3. Manage Viewing Requests");
                }
                else if (user is Buyer)
                {
                    Console.WriteLine("\n--- Buyer Menu ---");
                    Console.WriteLine("1. Browse Properties");
                    Console.WriteLine("2. Request a Viewing");
                }

                Console.WriteLine("0. Logout");
                Console.Write("\nEnter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                if (choice == 0)
                {
                    Console.WriteLine("You have logged out successfully.");
                    break;
                }

                UserPermission? requiredPermission = null;
                string actionName = "";


                if (user is SystemAdmin)
                {
                    switch (choice)
                    {
                        case 1:
                            requiredPermission = UserPermission.BrowseProperties;
                            actionName = "Browse Properties";
                            break;

                        case 2:
                            requiredPermission = UserPermission.ManageAllProperties;
                            actionName = "Manage All Properties";
                            break;

                        case 3:
                            requiredPermission = UserPermission.ManageViewingRequests;
                            actionName = "Manage Viewing Requests";
                            break;

                        case 4:
                            requiredPermission = UserPermission.ApproveListings;
                            actionName = "Approve Listings";
                            break;

                        case 5:
                            requiredPermission = UserPermission.ManageUsers;
                            actionName = "Manage Users";
                            break;

                        case 6:
                            requiredPermission = UserPermission.CompleteTransactions;
                            actionName = "Complete Transactions";
                            break;

                        case 7:
                            requiredPermission = UserPermission.ViewReports;
                            actionName = "View Reports";
                            break;
                    }
                }
                else if (user is Owner)
                {
                    switch (choice)
                    {
                        case 1:
                            requiredPermission = UserPermission.BrowseProperties;
                            actionName = "Browse Properties";
                            break;

                        case 2:
                            requiredPermission = UserPermission.ManageOwnProperties;
                            actionName = "Manage Own Properties";
                            break;

                        case 3:
                            requiredPermission = UserPermission.ManageViewingRequests;
                            actionName = "Manage Viewing Requests";
                            break;
                    }
                }
                else if (user is Buyer)
                {
                    switch (choice)
                    {
                        case 1:
                            requiredPermission = UserPermission.BrowseProperties;
                            actionName = "Browse Properties";
                            break;

                        case 2:
                            requiredPermission = UserPermission.RequestViewing;
                            actionName = "Request a Viewing";
                            break;
                    }
                }

                if (!requiredPermission.HasValue)
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                    continue;
                }

                if (authorizationService.HasPermission(user, requiredPermission.Value))
                {
                    Console.WriteLine($"\nAccess granted: {actionName}");
                    Console.WriteLine("This feature is not implemented yet.");
                }
                else
                {
                    Console.WriteLine("\nAccess denied. You don't have permission.");
                }
            }
        }

        static void Main(string[] args)
        {
            UserManager userManager = new UserManager();
            AuthorizationService authorizationService = new AuthorizationService();

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

                            ShowUserMenu(currentUser, authorizationService);
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
