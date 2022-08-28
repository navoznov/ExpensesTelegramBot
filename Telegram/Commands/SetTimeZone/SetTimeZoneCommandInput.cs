using System;
using System.Globalization;
using System.Linq;

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

            var offsetFormats = new[] {"\\+H", "\\-H","\\+H:mm", "\\-H:mm"};
            if (!DateTimeOffset.TryParseExact(input, offsetFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTimeOffset))
            {
                return false;
            }

            var timeZoneInfos = TimeZoneInfo.GetSystemTimeZones()
                .Where(z => z.BaseUtcOffset == dateTimeOffset.Offset)
                .ToArray();
            if (!timeZoneInfos.Any())
            {
                return false;
            }

            // TODO: dont take first, suggest user to choose
            setTimeZoneCommandInput = new SetTimeZoneCommandInput(timeZoneInfos.First());
            return true;
        }

    }
}