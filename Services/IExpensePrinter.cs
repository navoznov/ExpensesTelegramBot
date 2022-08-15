using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public interface IExpensePrinter
    {
        string GetExpenseCsvString(Expense expense);
    }
}