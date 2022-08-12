using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExpensesTelegramBot.Models;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExpensesTelegramBot.Telegram
{
    public class Bot
    {
        readonly TelegramBotClient _botClient;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUpdateHandler _updateHandler;

        public Bot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _updateHandler = new UpdateHandler(new CsvExpensesRepository(), new ExpenseParser());
            _expensesRepository = new CsvExpensesRepository();
        }

        public async Task Run()
        {
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            _botClient.StartReceiving(
                updateHandler: _updateHandler.HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
            
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
                CancellationToken cancellationToken)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(errorMessage);
                return Task.CompletedTask;
            }
        }

        private async Task SendAllExpensesByCurrentMonth(ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var expenses = _expensesRepository.GetAll(now.Year, now.Month);
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