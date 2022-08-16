using System;

namespace ExpensesTelegramBot.Telegram.Commands.GetAll
{
    public class GetAllCommandInput : CommandInput
    {
        public int Year { get; }
        public int Month { get; }

        public GetAllCommandInput()
        {
            Year = DateTime.Now.Year;
            Month = DateTime.Now.Month;
        }
    }
}