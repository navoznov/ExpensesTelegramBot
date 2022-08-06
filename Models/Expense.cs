using System;

public class Expense
{
    public decimal Money { get; set; }
    public string Description { get; set; }
    public DateTime MyProperty { get; set; } = DateTime.Now.Date;
}