using System;

namespace ExpensesTelegramBot.Exceptions
{
    public class ParsingException : Exception
    {
        public ParsingException(string? message) : base(message)
        {
        }
    }
}