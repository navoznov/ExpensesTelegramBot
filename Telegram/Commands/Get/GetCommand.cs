using System;
using System.Linq;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;

namespace ExpensesTelegramBot.Telegram.Commands.Get
{
    public class GetCommand : Command
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;
        private readonly long _chatId;

        public GetCommand(GetCommandInput input,
            long chatId, IExpensesRepository expensesRepository, IExpensePrinter expensePrinter)
            : base(input)
        {
            _chatId = chatId;
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public override CommandResult Execute()
        {
            const int COUNT = 5;
            var now = DateTime.Now;
            var expenses = _expensesRepository.GetLastExpenses(_chatId, COUNT);
            var text = expenses.Any() ? _expensePrinter.ToPlainText(expenses) : "No records";
            return new CommandTextResult(text);
        }
    }
}