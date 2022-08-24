using System.Runtime.InteropServices.WindowsRuntime;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;

namespace ExpensesTelegramBot.Telegram.Commands.GetAll
{
    public class GetAllCommand : Command
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;
        private readonly long _chatId;

        public GetAllCommand(GetAllCommandInput input, long chatId, IExpensesRepository expensesRepository,
            IExpensePrinter expensePrinter)
            : base(input)
        {
            _chatId = chatId;
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public override CommandResult Execute()
        {
            var input = (GetAllCommandInput) Input;
            var expenses = _expensesRepository.GetAll(_chatId, input.Year, input.Month);
            var text = _expensePrinter.ToPlainText(expenses);
            return new CommandTextResult(text);
        }
    }
}