using System.IO;

namespace ExpensesTelegramBot.Telegram.Commands.Help
{
    public class HelpCommand : Command
    {
        public HelpCommand(HelpCommandInput input) : base(input)
        {
        }

        public override CommandResult Execute()
        {
            var markdownText = File.ReadAllText("Resources/Help/helpCommand.md");
            return new CommandMarkdownTextResult(markdownText);
        }
    }
}