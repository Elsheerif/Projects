using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Models.Users;
using EstateFlow.src.Services;
using System.Globalization;

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
            int choice = 0;
            do
            {
                Console.WriteLine("\n================================");
                Console.WriteLine("          Property Menu");
                Console.WriteLine("================================");
                Console.WriteLine("1. Add a new item");
                Console.WriteLine("2. View all items");
                Console.WriteLine("3. Search by name or keyword");
                Console.WriteLine("4. Update an item's status");
                Console.WriteLine("5. Remove an item");
                Console.WriteLine("6. View available listings");
                Console.WriteLine("Type 'I want to buy <property name>' to buy an available listing.");
                Console.WriteLine("7. Exit");

                string menuInput = ReadText("Enter your choice: ");
                if (menuInput.StartsWith("I want to buy ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string propertyName = menuInput["I want to buy ".Length..].Trim();
                        manager.BuyByName(propertyName, currentUser);
                        manager.SaveAll(manager.Properties.ToList(), PropertiesFilePath);
                        Console.WriteLine($"'{propertyName}' has been marked as sold.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not complete the purchase: {ex.Message}");
                    }
                    continue;
                }

                if (!int.TryParse(menuInput, out choice))
                {
                    Console.WriteLine("Invalid input. Enter a menu number or a purchase command.");
                    choice = 0;
                    continue;
                }

                try
                {
                    switch (choice)
                    {
                        case 1:
                            AddProperty(manager);
                            break;
                        case 2:
                            manager.Display();
                            break;
                        case 3:
                            SearchProperties(manager);
                            break;
                        case 4:
                            UpdateStatus(manager, currentUser);
                            break;
                        case 5:
                            RemoveProperty(manager);
                            break;
                        case 6:
                            manager.DisplayAvailable();
                            break;
                        case 7:
                            Console.WriteLine("Thank you for using EstateFlow!");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please select a number from 1 to 7.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not complete that action: {ex.Message}");
                }
            } while (choice != 7);
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
            if (!TryReadAmount("Total cost: ", out decimal totalCost) || totalCost <= 0)
            {
                Console.WriteLine("Total cost must be a number greater than zero. No property was added.");
                return;
            }
            if (!TryReadAmount("Amount already spent: ", out decimal amountSpent) || amountSpent < 0 || amountSpent > totalCost)
            {
                Console.WriteLine("Amount spent must be between zero and the total cost. No property was added.");
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
            manager.SaveAll(manager.Properties.ToList(), PropertiesFilePath);
            Console.WriteLine("Property added. Its availability is Available.");
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

        private static void UpdateStatus(PropertyManager manager, User currentUser)
        {
            string id = ReadText("Enter the property ID: ").Trim();
            if (currentUser is SystemAdmin)
            {
                Console.WriteLine("1. Available  2. Sold");
                if (!TryReadNumber("New status: ", out int statusChoice) || statusChoice is < 1 or > 2)
                {
                    Console.WriteLine("Invalid status. No change was made.");
                    return;
                }

                PropertyStatus status = statusChoice == 1 ? PropertyStatus.Available : PropertyStatus.Sold;
                manager.UpdateStatus(id, status, currentUser);
            }
            else
            {
                Console.WriteLine("Only a System Admin can restore a sold listing. You may mark a property Sold.");
                manager.UpdateStatus(id, PropertyStatus.Sold, currentUser);
            }

            manager.SaveAll(manager.Properties.ToList(), PropertiesFilePath);
            Console.WriteLine("Property status updated.");
        }

        private static void RemoveProperty(PropertyManager manager)
        {
            string id = ReadText("Enter the property ID to remove: ").Trim();
            manager.Remove(id);
            manager.SaveAll(manager.Properties.ToList(), PropertiesFilePath);
            Console.WriteLine("Property removed.");
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
    }
}
