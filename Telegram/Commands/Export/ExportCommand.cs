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

namespace ExpensesTelegramBot.Telegram.Commands.Export
{
    public class ExportCommand : Command<ExportCommandInput, CommandFileResult>
    {
        public const string NAME = "export";

        private readonly IExpensesRepository _expensesRepository;

        public ExportCommand(ExportCommandInput input, IExpensesRepository expensesRepository) : base(input)
        {
            _expensesRepository = expensesRepository;
        }

        public override CommandFileResult Execute()
        {
            var year = Input.Year;
            var month = Input.Month;
            var expenses = _expensesRepository.GetAll(year, month);
            var aggregatedExpenses = expenses.GroupBy(e => e.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Money));

            var monthDaysCount = new DateTime(year, month, 1).EndOfMonth().Day;
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
            return new CommandFileResult(fileName);
        }
    }
}