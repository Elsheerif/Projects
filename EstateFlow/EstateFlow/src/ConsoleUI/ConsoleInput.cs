using System.Globalization;

namespace EstateFlow.src.ConsoleUI
{
    internal static class ConsoleInput
    {
        public static bool TryReadAmount(string prompt, out decimal amount)
        {
            string input = ReadText(prompt);
            return decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) ||
                decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        public static bool TryReadNumber(string prompt, out int number)
        {
            return int.TryParse(ReadText(prompt), out number);
        }

        public static string ReadText(string prompt)
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
    }
}
