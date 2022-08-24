namespace ExpensesTelegramBot.Telegram.Commands
{
    public class CommandMarkdownTextResult : CommandResult
    {
        public string MarkdownText { get; }

        public CommandMarkdownTextResult(string markdownText)
        {
            MarkdownText = markdownText;
        }
    }
}