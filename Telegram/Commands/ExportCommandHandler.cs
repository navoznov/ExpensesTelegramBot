using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using Navoznov.DotNetHelpers.Extensions;

namespace ExpensesTelegramBot.Telegram.Commands
{
    public class ExportCommandHandler 
    {
        public const string EXPORT_COMMAND = "export";
        private readonly IExpensesRepository _expensesRepository;

        public ExportCommandHandler(IExpensesRepository expensesRepository)
        {
            _expensesRepository = expensesRepository;
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

            var monthDaysCount = now.EndOfMonth().Day;
            var dayExpenses = Enumerable.Range(1, monthDaysCount)
                .Select(day => new DateTime(year, month, day))
                .Select(date => new Expense(aggregatedExpenses.GetValueOrDefault(date, 0), date, null))
                .ToArray();

            var fileName = $"export-{year}-{month:00}.csv";
            using var stream = File.Open(fileName, FileMode.Create);
            using var writer = new StreamWriter(stream);
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
            };
            using var csv = new CsvWriter(writer, csvConfiguration);
            csv.WriteRecords(dayExpenses);
            return fileName;
        }
    }
}