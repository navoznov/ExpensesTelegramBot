using System.IO;

namespace ExpensesTelegramBot.Telegram.Commands.Help
{
    public class HelpCommand : Command<HelpCommandInput, CommandTextResult>
    {
        public const string NAME = "help";

        public HelpCommand(HelpCommandInput input) : base(input)
        {
        }

        public override CommandTextResult Execute()
        {
            var text = File.ReadAllText("Resources/Help/helpCommand.md");
            return new CommandTextResult(text);
        }
    }
}