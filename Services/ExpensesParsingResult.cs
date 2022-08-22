using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Services
{
    public class ExpensesParsingResult
    {
        public bool IsSuccess { get; set; }
        public Expense[] ParsedExpenses { get; set; }
        public string[] UnparsedLines { get; set; }

        public ExpensesParsingResult(bool isSuccess,
            Expense[] parsedExpenses = null,
            string[] unparsedLines = null)
        {
            IsSuccess = isSuccess;
            ParsedExpenses = parsedExpenses;
            UnparsedLines = unparsedLines;
        }
    }
}