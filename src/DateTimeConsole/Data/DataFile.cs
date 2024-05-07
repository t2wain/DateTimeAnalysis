using CsvHelper;
using System.Globalization;

namespace DateTimeConsole.Data
{
    /// <summary>
    /// Reading event data saved in a file
    /// </summary>
    public static class DataFile
    {
        /// <summary>
        /// Read EventData from a file
        /// </summary>
        public static IEnumerable<EventData> ReadEventData(string fileName)
        {
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<EventData>();
            var data = records.ToList();
            return data;
        }
    }
}
