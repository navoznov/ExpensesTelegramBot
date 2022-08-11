using System;
using CsvHelper.Configuration.Attributes;

namespace ExpensesTelegramBot.Models
{
    public class Expense
    {
        [Index(0)]
        [Format("yyyy-MM-dd")]
        public DateTime Date { get; set; } = DateTime.Now.Date;
    
        [Index(1)]
        public decimal Money { get; set; }
    
        [Index(2)]
        public string? Description { get; set; }

        public Expense(decimal money, string? description)
        {
            Money = money;
            Description = description;
        }
    }
}