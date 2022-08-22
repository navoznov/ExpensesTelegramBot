using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Repositories
{
    public interface IExpensesRepository
    {
        void Save(long chatId, Expense[] expenses);
        Expense[] GetAll(long chatId, int year, int month);
        Expense[] GetLastExpenses(long chatId, int count);
    }
}