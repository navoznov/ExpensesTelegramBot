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
        private const string CSV_FILES_STORAGE_FOLDER_NAME = "data";
        
        private readonly CsvConfiguration _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false,
        };

        public CsvExpensesRepository()
        {
            if (!Directory.Exists(CSV_FILES_STORAGE_FOLDER_NAME))
            {
                Directory.CreateDirectory(CSV_FILES_STORAGE_FOLDER_NAME);
            }
        }

        public void Save(Expense expense)
        {
            var fileName = GetFileName(expense.Date.Year, expense.Date.Month);
            var filePath = GetFilePath(fileName);
            using var stream = File.Open(filePath, FileMode.Append);
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

        private static string GetFileName(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            return $"{date:yyyy-MM}.csv";
        }
        
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(CSV_FILES_STORAGE_FOLDER_NAME, fileName);
        }

        private Expense[] GetExpensesFromFile(string fileName)
        {
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, _csvConfiguration);
            return csv.GetRecords<Expense>().ToArray();
        }
    }
}