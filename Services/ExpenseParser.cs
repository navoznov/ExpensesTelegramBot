using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpenseParser : IExpenseParser
    {
        public (bool Success, Expense? Expense) TryParse(string input)
        {
            var pattern = @"(?:(?<date>\d\d\d\d[\.-]\d\d[\.-]\d\d) *)?(?<money>\d+)(?<multiplier>[KkMm]?) *(?<description>.+)?";

            var regex = new Regex(pattern);

            var match = regex.Match(input.Trim());
            if (!match.Success)
            {
                return (false, null);
            }

            var moneyStr = match.Groups["money"]?.Value;

            if (!decimal.TryParse(moneyStr, out var money))
            {
                return (false, null);
            }
        
            var multiplierStr = match.Groups["multiplier"]?.Value;
            var multiplier = multiplierStr?.ToUpper() == "K" ? 1_000 : 1;

            var date = ParseDate(match.Groups["date"]?.Value);
            var description = match.Groups["description"]?.Value.Trim();
            var expense = new Expense(money* multiplier, date, description);
            return (true, expense);
        }

        DateTime? ParseDate(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            
            var formats = new [] {"yyyy-MM-dd", "yyyy.MM.dd"};
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
                {
                    return date;
                }
            }

            return null;
        }
    }
}