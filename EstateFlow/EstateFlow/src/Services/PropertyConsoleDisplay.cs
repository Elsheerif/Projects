using EstateFlow.src.Models.Properties;
using System.Globalization;

namespace EstateFlow.src.Services
{
    public static class PropertyConsoleDisplay
    {
        public static void Display(IEnumerable<Property> properties)
        {
            List<Property> results = properties.ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("No properties found.");
                return;
            }

            foreach (Property property in results)
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
    }
}
