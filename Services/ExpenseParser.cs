using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpenseParser : IExpenseParser
    {
        public async Task<ExpensesParsingResult> TryParse(string input)
        {
            var isSuccess = true;
            var parsedExpenses = new List<Expense>();
            var invalidLines = new List<string>();
            
            var stringReader = new StringReader(input);
            const string EXPENSE_PATTERN =
                @"^(?:(?<date>\d\d\d\d[\.-]\d\d[\.-]\d\d) +)?(?<money>\d+)(?<multiplier>[KkMm]?)(?: +(?<description>.+))?$";
            var regex = new Regex(EXPENSE_PATTERN);
            while (true)
            {
                var line = await stringReader.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                var match = regex.Match(line);
                if (!match.Success)
                {
                    isSuccess = false;
                    invalidLines.Add(line);
                    continue;
                }

                var moneyStr = match.Groups["money"]?.Value;
                if (!decimal.TryParse(moneyStr, out var money))
                {
                    isSuccess = false;
                    invalidLines.Add(line);
                    continue;
                }

                var multiplierStr = match.Groups["multiplier"]?.Value;
                var multiplier = multiplierStr?.ToUpper() == "K" ? 1_000 : 1;

                var date = ParseDateWithMultiplyFormats(match.Groups["date"]?.Value);
                var description = match.Groups["description"]?.Value.Trim();
                
                var expense = new Expense(money * multiplier, date, description);
                parsedExpenses.Add(expense);
            }
            
            return new ExpensesParsingResult(isSuccess, parsedExpenses.ToArray(), invalidLines.ToArray());
        }

        DateTime? ParseDateWithMultiplyFormats(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var formats = new[] {"yyyy-MM-dd", "yyyy.MM.dd"};
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