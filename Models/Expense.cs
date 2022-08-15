using System;

namespace ExpensesTelegramBot.Models
{
    public class Expense
    {
        public DateTime Date { get; set; }
    
        public decimal Money { get; set; }
    
        public string? Description { get; set; }

        public Expense(decimal money, DateTime? date, string? description)
        {
            Date = date ?? DateTime.Now.Date;
            Money = money;
            Description = description;
        }
    }
}