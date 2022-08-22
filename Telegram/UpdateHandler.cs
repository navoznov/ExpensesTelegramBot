using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
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

        public UpdateHandler(IExpensesRepository expensesRepository, IExpenseParser expenseParser,
            IExpensePrinter expensePrinter)
        {
            _expensesRepository = expensesRepository;
            _expenseParser = expenseParser;
            _expensePrinter = expensePrinter;
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
            if (messageText.StartsWith("/"))
            {
                var command = messageText[1..].ToLower();

                if (command == HelpCommand.NAME)
                {
                    var helpCommand = new HelpCommand(new HelpCommandInput());
                    var commandResult = helpCommand.Execute();
                    var text = commandResult.Text;
                    await botClient.SendTextMessageAsync(chatId, text, ParseMode.Markdown,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
                else if (command == GetCommand.NAME)
                {
                    var getCommandInput = new GetCommandInput();
                    var getCommand = new GetCommand(getCommandInput, _expensesRepository, _expensePrinter);
                    var getCommandResult = getCommand.Execute();
                    var text = getCommandResult.Text;
                    await botClient.SendTextMessageAsync(chatId, text,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
                else if (command == GetAllCommand.NAME)
                {
                    var getAllCommandInput = new GetAllCommandInput();
                    var getAllCommand = new GetAllCommand(getAllCommandInput, _expensesRepository, _expensePrinter);
                    var commandResult = getAllCommand.Execute();
                    var text = commandResult.Text;
                    await botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
                }
                else if (command.StartsWith(ExportCommand.NAME))
                {
                    var now = DateTime.Now;
                    var exportCommandInput = new ExportCommandInput(now.Year, now.Month);
                    var exportCommand = new ExportCommand(exportCommandInput, _expensesRepository, _expensePrinter);
                    var exportResult = exportCommand.Execute();
                    var exportFileName = exportResult.FilePath;

                    await using Stream stream = File.OpenRead(exportFileName);
                    var inputOnlineFile = new InputOnlineFile(stream, exportFileName);
                    await botClient.SendDocumentAsync(chatId, inputOnlineFile,
                        replyToMessageId: message.MessageId,
                        caption: "Expenses day by day for the month",
                        cancellationToken: cancellationToken);

                    File.Delete(exportFileName);
                }
                else if (command.StartsWith(SumCommandInput.NAME))
                {
                    var text = "Command arguments parsing error";
                    var argsStr = command[SumCommandInput.NAME.Length..];
                    if (SumCommandInput.TryParse(argsStr, out var sumCommandInput))
                    {
                        var sumCommand = new SumCommand(sumCommandInput!, _expensesRepository);
                        var commandTextResult = sumCommand.Execute();
                        text = commandTextResult.Text;
                    }

                    await botClient.SendTextMessageAsync(chatId, text,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }

                return;
            }

            var expensesParsingResult = await _expenseParser.TryParse(messageText);
            _expensesRepository.Save(expensesParsingResult.ParsedExpenses);
            var answerText = GetExpensesParsingAnswerText(expensesParsingResult);
            await botClient.SendTextMessageAsync(chatId: chatId, text: answerText,
                replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
        }

        private string? GetExpensesParsingAnswerText(ExpensesParsingResult expensesParsingResult)
        {
            var parsedExpenses = expensesParsingResult.ParsedExpenses;
            var unparsedLines = expensesParsingResult.UnparsedLines;
            var answerTextBuilder = new StringBuilder()
                .AppendLine($"Parsed count: {parsedExpenses.Length}")
                .AppendLine($"Unparsed count: {unparsedLines.Length}");

            var expensesText = _expensePrinter.ToPlainText(parsedExpenses);
            var expensesAnswerText = string.IsNullOrEmpty(expensesText) ? "No expenses" : expensesText;
            answerTextBuilder
                .AppendLine()
                .AppendLine("Parsed expenses:")
                .AppendLine(expensesAnswerText);

            if (unparsedLines.Any())
            {
                answerTextBuilder.AppendLine("Unparsed lines:");
                foreach (var unparsedLine in unparsedLines)
                {
                    answerTextBuilder.AppendLine(unparsedLine);
                }
            }

            return answerTextBuilder.ToString();
        }
    }
}