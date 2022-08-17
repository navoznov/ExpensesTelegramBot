namespace ExpensesTelegramBot.Telegram.Commands
{
    public abstract class Command<TInput, TResult>
        where TInput : CommandInput
        where TResult : CommandResult
    {
        protected readonly TInput Input;

        public abstract TResult Execute();

        protected Command(TInput input)
        {
            Input = input;
        }
    }
}