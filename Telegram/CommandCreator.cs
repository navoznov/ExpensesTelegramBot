using System;
using System.IO;
using ExpensesTelegramBot.Exceptions;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using ExpensesTelegramBot.Telegram.Commands;
using ExpensesTelegramBot.Telegram.Commands.Export;
using ExpensesTelegramBot.Telegram.Commands.Get;
using ExpensesTelegramBot.Telegram.Commands.GetAll;
using ExpensesTelegramBot.Telegram.Commands.Help;
using ExpensesTelegramBot.Telegram.Commands.Sum;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace ExpensesTelegramBot.Telegram
{
    public class CommandCreator : ICommandCreator
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;

        public CommandCreator(IExpensesRepository expensesRepository, IExpensePrinter expensePrinter)
        {
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
        }

        public Command CreateCommand(string input, long chatId)
        {
            var commandName = input[1..].ToLower();
            switch (commandName)
            {
                case "help":
                    return new HelpCommand(new HelpCommandInput());
                case "get":
                {
                    var getCommandInput = new GetCommandInput();
                    return new GetCommand(getCommandInput, chatId, _expensesRepository, _expensePrinter);
                }
                case "getall":
                {
                    var getAllCommandInput = new GetAllCommandInput();
                    return new GetAllCommand(getAllCommandInput, chatId, _expensesRepository, _expensePrinter);
                }
                case "export":
                {
                    var now = DateTime.Now;
                    var exportCommandInput = new ExportCommandInput(now.Year, now.Month);
                    return new ExportCommand(exportCommandInput, chatId, _expensesRepository, _expensePrinter);
                }
                case "sum":
                {
                    var argsStr = commandName["sum".Length..];
                    if (!SumCommandInput.TryParse(argsStr, out var sumCommandInput))
                    {
                        throw new ParsingException($"Sum command arguments parsing error. Arguments string: {argsStr}");
                    }

                    return new SumCommand(sumCommandInput!, chatId, _expensesRepository);
                }
                default:
                    throw new ParsingException($"Command parsing error. Unknown input: {input}");
            }
        }
    }
}