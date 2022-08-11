using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Repositories
{
    public class CsvExpensesRepository : IExpensesRepository
    {
        private readonly CsvConfiguration _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false,
        };

        public void Save(Expense expense)
        {
            var fileName = GetFileName(expense.Date.Year, expense.Date.Month);
            using var stream = File.Open(fileName, FileMode.Append);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, _csvConfiguration);
            csv.WriteRecords(new[] {expense});
        }

        public Expense[] GetAll(int year, int month)
        {
            var fileName = GetFileName(year, month);
            if (!File.Exists(fileName))
            {
                return Array.Empty<Expense>();
            }

            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, _csvConfiguration);
            return csv.GetRecords<Expense>()
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date)
                .ToArray();
        }

        private static string GetFileName(int year, int month)
        {
            const string FILE_NAME_PATTERN = "yyyy-MM";
            var date = new DateTime(year, month, 1);
            var dateStr = date.ToString(FILE_NAME_PATTERN);
            var fileName = $"{dateStr}.csv";
            return fileName;
        }
    }
}