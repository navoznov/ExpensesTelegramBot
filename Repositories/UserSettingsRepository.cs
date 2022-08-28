using System;
using System.IO;
using System.Text;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Repositories
{
    public class FileUserSettingsRepository : IUserSettingsRepository
    {
        private const string FILES_STORAGE_FOLDER_NAME = "data";

        public void Save(UserSettingsInfo userSettingsInfo)
        {
            string content = GetUserSettingsFileContent(userSettingsInfo);
            var filePath = GetFilePath(userSettingsInfo.UserId);
            using var streamWriter = File.CreateText(filePath);
            streamWriter.Write(content);
        }

        public UserSettingsInfo GetUserSettingsInfo(long userId)
        {
            var filePath = GetFilePath(userId);
            if (!File.Exists(filePath))
            {
                return new UserSettingsInfo(userId);
            }

            var lines = File.ReadAllLines(filePath);
            var timeZoneLine = lines[0];
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneLine);
            return new UserSettingsInfo(userId, timeZoneInfo);
        }

        private static string GetFilePath(long userId)
        {
            return Path.Combine(FILES_STORAGE_FOLDER_NAME, userId.ToString(), "userSettings.cfg");
        }

        private string GetUserSettingsFileContent(UserSettingsInfo userSettingsInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine(userSettingsInfo.TimeZoneInfo.Id);
            return sb.ToString();
        }
    }
}