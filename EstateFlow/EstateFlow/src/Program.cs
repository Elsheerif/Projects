using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;
using System.Globalization;
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
        private enum PropertyMenuAction
        {
            AddProperty,
            ViewAll,
            Search,
            Buy,
            ViewAvailable,
            RestoreSold,
            Exit
        }

        UserManager userManager = new UserManager();
        AuthorizationService authorizationService = new AuthorizationService();

        private static readonly string PropertiesFilePath = Path.Combine(FindProjectDirectory(), "properties.csv");

        private static void Main(string[] args)
        {
            try
            {
                UserManager userManager = CreateUserManager();
                PropertyManager propertyManager = new();
                propertyManager.LoadAll(PropertiesFilePath);

                User? currentUser = SelectAccount(userManager);
                if (currentUser != null)
                    RunPropertyMenu(propertyManager, currentUser);
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

                if (!TryReadNumber("Enter your choice: ", out choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    choice = 0;
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        userManager.DisplayUsers();
                        string userId = ReadText("Enter your User ID: ");
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

        private static void RunPropertyMenu(PropertyManager manager, User currentUser)
        {
            bool isAdmin = currentUser is SystemAdmin;
            List<PropertyMenuAction> menuActions = new();
            if (currentUser is Owner or SystemAdmin)
                menuActions.Add(PropertyMenuAction.AddProperty);
            menuActions.Add(PropertyMenuAction.ViewAll);
            menuActions.Add(PropertyMenuAction.Search);
            if (currentUser is Buyer)
                menuActions.Add(PropertyMenuAction.Buy);
            menuActions.Add(PropertyMenuAction.ViewAvailable);
            if (isAdmin)
                menuActions.Add(PropertyMenuAction.RestoreSold);
            menuActions.Add(PropertyMenuAction.Exit);

            bool exitRequested = false;
            do
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("          Property Menu");
                Console.WriteLine("================================");
                for (int index = 0; index < menuActions.Count; index++)
                    Console.WriteLine($"{index + 1}. {GetMenuLabel(menuActions[index])}");
                if (currentUser is Buyer)
                    Console.WriteLine("You can also type 'I want to buy <property name>'.");

                string menuInput = ReadText("Enter your choice: ");
                if (currentUser is Buyer && menuInput.StartsWith("I want to buy ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string propertyName = menuInput["I want to buy ".Length..].Trim();
                        BuyProperty(manager, currentUser, propertyName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not complete the purchase: {ex.Message}");
                    }
                    continue;
                }
                if (menuInput.StartsWith("I want to buy ", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Purchases can only be made from a Buyer account.");
                    continue;
                }

                if (!int.TryParse(menuInput, out int choice) || choice < 1 || choice > menuActions.Count)
                {
                    Console.WriteLine($"Invalid input. Enter a number from 1 to {menuActions.Count}.");
                    continue;
                }

                PropertyMenuAction action = menuActions[choice - 1];
                try
                {
                    switch (action)
                    {
                        case PropertyMenuAction.AddProperty:
                            AddProperty(manager);
                            break;
                        case PropertyMenuAction.ViewAll:
                            manager.Display();
                            break;
                        case PropertyMenuAction.Search:
                            SearchProperties(manager);
                            break;
                        case PropertyMenuAction.Buy:
                            BuyProperty(manager, currentUser);
                            break;
                        case PropertyMenuAction.ViewAvailable:
                            manager.DisplayAvailable();
                            break;
                        case PropertyMenuAction.RestoreSold:
                            RestoreSoldProperty(manager, currentUser);
                            break;
                        case PropertyMenuAction.Exit:
                            Console.WriteLine("Thank you for using EstateFlow!");
                            exitRequested = true;
                            break;
                        default:
                            Console.WriteLine("Invalid menu action.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not complete that action: {ex.Message}");
                }
            } while (!exitRequested);
        }

        private static string GetMenuLabel(PropertyMenuAction action)
        {
            return action switch
            {
                PropertyMenuAction.AddProperty => "Add a new property",
                PropertyMenuAction.ViewAll => "View all properties",
                PropertyMenuAction.Search => "Search by name or keyword",
                PropertyMenuAction.Buy => "Buy a property",
                PropertyMenuAction.ViewAvailable => "View available properties",
                PropertyMenuAction.RestoreSold => "Restore a sold property",
                PropertyMenuAction.Exit => "Exit",
                _ => "Unknown action"
            };
        }

        private static void AddProperty(PropertyManager manager)
        {
            Console.WriteLine("\nChoose a property type:");
            Console.WriteLine("1. Apartment  2. Villa  3. Duplex  4. Penthouse  5. Office  6. Retail Shop");
            if (!TryReadNumber("Type: ", out int typeChoice) || typeChoice < 1 || typeChoice > 6)
            {
                Console.WriteLine("Invalid property type. No property was added.");
                return;
            }

            string id = ReadText("ID: ").Trim();
            string name = ReadText("Name: ").Trim();
            string address = ReadText("Address: ").Trim();
            if (!TryReadAmount("Total cost: ", out decimal totalCost))
            {
                Console.WriteLine("Total cost must be a valid number. No property was added.");
                return;
            }
            try
            {
                manager.ValidateFinancials(totalCost, 0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"{ex.Message} No property was added.");
                return;
            }

            if (!TryReadAmount("Amount already spent: ", out decimal amountSpent))
            {
                Console.WriteLine("Amount spent must be a valid number. No property was added.");
                return;
            }
            try
            {
                manager.ValidateFinancials(totalCost, amountSpent);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"{ex.Message} No property was added.");
                return;
            }

            string developer = ReadText("Real estate developer: ").Trim();
            string description = ReadText("Description: ").Trim();

            Property property = typeChoice switch
            {
                1 => new Apartment(id, name, address, totalCost, amountSpent, developer, description),
                2 => new Villa(id, name, address, totalCost, amountSpent, developer, description),
                3 => new Duplex(id, name, address, totalCost, amountSpent, developer, description),
                4 => new Penthouse(id, name, address, totalCost, amountSpent, developer, description),
                5 => new Office(id, name, address, totalCost, amountSpent, developer, description),
                6 => new RetailShop(id, name, address, totalCost, amountSpent, developer, description),
                _ => throw new InvalidOperationException("Property type is invalid.")
            };

            manager.Add(property);
            SaveChanges(manager, "Property added. Its availability is Available.");
        }

        private static void SearchProperties(PropertyManager manager)
        {
            string keyword = ReadText("Enter a name or keyword: ");
            IReadOnlyList<Property> matches = manager.FindAll(keyword);
            if (matches.Count == 0)
                Console.WriteLine("No matching properties found.");
            else
                DisplayProperties(matches);
        }

        private static void DisplayProperties(IEnumerable<Property> properties)
        {
            foreach (Property property in properties)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"ID: {property.Id}");
                Console.WriteLine($"Type: {property.GetPropertyType()}");
                Console.WriteLine($"Name: {property.Name}");
                Console.WriteLine($"Address: {property.Address}");
                Console.WriteLine($"Total cost: {property.TotalCost.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Amount spent: {property.AmountSpent.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Remaining: {property.Remaining.ToString("F2", CultureInfo.InvariantCulture)}");
                Console.WriteLine($"Real estate developer: {property.Developer}");
                Console.WriteLine($"Availability: {property.Status}");
                Console.WriteLine($"Description: {property.Description}");
            }
            Console.WriteLine("----------------------------------------");
        }

        private static void BuyProperty(PropertyManager manager, User currentUser, string? requestedName = null)
        {
            string propertyName = string.IsNullOrWhiteSpace(requestedName)
                ? ReadText("Enter the property name to buy: ").Trim()
                : requestedName.Trim();

            IReadOnlyList<Property> matches = manager.FindByExactName(propertyName);
            if (matches.Count > 1)
            {
                Console.WriteLine("Several properties have that name. Select one by ID:");
                DisplayProperties(matches);
                string id = ReadText("Property ID to buy: ").Trim();
                manager.BuyById(id, currentUser);
            }
            else
            {
                manager.BuyByName(propertyName, currentUser);
            }

            SaveChanges(manager, $"'{propertyName}' has been marked as sold.");
        }

        private static void RestoreSoldProperty(PropertyManager manager, User currentUser)
        {
            string id = ReadText("Enter the sold property ID to restore: ").Trim();
            manager.UpdateStatus(id, PropertyStatus.Available, currentUser);
            SaveChanges(manager, "Property is Available again.");
        }

        private static void SaveChanges(PropertyManager manager, string successMessage)
        {
            if (manager.SaveAll(manager.Properties.ToList(), PropertiesFilePath))
            {
                Console.WriteLine(successMessage);
                return;
            }

            Console.WriteLine("The change is in memory but was not saved. It may be lost when the app closes.");
        }

        private static bool TryReadAmount(string prompt, out decimal amount)
        {
            string input = ReadText(prompt);
            return decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) ||
                decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        private static bool TryReadNumber(string prompt, out int number)
        {
            return int.TryParse(ReadText(prompt), out number);
        }

        private static string ReadText(string prompt)
        {
            Console.Write(prompt);
            try
            {
                return Console.ReadLine() ?? string.Empty;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Could not read input: {ex.Message}");
                return string.Empty;
            }
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

        //ShowUserMenu(currentUser, authorizationService); 
        //                }
        //                catch (ArgumentException ex)
        //                {
        //                    Console.WriteLine($"Error: {ex.Message}");
        //                }

    }
}
