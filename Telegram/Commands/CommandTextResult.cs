namespace ExpensesTelegramBot.Telegram.Commands
{
    public class CommandTextResult : CommandResult
    {
        public string Text { get; }

        public CommandTextResult(string text)
        {
            Text = text;
        }
    }
}