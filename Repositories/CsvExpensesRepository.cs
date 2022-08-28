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
        private readonly IUserSettingsRepository _userSettingsRepository;
        private const string CSV_FILES_STORAGE_FOLDER_NAME = "data";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const char CSV_DELIMITER = ';';
        private const string EXPENSES_FILENAME = "expenses.csv";


        public CsvExpensesRepository(IExpensePrinter expensePrinter, IUserSettingsRepository userSettingsRepository)
        {
            _expensePrinter = expensePrinter;
            _userSettingsRepository = userSettingsRepository;
            if (!Directory.Exists(CSV_FILES_STORAGE_FOLDER_NAME))
            {
                Directory.CreateDirectory(CSV_FILES_STORAGE_FOLDER_NAME);
            }
        }

        public void Save(long chatId, Expense[] expenses)
        {
            var filePath = GetFilePath(chatId);
            // TODO: remove hack
            CopyExpensesToNewFile(chatId, filePath);
            var orderedExpenses = expenses.OrderBy(e => e.Date).ToArray();
            var csv = GetExpensesCsv(orderedExpenses);
            AppendToFile(filePath, csv);
        }

        public Expense[] GetLastExpenses(long chatId, int count)
        {
            var filePath = GetFilePath(chatId);
            if (!File.Exists(filePath))
            {
                // TODO: remove hack
                CopyExpensesToNewFile(chatId, filePath);
                if (!File.Exists(filePath))
                {
                    return Array.Empty<Expense>();
                }
            }

            var utcOffset = GetUserTimeZoneUtcOffset(chatId);
            var expenses = GetExpensesFromFile(filePath, utcOffset);
            return expenses
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToArray();
        }

        public Expense[] GetAll(long chatId, int year, int month)
        {
            var filePath = GetFilePath(chatId);
            if (!File.Exists(filePath))
            {
                // TODO: remove hack
                CopyExpensesToNewFile(chatId, filePath);
                if (!File.Exists(filePath))
                {
                    return Array.Empty<Expense>();
                }
            }

            var utcOffset = GetUserTimeZoneUtcOffset(chatId);
            var expenses = GetExpensesFromFile(filePath, utcOffset);
            return expenses
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date)
                .ToArray();
        }

        private TimeSpan GetUserTimeZoneUtcOffset(long chatId)
        {
            var userSettingsInfo = _userSettingsRepository.GetUserSettingsInfo(chatId);
            return userSettingsInfo.TimeZoneInfo.BaseUtcOffset;
        }

        private static string GetFilePath(long chatId)
        {
            var directoryPath = Path.Combine(CSV_FILES_STORAGE_FOLDER_NAME, chatId.ToString());
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, EXPENSES_FILENAME);
        }

        private static void CopyExpensesToNewFile(long chatId, string? filePath)
        {
            if (File.Exists(filePath)) return;

            // HACK: copy saved expenses to new file
            var oldFilePath = Path.Combine(CSV_FILES_STORAGE_FOLDER_NAME, chatId.ToString(), "2022-08.csv");
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, filePath);
                File.Delete(oldFilePath);
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

        private static void AppendToFile(string filePath, string text)
        {
            using var stream = File.Open(filePath, FileMode.Append);
            using var writer = new StreamWriter(stream);
            writer.WriteAsync(text);
        }

        private Expense[] GetExpensesFromFile(string filePath, TimeSpan utcOffset)
        {
            var lines = File.ReadAllLines(filePath);
            return lines.Select(s => ParseCsvExpense(s, utcOffset)).ToArray();
        }

        Expense ParseCsvExpense(string line, TimeSpan utcOffset)
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

            var dateTimeOffset = new DateTimeOffset(date, utcOffset);
            date = dateTimeOffset.Date;
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