using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;

namespace EstateFlow.src.ConsoleUI
{
    internal sealed class PropertyMenu
    {
        private enum MenuAction
        {
            AddProperty,
            ViewAll,
            Search,
            Buy,
            ViewAvailable,
            ViewSold,
            RestoreSold,
            Logout,
            Exit
        }

        private readonly PropertyManager _manager;
        private readonly User _currentUser;
        private readonly string _propertiesFilePath;

        public PropertyMenu(PropertyManager manager, User currentUser, string propertiesFilePath)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _propertiesFilePath = propertiesFilePath ?? throw new ArgumentNullException(nameof(propertiesFilePath));
        }

        public bool Run()
        {
            bool canBrowseProperties = _currentUser.HasPermission(UserPermission.BrowseProperties);
            bool canAddProperties = _currentUser.HasPermission(UserPermission.ManageOwnProperties) ||
                _currentUser.HasPermission(UserPermission.ManageAllProperties);
            bool canBuyProperties = _currentUser.HasPermission(UserPermission.PurchaseProperties);
            bool canManageAllProperties = _currentUser.HasPermission(UserPermission.ManageAllProperties);
            List<MenuAction> menuActions = new();
            if (canAddProperties)
                menuActions.Add(MenuAction.AddProperty);
            if (canBrowseProperties)
            {
                menuActions.Add(MenuAction.ViewAll);
                menuActions.Add(MenuAction.Search);
            }
            if (canBuyProperties)
                menuActions.Add(MenuAction.Buy);
            if (canBrowseProperties)
                menuActions.Add(MenuAction.ViewAvailable);
            if (canManageAllProperties)
            {
                menuActions.Add(MenuAction.ViewSold);
                menuActions.Add(MenuAction.RestoreSold);
            }
            menuActions.Add(MenuAction.Logout);
            menuActions.Add(MenuAction.Exit);

            bool exitRequested = false;
            bool logoutRequested = false;
            do
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("          Property Menu");
                Console.WriteLine("================================");
                for (int index = 0; index < menuActions.Count; index++)
                    Console.WriteLine($"{index + 1}. {GetMenuLabel(menuActions[index])}");
                if (canBuyProperties)
                    Console.WriteLine("You can also type 'I want to buy <property name>'.");

                string menuInput = ConsoleInput.ReadText("Enter your choice: ");
                if (canBuyProperties && menuInput.StartsWith("I want to buy ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string propertyName = menuInput["I want to buy ".Length..].Trim();
                        BuyProperty(propertyName);
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

                try
                {
                    switch (menuActions[choice - 1])
                    {
                        case MenuAction.AddProperty:
                            AddProperty();
                            break;
                        case MenuAction.ViewAll:
                            _manager.Display();
                            break;
                        case MenuAction.Search:
                            SearchProperties();
                            break;
                        case MenuAction.Buy:
                            BuyProperty();
                            break;
                        case MenuAction.ViewAvailable:
                            _manager.DisplayAvailable();
                            break;
                        case MenuAction.ViewSold:
                            _manager.DisplaySold();
                            break;
                        case MenuAction.RestoreSold:
                            RestoreSoldProperty();
                            break;
                        case MenuAction.Logout:
                            Console.WriteLine("You have logged out. Choose another account.");
                            logoutRequested = true;
                            break;
                        case MenuAction.Exit:
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
            } while (!exitRequested && !logoutRequested);

            return logoutRequested;
        }

        private static string GetMenuLabel(MenuAction action)
        {
            return action switch
            {
                MenuAction.AddProperty => "Add a new property",
                MenuAction.ViewAll => "View all properties",
                MenuAction.Search => "Search by property type",
                MenuAction.Buy => "Buy a property",
                MenuAction.ViewAvailable => "View available properties",
                MenuAction.ViewSold => "View sold properties",
                MenuAction.RestoreSold => "Restore a sold property",
                MenuAction.Logout => "Log out",
                MenuAction.Exit => "Exit",
                _ => "Unknown action"
            };
        }

        private void AddProperty()
        {
            Console.WriteLine("\nChoose a property type:");
            Console.WriteLine("1. Apartment  2. Villa  3. Duplex  4. Penthouse  5. Office  6. Retail Shop");
            if (!ConsoleInput.TryReadNumber("Type: ", out int typeChoice) || typeChoice < 1 || typeChoice > 6)
            {
                Console.WriteLine("Invalid property type. No property was added.");
                return;
            }

            string id = ConsoleInput.ReadText("ID: ").Trim();
            string name = ConsoleInput.ReadText("Name: ").Trim();
            string address = ConsoleInput.ReadText("Address: ").Trim();
            if (!ConsoleInput.TryReadAmount("Total cost: ", out decimal totalCost))
            {
                Console.WriteLine("Total cost must be a valid number. No property was added.");
                return;
            }
            try
            {
                _manager.ValidateFinancials(totalCost, 0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"{ex.Message} No property was added.");
                return;
            }

            if (!ConsoleInput.TryReadAmount("Amount already spent: ", out decimal amountSpent))
            {
                Console.WriteLine("Amount spent must be a valid number. No property was added.");
                return;
            }
            try
            {
                _manager.ValidateFinancials(totalCost, amountSpent);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"{ex.Message} No property was added.");
                return;
            }

            string developer = ConsoleInput.ReadText("Real estate developer: ").Trim();
            string description = ConsoleInput.ReadText("Description: ").Trim();
            Property property = PropertyFactory.Create(
                (PropertyType)typeChoice, id, name, address, totalCost, amountSpent, developer, description);

            _manager.Add(property, _currentUser);
            SaveChanges("Property added. Its availability is Available.");
        }

        private void SearchProperties()
        {
            string keyword = ConsoleInput.ReadText("Enter a property type: ");
            IReadOnlyList<Property> matches = _manager.FindByType(keyword);
            if (matches.Count == 0)
                Console.WriteLine("No matching properties found.");
            else
                PropertyConsoleDisplay.Display(matches);
        }

        private void BuyProperty(string? requestedName = null)
        {
            string propertyName = string.IsNullOrWhiteSpace(requestedName)
                ? ConsoleInput.ReadText("Enter the property name to buy: ").Trim()
                : requestedName.Trim();

            IReadOnlyList<Property> matches = _manager.FindByExactName(propertyName);
            if (matches.Count == 0)
                throw new InvalidOperationException("No property with that name was found.");

            if (matches.Count > 1)
            {
                Console.WriteLine("Several properties have that name. Select one by ID:");
                PropertyConsoleDisplay.Display(matches);
                string id = ConsoleInput.ReadText("Property ID to buy: ").Trim();
                _manager.BuyById(id, _currentUser);
            }
            else
            {
                _manager.BuyById(matches[0].Id, _currentUser);
            }

            SaveChanges($"'{propertyName}' has been marked as sold.");
        }

        private void RestoreSoldProperty()
        {
            string id = ConsoleInput.ReadText("Enter the sold property ID to restore: ");
            _manager.RestoreSoldProperty(id, _currentUser);
            SaveChanges("Property is Available again.");
        }

        private void SaveChanges(string successMessage)
        {
            if (_manager.SaveAll(_manager.Properties.ToList(), _propertiesFilePath))
            {
                Console.WriteLine(successMessage);
                return;
            }

            Console.WriteLine("The change is in memory but was not saved. It may be lost when the app closes.");
        }
    }
}
