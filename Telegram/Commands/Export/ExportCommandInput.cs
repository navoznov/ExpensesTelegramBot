namespace ExpensesTelegramBot.Telegram.Commands.Export
{
    public class ExportCommandInput : CommandInput
    {
        public int Year { get; }
        public int Month { get; }

        public ExportCommandInput(int year, int month)
        {
            Year = year;
            Month = month;
        }
    }
}