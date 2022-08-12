using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public interface IExpenseParser
    {
        (bool Success, Expense? Expense) TryParse(string input);
    }
}