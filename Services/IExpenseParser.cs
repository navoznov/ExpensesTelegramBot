using System;
using System.Threading.Tasks;

namespace ExpensesTelegramBot.Services
{
    public interface IExpenseParser
    {
        Task<ExpensesParsingResult> TryParse(string input, TimeZoneInfo userTimeZoneInfo);
    }
}