using System.Linq;
using ExpensesTelegramBot.Repositories;

namespace ExpensesTelegramBot.Telegram.Commands.Sum
{
    public class SumCommand : Command<SumCommandInput, CommandTextResult>
    {
        private readonly IExpensesRepository _expensesRepository;
        private long _chatId;

        public SumCommand(SumCommandInput input, long chatId, IExpensesRepository expensesRepository) : base(input)
        {
            _chatId = chatId;
            _expensesRepository = expensesRepository;
        }

        public override CommandTextResult Execute()
        {
            var expenses = _expensesRepository.GetAll(_chatId, Input.Year, Input.Month);
            var sum = expenses.Sum(e => e.Money);
            var text = sum.ToString();
            return new CommandTextResult(text);
        }
    }
}