using System;
using System.Text.RegularExpressions;

public class ExpenseParser
{
    public (bool Success, Expense? Expense) TryParse(string input)
    {
        var pattern = @"(\d+[KkMm]?) *(.+)?";

        var regex = new Regex(pattern);

        var matches = regex.Matches(input.Trim());
        foreach(var match in matches){
            Console.WriteLine();
        }

        return (false, null);

        // input = input.Trim();
        // var parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        // if (!parts.Any())
        // {
        //     return (false, null);
        // }

        // var firstSpaceIndex = input.IndexOf(' ');
        // if (firstSpaceIndex == -1)
        // {
        //     return (false, null);
        // }

        // var moneyPart = input.Substring(0, firstSpaceIndex);
        // var thousandsSuffixes = new[] { 'K', 'k' };
        // var multiplier = 1;
        // if (thousandsSuffixes.Contains(moneyPart[-1]))
        // {
        //     multiplier = 1_000;
        // }
        // if(!decimal.TryParse(moneyPart.Substring(0, moneyPart.Length -1), out var money)){
        //     return (false, null);
        // }
    }
}