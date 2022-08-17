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
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const char CSV_DELIMITER = ';';

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
            var expenseCsvString = GetExpenseCsvString(expense);
            using var stream = File.Open(filePath, FileMode.Append);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(expenseCsvString);
        }

        public Expense[] GetAll(int year, int month)
        {
            var fileName = GetFileName(year, month);
            var filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
            {
                return Array.Empty<Expense>();
            }

            var expenses = GetExpensesFromFile(filePath);
            return expenses
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date)
                .ToArray();
        }

        public Expense[] GetLastExpenses(int count)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(".", CSV_FILES_STORAGE_FOLDER_NAME));
            var fileNames = directoryInfo.GetFiles().Select(fi => fi.Name).ToArray();

            const string EXPENSES_DATA_FILE_PATTERN = @"^\d\d\d\d-\d\d\.csv$";
            var regex = new Regex(EXPENSES_DATA_FILE_PATTERN);
            var matchedFileNames = fileNames.Where(fn => regex.IsMatch(fn))
                .OrderByDescending(fn => fn)
                .ToArray();
            var result = new List<Expense>();
            foreach (var fileName in matchedFileNames)
            {
                var filePath = GetFilePath(fileName);
                var allFileRecords = GetExpensesFromFile(filePath);
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
            var lines = File.ReadAllLines(fileName);
            return lines.Select(ParseCsvExpense).ToArray();
        }

        Expense ParseCsvExpense(string line)
        {
            var fields = line.Split(CSV_DELIMITER);
            if (fields.Length < 2)
            {
                throw new Exception("Parsing error. Line must have at least 2 fields");
            }

            var dateStr = fields[0];
            if (!DateTime.TryParseExact(dateStr, DATE_FORMAT, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            {
                throw new Exception($"Parsing error. Invalid date format {dateStr}");
            }

            var moneyStr = fields[1];
            if (!decimal.TryParse(moneyStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var money))
            {
                throw new Exception($"Parsing error. Invalid money format {dateStr}");
            }

            var description = fields.Length > 2 ? fields[2] : null;

            return new Expense(money, date, description);
        }

        string GetExpenseCsvString(Expense expense)
        {
            var fields = new[]
            {
                expense.Date.ToString(DATE_FORMAT),
                expense.Money.ToString(CultureInfo.InvariantCulture),
                expense.Description ?? string.Empty,
            };
            return string.Join(CSV_DELIMITER, fields);
        }
    }
}