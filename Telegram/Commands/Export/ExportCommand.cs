using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using Navoznov.DotNetHelpers.Extensions;

namespace ExpensesTelegramBot.Telegram.Commands.Export
{
    public class ExportCommand : Command<ExportCommandInput, CommandFileResult>
    {
        public const string NAME = "export";

        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;
        private readonly long _chatId;

        public ExportCommand(ExportCommandInput input, long chatId, IExpensesRepository expensesRepository,
            IExpensePrinter expensePrinter) 
            : base(input)
        {
            _chatId = chatId;
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public override CommandFileResult Execute()
        {
            var year = Input.Year;
            var month = Input.Month;
            var expenses = _expensesRepository.GetAll(_chatId, year, month);
            var aggregatedExpenses = expenses.GroupBy(e => e.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Money));

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

            return new CommandFileResult(fileName);
        }
    }
}