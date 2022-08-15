using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpensePrinter : IExpensePrinter
    {
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
    }
}