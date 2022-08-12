using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;

namespace ExpensesTelegramBot.Repositories
{
    public interface IExpensesRepository
    {
        void Save(Expense expense);
        Expense[] GetAll(int year, int month);
        Expense[] GetLastExpenses(int count);
    }
}