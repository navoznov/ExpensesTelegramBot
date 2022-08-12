using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            return GetExpensesFromFile(fileName)
                .OrderBy(e => e.Date)
                .ToArray();
        }

        public Expense[] GetLastExpenses(int count)
        {
            var fileNames = new DirectoryInfo(".").GetFiles().Select(fi => fi.Name).ToArray();
            
            var regex = new Regex(@"^\d\d\d\d-\d\d\.csv$");
            var test = regex.IsMatch("1233-34.csv");
            var matchedFileNames = fileNames.Where(fn => regex.IsMatch(fn))
                .OrderBy(fn => fn)
                .ToArray();
            var result = new List<Expense>();
            foreach (var fileName in matchedFileNames)
            {
                var allFileRecords = GetExpensesFromFile(fileName);
                var expensesToResult = allFileRecords
                    .OrderByDescending(e => e.Date)
                    .Take(count - result.Count);
                result.AddRange(expensesToResult);
                if (result.Count == count)
                {
                    break;
                }
            }

            return result.ToArray();
        }

        Expense[] GetExpensesFromFile(string fileName)
        {
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, _csvConfiguration);
            return csv.GetRecords<Expense>().ToArray();
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