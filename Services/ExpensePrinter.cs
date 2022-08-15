using System.Globalization;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpensePrinter : IExpensePrinter
    {
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const char CSV_DELIMITER = ';';

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

        // public string GetExpenseToMessageString(Expense expense)
        // {
        //     var dateStr = expense.Date.ToString(DATE_FORMAT);
        //     return $"{dateStr} {expense.Money} {expense.Description}";
        // }
    }
}