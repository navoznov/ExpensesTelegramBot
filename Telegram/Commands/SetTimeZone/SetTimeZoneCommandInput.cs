using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpensesTelegramBot.Exceptions;

namespace ExpensesTelegramBot.Telegram.Commands.SetTimeZone
{
    public sealed class SetTimeZoneCommandInput : CommandInput
    {
        public TimeZoneInfo TimeZoneInfo { get; }

        private SetTimeZoneCommandInput(TimeZoneInfo timeZoneInfo)
        {
            TimeZoneInfo = timeZoneInfo;
        }

        public static bool TryParse(string input, out SetTimeZoneCommandInput? setTimeZoneCommandInput)
        {
            setTimeZoneCommandInput = null;

            var timeZoneOffset = ParseTimeZoneOffset(input);
            var timeZoneInfos = TimeZoneInfo.GetSystemTimeZones()
                .Where(x=>x.BaseUtcOffset == timeZoneOffset)
                .ToArray();
            if (!timeZoneInfos.Any())
            {
                return false;
            }

            // TODO: dont take first, suggest user to choose
            setTimeZoneCommandInput = new SetTimeZoneCommandInput(timeZoneInfos.First());
            return true;
        }
        
        private static TimeSpan ParseTimeZoneOffset(string input)
        {
            // 8, +8, -8, +08, -08, +08:30, -8:30
            const string? PATTERN = @"(?<hours>[\-\+]?\d\d?)(?:\:(?<minutes>\d\d))?";
            var regex = new Regex(PATTERN);
            if (!regex.IsMatch(input))
            {
                throw new ParsingException($"Invalid time zone offset: {input}");
            }

            var match = regex.Match(input);
            var hoursStr = match.Groups["hours"].Value;
            var hours = int.Parse(hoursStr);

            var minutesStr = match.Groups["minutes"]?.Value;
            var minutes = string.IsNullOrEmpty(minutesStr) ? 0 : int.Parse(minutesStr);
            var offset = new TimeSpan(hours, minutes, 0);
            return offset;
        }
    }
}