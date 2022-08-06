using System;

public class Expense
{
    public decimal Money { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; } = DateTime.Now.Date;

    public Expense(decimal money, string? description)
    {
        Money = money;
        Description = description;
    }
}