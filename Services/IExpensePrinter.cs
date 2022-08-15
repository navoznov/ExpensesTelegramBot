using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public interface IExpensePrinter
    {
        string ToPlainText(Expense[] expenses);
    }
}