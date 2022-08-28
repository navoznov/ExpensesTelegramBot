using System;

namespace ExpensesTelegramBot.Models
{
    public class UserSettingsInfo
    {
        public long UserId { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
        
        public UserSettingsInfo(long userId, TimeZoneInfo timeZoneInfo)
        {
            UserId = userId;
            TimeZoneInfo = timeZoneInfo;
        }

        public UserSettingsInfo(long userId) : this(userId, TimeZoneInfo.Local)
        {
        }
    }
}