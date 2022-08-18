using System.Threading.Tasks;
using ExpensesTelegramBot.Telegram;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensesTelegramBot
{
    class Program
    {
        private const string token = "5533108915:AAEKMmHb9xl61t3T8EQhQOcZAhnGZoJPlCw";

        static async Task Main(string[] args)
        {
            var serviceProvider =  new ServiceCollection()
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Bot>(provider, token))
                .BuildServiceProvider();
            var bot = serviceProvider.GetService<Bot>();
            await bot.Run();            
        }
    }
}