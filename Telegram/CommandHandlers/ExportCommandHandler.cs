using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using Navoznov.DotNetHelpers.Extensions;

namespace ExpensesTelegramBot.Telegram.CommandHandlers
{
    public class ExportCommandHandler 
    {
        public const string EXPORT_COMMAND = "export";
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;

        public ExportCommandHandler(IExpensesRepository expensesRepository, IExpensePrinter expensePrinter)
        {
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public string Handle(string commandInput)
        {
            if (!commandInput.StartsWith(EXPORT_COMMAND))
            {
                throw new ArgumentException($"Command should start with \"{EXPORT_COMMAND}\"");
            }

            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            var expenses = _expensesRepository.GetAll(year, month);
            var aggregatedExpenses = expenses.GroupBy(e=> e.Date)
                .ToDictionary(g=>g.Key, g=> g.Sum(e=> e.Money));

            var monthDaysCount = new DateTime(year, month, 1).EndOfMonth().Day;
            var dayExpenseStrings = Enumerable.Range(1, monthDaysCount)
                .Select(day => new DateTime(year, month, day))
                .Select(date => new Expense(aggregatedExpenses.GetValueOrDefault(date, 0), date, null))
                .Select(e=>_expensePrinter.GetExpenseCsvString(e))
                .ToArray();

            var fileName = $"export-{year}-{month:00}.csv"; 
            using var stream = File.Open(fileName, FileMode.Create);
            using var writer = new StreamWriter(stream);
            foreach (var line in dayExpenseStrings)
            {
                writer.WriteLine(line);
            }

            return fileName;
        }
    }
}