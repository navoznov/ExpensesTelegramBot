namespace ExpensesTelegramBot.Telegram.Commands
{
    public abstract class Command
    {
        protected CommandInput Input;
        public abstract CommandResult Execute();
        protected Command(CommandInput input)
        {
            Input = input;
        }
    }
}