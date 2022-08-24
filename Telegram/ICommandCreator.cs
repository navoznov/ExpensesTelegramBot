using ExpensesTelegramBot.Telegram.Commands;

namespace ExpensesTelegramBot.Telegram
{
    public interface ICommandCreator
    {
        Command CreateCommand(string input, long chatId);
    }
}