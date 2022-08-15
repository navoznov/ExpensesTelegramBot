using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using ExpensesTelegramBot.Telegram.CommandHandlers;
using Telegram.Bot;
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

        public UpdateHandler(IExpensesRepository expensesRepository, IExpenseParser expenseParser)
        {
            _expensesRepository = expensesRepository;
            _expenseParser = expenseParser;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

            var chatId = message.Chat.Id;
            if (messageText.StartsWith("/"))
            {
                var command = messageText[1..].ToLower();
             
                if (command == "help")
                {
                    var helpCommandOutput = await new HelpCommandHandler().Handle();
                    var answer = await botClient.SendTextMessageAsync(chatId, helpCommandOutput, ParseMode.Markdown,
                        replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
                }
                else if (command == "get")
                {
                    const int COUNT = 5;
                    await SendLastExpenses(botClient, chatId, COUNT, cancellationToken);
                }
                else if (command == "getall")
                {
                    await SendAllExpensesByCurrentMonth(botClient, chatId, cancellationToken);
                }
                else if (command.StartsWith("export"))
                {
                    var exportCommandHandler = new ExportCommandHandler(_expensesRepository);
                    var exportFileName = exportCommandHandler.Handle(command);
                    await using Stream stream = File.OpenRead(exportFileName);
                    var inputOnlineFile = new InputOnlineFile(stream, exportFileName);
                    await botClient.SendDocumentAsync(chatId, inputOnlineFile,
                        replyToMessageId: message.MessageId,
                        caption: "Expenses day by day for the month", 
                        cancellationToken: cancellationToken);
                    File.Delete(exportFileName);
                }
                else if (command.StartsWith("sum"))
                {
                    var fields = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    // /sum
                    if (fields.Length == 1)         // /sum
                    {
                        var now = DateTime.Now;
                        var sum = GetExpensesSum(now.Year, now.Month);
                        await botClient.SendTextMessageAsync(chatId, text: sum.ToString(), cancellationToken: cancellationToken);
                    }
                    
                    if (fields.Length == 2)         // /sum 8
                    {
                        var monthStr = fields[1];
                        if (int.TryParse(monthStr, out var month) && month >=1 && month <= 12)
                        {
                            var sum = GetExpensesSum(DateTime.Now.Year, month);
                            var text = sum.ToString();
                            await botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
                        }
                    }
                    else if (fields.Length == 3)    // /sum 2022 8
                    {
                        var yearStr = fields[1];
                        var monthStr = fields[2];
                        if (int.TryParse(yearStr, out var year) && year > 2000 && year < 2100
                            && int.TryParse(monthStr, out var month) && month >=1 && month <= 12)
                        {
                            var sum = GetExpensesSum(year, month);
                            var text = sum.ToString();
                            await botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
                        }
                    }
                }
                
                return;
            }

            var (success, expense) = _expenseParser.TryParse(messageText);
            var answerText = "Parsing error";
            if (success)
            {
                answerText = $"Parsed expense:\n{expense!.Date:yyyy MMM} => {expense.Money} {expense.Description}";
                _expensesRepository.Save(expense);
            }

            await botClient.SendTextMessageAsync(chatId: chatId, text: answerText,
                replyToMessageId: message.MessageId, cancellationToken: cancellationToken);
        }

        private async Task SendAllExpensesByCurrentMonth(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var expenses = _expensesRepository.GetAll(now.Year, now.Month);
            var text = GetExpensesText(expenses);

            await botClient.SendTextMessageAsync(chatId, text: text, cancellationToken: cancellationToken);
        }
        
        private async Task SendLastExpenses(ITelegramBotClient botClient, long chatId, int count,
            CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var expenses = _expensesRepository.GetLastExpenses(count);
            var text = GetExpensesText(expenses);

            await botClient.SendTextMessageAsync(chatId, text: text, cancellationToken: cancellationToken);
        }

        private static string GetExpensesText(IReadOnlyCollection<Expense> expenses)
        {
            if (expenses.Count == 0)
            {
                return "No records";
            }

            var stringBuilder = new StringBuilder();
            foreach (var expense in expenses)
            {
                stringBuilder.Append(expense.Date.ToString("yyyy-MM-dd"));
                stringBuilder.Append('\t');
                stringBuilder.Append(expense.Money);
                stringBuilder.Append('\t');
                stringBuilder.Append(expense.Description);
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
        
        private decimal GetExpensesSum(int year, int month)
        {
            var expenses = _expensesRepository.GetAll(year, month);
            return expenses.Sum(e => e.Money);
        }
    }
}