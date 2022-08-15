using System.Linq;
using ExpensesTelegramBot.Repositories;

namespace ExpensesTelegramBot.Telegram.Commands.Sum
{
    public class SumCommand : Command<SumCommandInput, CommandTextResult>
    {
        private readonly IExpensesRepository _expensesRepository;

        public SumCommand(SumCommandInput input, IExpensesRepository expensesRepository) : base(input)
        {
            _expensesRepository = expensesRepository;
        }

        public override CommandTextResult Execute()
        {
            var expenses = _expensesRepository.GetAll(Input.Year, Input.Month);
            var sum = expenses.Sum(e => e.Money);
            var text = sum.ToString();
            return new CommandTextResult(text);
        }
    }
}