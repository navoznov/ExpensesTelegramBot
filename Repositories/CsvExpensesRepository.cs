using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Services;

namespace ExpensesTelegramBot.Repositories
{
    public class CsvExpensesRepository : IExpensesRepository
    {
        private readonly IExpensePrinter _expensePrinter;
        private const string CSV_FILES_STORAGE_FOLDER_NAME = "data";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const char CSV_DELIMITER = ';';

        public CsvExpensesRepository(IExpensePrinter expensePrinter)
        {
            _expensePrinter = expensePrinter;
            if (!Directory.Exists(CSV_FILES_STORAGE_FOLDER_NAME))
            {
                Directory.CreateDirectory(CSV_FILES_STORAGE_FOLDER_NAME);
            }
        }

        public void Save(long chatId, Expense[] expenses)
        {
            var expensesGroupedByFileName = expenses
                .GroupBy(e => GetFileName(e.Date.Year, e.Date.Month));

            foreach (var expensesGroup in expensesGroupedByFileName)
            {

                var fileName = expensesGroup!.Key;
                var filePath = GetFilePath(chatId, fileName);
                var csv = GetExpensesCsv(expensesGroup);
                AppendToFile(filePath, csv);
            }
        }

        private string GetExpensesCsv(IEnumerable<Expense> expenses)
        {
            var csvTextBuilder = new StringBuilder();
            foreach (var expense in expenses)
            {
                var expenseCsvString = _expensePrinter.GetExpenseCsvString(expense);
                csvTextBuilder.AppendLine(expenseCsvString);
            }

            return csvTextBuilder.ToString();
        }

        public Expense[] GetAll(long chatId, int year, int month)
        {
            var fileName = GetFileName(year, month);
            var filePath = GetFilePath(chatId, fileName);
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

        public Expense[] GetLastExpenses(long chatId, int count)
        {
            var directoryPath = Path.Combine(".", CSV_FILES_STORAGE_FOLDER_NAME, chatId.ToString());
            if (!Directory.Exists(directoryPath))
            {
                return Array.Empty<Expense>();
            }
            
            var directoryInfo = new DirectoryInfo(directoryPath);
            var fileNames = directoryInfo.GetFiles().Select(fi => fi.Name).ToArray();

            const string EXPENSES_DATA_FILE_PATTERN = @"^\d\d\d\d-\d\d\.csv$";
            var regex = new Regex(EXPENSES_DATA_FILE_PATTERN);
            var matchedFileNames = fileNames.Where(fn => regex.IsMatch(fn))
                .OrderByDescending(fn => fn)
                .ToArray();
            var result = new List<Expense>();
            foreach (var fileName in matchedFileNames)
            {
                var filePath = GetFilePath(chatId, fileName);
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

        private static string GetFilePath(long chatId, string fileName)
        {
            var directoryPath = Path.Combine(CSV_FILES_STORAGE_FOLDER_NAME, chatId.ToString());
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return Path.Combine(directoryPath, fileName);
        }

        private static void AppendToFile(string filePath, string text)
        {
            using var stream = File.Open(filePath, FileMode.Append);
            using var writer = new StreamWriter(stream);
            writer.WriteAsync(text);
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
    }
}