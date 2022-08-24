using System;
using System.Linq;
using ExpensesTelegramBot.Repositories;

namespace ExpensesTelegramBot.Telegram.Commands.Sum
{
    public class SumCommand : Command
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly long _chatId;

        public SumCommand(SumCommandInput input, long chatId, IExpensesRepository expensesRepository) : base(input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _chatId = chatId;
            _expensesRepository = expensesRepository;
        }

        public override CommandResult Execute()
        {
            var input = (SumCommandInput) Input;
            var expenses = _expensesRepository.GetAll(_chatId, input.Year, input.Month);
            var sum = expenses.Sum(e => e.Money);
            var text = sum.ToString();
            return new CommandTextResult(text);
        }
    }
}