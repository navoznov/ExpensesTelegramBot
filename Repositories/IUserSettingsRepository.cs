using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Repositories
{
    public interface IUserSettingsRepository
    {
        void Save(UserSettingsInfo userSettingsInfo);
        UserSettingsInfo GetUserSettingsInfo(long userId);
    }
}