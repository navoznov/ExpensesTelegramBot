using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            var messageText = message.Text;
            if (messageText is null)
            {
                return;
            }

            var chatId = message.Chat.Id;
            if (messageText.StartsWith("/"))
            {
                var command = messageText[1..].ToLower();
                if (command.StartsWith("get"))
                {
                    const int COUNT = 5;
                    await SendLastExpenses(botClient, chatId, COUNT, cancellationToken);
                }
                else if (command.ToLower() == "getall")
                {
                    await SendAllExpensesByCurrentMonth(botClient, chatId, cancellationToken);
                }

                return;
            }

            var (success, expense) = _expenseParser.TryParse(messageText);
            var answerText = "Parsing error";
            if (success)
            {
                answerText = $"Parsed expense: {expense!.Money} {expense.Description}";
                _expensesRepository.Save(expense!);
            }

            await botClient.SendTextMessageAsync(chatId: chatId, text: answerText,
                cancellationToken: cancellationToken);
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
    }
}