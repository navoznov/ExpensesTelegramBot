using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExpensesTelegramBot.Exceptions;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using ExpensesTelegramBot.Telegram.Commands;
using Telegram.Bot;
using ExpensesTelegramBot.Telegram.Commands.Export;
using ExpensesTelegramBot.Telegram.Commands.Get;
using ExpensesTelegramBot.Telegram.Commands.GetAll;
using ExpensesTelegramBot.Telegram.Commands.Help;
using ExpensesTelegramBot.Telegram.Commands.Sum;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace ExpensesTelegramBot.Telegram
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly IExpenseParser _expenseParser;
        private readonly IExpensePrinter _expensePrinter;
        private readonly ICommandCreator _commandCreator;

        public UpdateHandler(IExpensesRepository expensesRepository, IExpenseParser expenseParser,
            IExpensePrinter expensePrinter, ICommandCreator commandCreator)
        {
            _expensesRepository = expensesRepository;
            _expenseParser = expenseParser;
            _expensePrinter = expensePrinter;
            _commandCreator = commandCreator;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            var message = update.Message;
            if (message is null)
            {
                return;
            }

            // Only process text messages
            var messageText = message.Text?.Trim();
            if (messageText is null)
            {
                return;
            }

            Console.WriteLine($"Received a message: {messageText}");
            var chatId = message.Chat.Id;
            var replyToMessageId = message.MessageId;
            if (messageText.StartsWith("/"))
            {
                try
                {
                    var command = _commandCreator.CreateCommand(messageText, chatId);
                    var commandResult = command.Execute();
                    await ProcessCommandResult(commandResult, botClient, chatId, replyToMessageId, cancellationToken);
                }
                catch (ParsingException e)
                {
                    Console.WriteLine(e);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Unknown command",
                        replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
                }

                return;
            }

            var expensesParsingResult = await _expenseParser.TryParse(messageText);
            _expensesRepository.Save(chatId, expensesParsingResult.ParsedExpenses);
            var expensesParsingAnswerText = GetExpensesParsingAnswerText(expensesParsingResult);
            await botClient.SendTextMessageAsync(chatId: chatId, text: expensesParsingAnswerText,
                replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
        }

        private static async Task ProcessCommandResult(CommandResult commandResult, ITelegramBotClient botClient, 
            long chatId, int replyToMessageId, CancellationToken cancellationToken)
        {
            switch (commandResult)
            {
                case CommandTextResult commandTextResult:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commandTextResult.Text,
                        replyToMessageId: replyToMessageId, cancellationToken: cancellationToken);
                    break;
                case CommandMarkdownTextResult commandMarkdownTextResult:
                    await botClient.SendTextMessageAsync(chatId,
                        commandMarkdownTextResult.MarkdownText,
                        ParseMode.Markdown,
                        replyToMessageId: replyToMessageId,
                        cancellationToken: cancellationToken);
                    break;
                case CommandFileResult commandFileResult:
                {
                    var exportFileName = commandFileResult.FilePath;
                    await using Stream stream = File.OpenRead(exportFileName);
                    var inputOnlineFile = new InputOnlineFile(stream, exportFileName);
                    await botClient.SendDocumentAsync(chatId, inputOnlineFile,
                        replyToMessageId: replyToMessageId,
                        caption: "Expenses day by day for the month",
                        cancellationToken: cancellationToken);

                    File.Delete(exportFileName);
                    break;
                }
                default:
                    throw new Exception($"Unknown command result type: {commandResult.GetType()}");
            }
        }

        private string GetExpensesParsingAnswerText(ExpensesParsingResult expensesParsingResult)
        {
            var parsedExpenses = expensesParsingResult.ParsedExpenses;
            var unparsedLines = expensesParsingResult.UnparsedLines;
            var answerTextBuilder = new StringBuilder()
                .AppendLine($"Parsed count: {parsedExpenses.Length}")
                .AppendLine($"Unparsed count: {unparsedLines.Length}");

            AppendParsedExpensesBlock(answerTextBuilder, parsedExpenses);
            AppendUnparsedLinesBlock(answerTextBuilder, unparsedLines);

            return answerTextBuilder.ToString();
        }

        private void AppendParsedExpensesBlock(StringBuilder answerTextBuilder, Expense[] parsedExpenses)
        {
            answerTextBuilder
                .AppendLine()
                .AppendLine("Parsed expenses:");

            if (parsedExpenses.Any())
            {
                var expensesText = _expensePrinter.ToPlainText(parsedExpenses);
                answerTextBuilder.Append(expensesText);
            }
            else
            {
                answerTextBuilder.AppendLine("No expenses");
            }
        }

        private void AppendUnparsedLinesBlock(StringBuilder answerTextBuilder, string[] unparsedLines)
        {
            if (unparsedLines.Length == 0) return;

            answerTextBuilder
                .AppendLine()
                .AppendLine("Unparsed lines:");
            foreach (var unparsedLine in unparsedLines)
            {
                answerTextBuilder.AppendLine(unparsedLine);
            }
        }
    }
}