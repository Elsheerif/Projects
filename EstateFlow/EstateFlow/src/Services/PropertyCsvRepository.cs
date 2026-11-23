using EstateFlow.src.Models.Enums;
using EstateFlow.src.Models.Properties;
using EstateFlow.src.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace EstateFlow.src.Services
{
    public class PropertyCsvRepository : IPropertyRepository
    {
        public bool SaveAll(List<Property> properties, string path)
        {
            ArgumentNullException.ThrowIfNull(properties);
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A file path is required.", nameof(path));

            string? temporaryPath = null;
            try
            {
                string fullPath = Path.GetFullPath(path);
                temporaryPath = fullPath + ".tmp";
                using (StreamWriter writer = new(temporaryPath, false, new UTF8Encoding(false)))
                {
                    foreach (Property property in properties)
                    {
                        ArgumentNullException.ThrowIfNull(property);
                        string[] columns =
                        {
                            property.Id,
                            property.GetPropertyType(),
                            property.Name,
                            property.Address,
                            property.TotalCost.ToString(CultureInfo.InvariantCulture),
                            property.AmountSpent.ToString(CultureInfo.InvariantCulture),
                            property.Remaining.ToString(CultureInfo.InvariantCulture),
                            property.Developer,
                            property.Status.ToString(),
                            property.Description
                        };
                        writer.WriteLine(string.Join(",", columns.Select(EscapeCsv)));
                    }
                    writer.Flush();
                }

                File.Move(temporaryPath, fullPath, true);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                Console.WriteLine($"Could not save property data: {ex.Message}");
                return false;
            }
            finally
            {
                if (temporaryPath != null && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Could not remove temporary save file: {ex.Message}");
                    }
                }
            }
        }

        public List<Property> LoadAll(string path)
        {
            List<Property> properties = new();
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Could not load property data: a file path is required.");
                return properties;
            }

            if (!File.Exists(path))
                return properties;

            try
            {
                using StreamReader reader = new(path, Encoding.UTF8);
                string? line;
                int lineNumber = 0;
                int recordStartLine = 0;
                StringBuilder record = new();
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (record.Length == 0 && string.IsNullOrWhiteSpace(line))
                        continue;

                    if (record.Length == 0)
                        recordStartLine = lineNumber;
                    else
                        record.Append('\n');

                    record.Append(line);
                    if (HasOpenQuotedField(record))
                        continue;

                    try
                    {
                        properties.Add(ParseProperty(ParseCsvLine(record.ToString())));
                    }
                    catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is InvalidOperationException)
                    {
                        Console.WriteLine($"Skipped invalid property data on line {recordStartLine}: {ex.Message}");
                    }

                    record.Clear();
                }

                if (record.Length > 0)
                {
                    Console.WriteLine($"Skipped invalid property data on line {recordStartLine}: A quoted field was not closed.");
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                properties.Clear();
                Console.WriteLine($"Could not load property data: {ex.Message}");
            }

            return properties;
        }

        private static Property ParseProperty(IReadOnlyList<string> columns)
        {
            if (columns.Count != 10)
                throw new FormatException("Expected ten comma-separated fields.");

            string id = columns[0];
            string type = columns[1];
            string name = columns[2];
            string address = columns[3];
            decimal totalCost = decimal.Parse(columns[4], NumberStyles.Number, CultureInfo.InvariantCulture);
            decimal amountSpent = decimal.Parse(columns[5], NumberStyles.Number, CultureInfo.InvariantCulture);
            decimal remaining = decimal.Parse(columns[6], NumberStyles.Number, CultureInfo.InvariantCulture);
            if (remaining != totalCost - amountSpent)
                throw new FormatException("Remaining cost does not match total cost minus amount spent.");
            string developer = columns[7];
            if (!Enum.TryParse(columns[8], true, out PropertyStatus status) || !Enum.IsDefined(status))
                throw new FormatException("Availability value is invalid.");

            return PropertyFactory.CreateFromName(type, id, name, address, totalCost, amountSpent, developer, columns[9], status);
        }

        private static string EscapeCsv(string value)
        {
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
                return value;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> columns = new();
            StringBuilder value = new();
            bool insideQuotes = false;

            for (int index = 0; index < line.Length; index++)
            {
                char current = line[index];
                if (current == '"')
                {
                    if (insideQuotes && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (current == ',' && !insideQuotes)
                {
                    columns.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(current);
                }
            }

            if (insideQuotes)
                throw new FormatException("A quoted field was not closed.");

            columns.Add(value.ToString());
            return columns;
        }

        private static bool HasOpenQuotedField(StringBuilder record)
        {
            bool insideQuotes = false;
            for (int index = 0; index < record.Length; index++)
            {
                if (record[index] != '"')
                    continue;

                if (insideQuotes && index + 1 < record.Length && record[index + 1] == '"')
                {
                    index++;
                    continue;
                }

                insideQuotes = !insideQuotes;
            }

            return insideQuotes;
        }
    }
}
