using System;

namespace ExpensesTelegramBot.Telegram.Commands.Sum
{
    public class SumCommandInput : CommandInput
    {
        public const string NAME = "sum";

        public int Year { get; }
        public int Month { get; }

        public SumCommandInput(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public static bool TryParse(string inputArgumentsLine, out SumCommandInput? sumCommandInput)
        {
            sumCommandInput = null;
            
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;
            
            var args = inputArgumentsLine.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (args.Length > 2)
            {
                // log("Sum command must have 0, 1 or 2 arguments");
                return false;
            }

            if (args.Length == 1 && !int.TryParse(args[^1], out month) || month > 12 || month < 1)
            {
                // log("Month argument of sum command is in invalid format.");
                return false;
            }

            if (args.Length == 2 && !int.TryParse(args[^2], out year) || year < 2000 || year > 2100)
            {
                // log("Year argument of sum command is in invalid format.");\
                return false;
            }

            sumCommandInput = new SumCommandInput(year, month);
            return true;
        }
    }
}