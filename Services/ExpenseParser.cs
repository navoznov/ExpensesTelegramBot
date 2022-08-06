using System.Text.RegularExpressions;

namespace ExpensesTelegramBot.Services
{
    public class ExpenseParser
    {
        public (bool Success, Expense? Expense) TryParse(string input)
        {
            var pattern = @"(?<money>\d+)(?<multiplier>[KkMm]?) *(?<description>.+)?";

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
        
            var descriptionStr = match.Groups["description"]?.Value.Trim();
            var expense = new Expense(money* multiplier, descriptionStr);
            return (true, expense);
        }
    }
}