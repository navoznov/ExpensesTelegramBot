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
using ExpensesTelegramBot.Telegram.Commands.SetTimeZone;
using ExpensesTelegramBot.Telegram.Commands.Sum;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace ExpensesTelegramBot.Telegram
{
    public class CommandCreator : ICommandCreator
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpensePrinter _expensePrinter;
        private readonly IUserSettingsRepository _userSettingsRepository;

        public CommandCreator(IExpensesRepository expensesRepository, IExpensePrinter expensePrinter,
            IUserSettingsRepository userSettingsRepository)
        {
            _expensesRepository = expensesRepository;
            _expensePrinter = expensePrinter;
            _userSettingsRepository = userSettingsRepository;
        }

        public Command CreateCommand(string input, long chatId)
        {
            var (commandName, commandArgsString) = SplitInput(input[1..]);
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
                    if (!SumCommandInput.TryParse(commandArgsString, out var sumCommandInput))
                    {
                        throw new ParsingException($"Sum command arguments parsing error. Arguments string: {commandArgsString}");
                    }

                    return new SumCommand(sumCommandInput!, chatId, _expensesRepository);
                }
                case "settimezone":
                {
                    if (!SetTimeZoneCommandInput.TryParse(commandArgsString, out var setTimeZoneCommandInput))
                    {
                        throw new ParsingException($"Set time zone command arguments parsing error. Arguments string: {commandArgsString}");
                    }

                    return new SetTimeZoneCommand(setTimeZoneCommandInput!, chatId, _userSettingsRepository);

                }
                default:
                    throw new ParsingException($"Command parsing error. Unknown input: {input}");
            }
        }

        private (string CommandName, string CommandArgsString) SplitInput(string input)
        {
            var indexOfSpacebar = input.IndexOf(' ');
            if (indexOfSpacebar == -1)
            {
                return (input, "");
            }

            var commandName = input[0..indexOfSpacebar];
            var commandArgsString = input[(indexOfSpacebar+1)..].Trim();
            return (commandName, commandArgsString);
        }
    }
}