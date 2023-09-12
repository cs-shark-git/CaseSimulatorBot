using CaseSimulatorBot.Configs;

namespace CaseSimulatorBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {           
            Bot bot = new Bot();
            await bot.RunAsync();
        }       
    }
}   