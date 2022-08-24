using System.Threading.Tasks;
using ExpensesTelegramBot.Repositories;
using ExpensesTelegramBot.Services;
using ExpensesTelegramBot.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Exceptions;

namespace ExpensesTelegramBot
{
    class Program
    {
        private const string token = "5533108915:AAEKMmHb9xl61t3T8EQhQOcZAhnGZoJPlCw";

        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider =  serviceCollection.BuildServiceProvider();
            var bot = serviceProvider.GetService<Bot>();
            await bot!.Run();            
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Bot>(provider, token))
                .AddScoped<IExpensesRepository, CsvExpensesRepository>()
                .AddSingleton<IExpenseParser, ExpenseParser>()
                .AddSingleton<IExpensePrinter, ExpensePrinter>()
                .AddScoped<IUpdateHandler, UpdateHandler>()
                .AddSingleton<ICommandCreator, CommandCreator>()
                ;
        }
    }
}