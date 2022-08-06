
namespace HelloWorld
{
    class Program
    {
        private const string token = "5533108915:AAEKMmHb9xl61t3T8EQhQOcZAhnGZoJPlCw";

        static async Task Main(string[] args)
        {
            var bot = new Bot(token);
            await bot.Run();            
        }
    }
}