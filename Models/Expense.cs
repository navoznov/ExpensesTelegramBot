using System;
using CsvHelper.Configuration.Attributes;

namespace ExpensesTelegramBot.Models
{
    public class Expense
    {
        [Index(0)]
        [Format("yyyy-MM-dd")]
        public DateTime Date { get; set; }
    
        [Index(1)]
        public decimal Money { get; set; }
    
        [Index(2)]
        public string? Description { get; set; }

        public Expense(decimal money, DateTime? date, string? description)
        {
            Date = date ?? DateTime.Now.Date;
            Money = money;
            Description = description;
        }

        public Expense()
        {
        }
    }
}