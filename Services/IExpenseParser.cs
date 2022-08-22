using System.Threading.Tasks;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public interface IExpenseParser
    {
        Task<ExpensesParsingResult> TryParse(string input);
    }
}