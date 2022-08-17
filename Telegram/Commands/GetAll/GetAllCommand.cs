using System.Runtime.InteropServices.WindowsRuntime;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;

namespace ExpensesTelegramBot.Telegram.Commands.GetAll
{
    public class GetAllCommand : Command<GetAllCommandInput, CommandTextResult>
    {
        public const string NAME = "getall";

        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;

        public GetAllCommand(GetAllCommandInput input, IExpensesRepository expensesRepository,
            IExpensePrinter expensePrinter)
            : base(input)
        {
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public override CommandTextResult Execute()
        {
            var expenses = _expensesRepository.GetAll(Input.Year, Input.Month);
            var text = _expensePrinter.ToPlainText(expenses);
            return new CommandTextResult(text);
        }
    }
}