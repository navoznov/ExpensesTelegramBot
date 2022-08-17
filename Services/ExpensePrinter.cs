using System.Text;
using System.Globalization;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpensePrinter : IExpensePrinter
    {
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const char CSV_DELIMITER = ';';

        public string ToPlainText(Expense[] expenses)
        {
            var stringBuilder = new StringBuilder();
            foreach (var expense in expenses)
            {
                stringBuilder.Append(expense.Date.ToString("yyyy-MM-dd"));
                stringBuilder.Append('\t');
                stringBuilder.Append(expense.Money);
                stringBuilder.Append('\t');
                stringBuilder.Append(expense.Description);
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public string GetExpenseCsvString(Expense expense)
        {
            var fields = new[]
            {
                expense.Date.ToString(DATE_FORMAT),
                expense.Money.ToString(CultureInfo.InvariantCulture),
                expense.Description ?? string.Empty,
            };
            return string.Join(CSV_DELIMITER, fields);
        }
    }
}