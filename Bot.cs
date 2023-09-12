using CaseSimulatorBot.Commands;
using CaseSimulatorBot.Configs;
using CaseSimulatorBot.Handlers;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;

namespace CaseSimulatorBot
{
    internal class Bot  
    {
        public DiscordClient Client { get; }

        public Bot()
        {
            TokenConfig? tokenConf = new ConfigContext(new TokenConfig()).GetInstance() as TokenConfig;
            
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = tokenConf?.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });
        }

        public async Task RunAsync()
        {
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<MainCommands>();
            slash.RegisterCommands<MoneyCommands>();
            slash.RegisterCommands<OtherCommands>();
            Client!.Ready += (s, e) => Task.CompletedTask;
            slash.SlashCommandErrored += async (ex, args) => 
            {
                await Task.Run(() =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(args.Exception+"\n");
                    Console.ForegroundColor = ConsoleColor.White;
                });
            };
            SellHandler sellHandler = new SellHandler();
            Client.ModalSubmitted += sellHandler.Handle;

            MessageHandler messageHandler = new MessageHandler();
            Client.MessageCreated += messageHandler.Handle;

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }       
    }
}