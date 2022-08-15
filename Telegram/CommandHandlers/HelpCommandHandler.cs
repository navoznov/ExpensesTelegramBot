using System.IO;
using System.Threading.Tasks;

namespace ExpensesTelegramBot.Telegram.CommandHandlers
{
    public class HelpCommandHandler
    {
        private static string? _helpContent;
        public async Task<string> Handle()
        {
            _helpContent ??= await File.ReadAllTextAsync("Resources/Help/helpCommand.md");

            return _helpContent!;
        }
    }
}