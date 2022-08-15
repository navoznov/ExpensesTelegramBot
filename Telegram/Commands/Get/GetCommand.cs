using System;
using System.Linq;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;

namespace ExpensesTelegramBot.Telegram.Commands.Get
{
    public class GetCommand : Command<GetCommandInput, CommandTextResult>
    {
        public const string NAME = "get";

        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;

        public GetCommand(GetCommandInput input, IExpensesRepository expensesRepository, IExpensePrinter expensePrinter)
            : base(input)
        {
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public override CommandTextResult Execute()
        {
            const int COUNT = 5;
            var now = DateTime.Now;
            var expenses = _expensesRepository.GetLastExpenses(COUNT);
            var text = expenses.Any()? _expensePrinter.ToPlainText(expenses) : "No records";
            return new CommandTextResult(text);
        }
    }
}