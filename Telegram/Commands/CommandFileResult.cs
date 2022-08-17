namespace ExpensesTelegramBot.Telegram.Commands
{
    public class CommandFileResult : CommandResult
    {
        public string FilePath { get; }

        public CommandFileResult(string filePath)
        {
            FilePath = filePath;
        }
    }
}