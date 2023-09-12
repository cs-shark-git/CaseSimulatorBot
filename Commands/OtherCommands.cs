using CaseSimulatorBot.Utils;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CaseSimulatorBot.Commands
{
    internal class OtherCommands : ApplicationCommandModule
    {
        [SlashCommand("about", "О приложении")]
        public async Task AboutCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(new DiscordEmbedBuilder
                {
                    Title = "Информация о продукте",
                    Description = "*Подробная информация о продукте*",
                    Color = DiscordColor.Brown
                }
                .AddField("Разработчик:", "```cs_shark```")
                .AddField("Разработан для сервера:", $"```{ctx.Guild.Name}```")
                .AddField("Платформа:", "```C# 11 & .NET 7.0```")
                .AddField("Использованные фреймворки и библиотеки:", "```DSharpPlus, EF Core 7, Newtonsoft.Json, SixLabors ImageSharp```")
                .AddField("База данных:", "```SQLite3```")    
                .WithThumbnail("https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/Info_icon-72a7cf.svg/2048px-Info_icon-72a7cf.svg.png")
                .WithImageUrl("https://media.tenor.com/W3LndppaSSIAAAAC/hu-tao-hu-tao-genshin.gif")
                .WithFooter($"{ctx.Guild.Name}|Bot - About", "https://cdn-icons-png.flaticon.com/512/7835/7835443.png")
                .WithTimestamp(DateTime.UtcNow)
                ).AsEphemeral());
        }


        [SlashCommand("help", "Помощь")]
        public async Task HelpCommand(InteractionContext ctx) 
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(new DiscordEmbedBuilder 
                {
                    Title = "Перечень команд",
                    Description = "*Основная сводка по всем комнаднам в боте*",
                    Color = DiscordColor.Turquoise
                }
                .WithThumbnail("https://cdn-icons-png.flaticon.com/512/682/682055.png")
                .AddField("/profile", "```Отображает профиль пользователя```", true)
                .AddField("/invetory", "```Отображает информацию о предметах в инвентаре пользователя```")
                .AddField("/shop", "```Открывает меню магазина для приобретения товаров (открытие кейсов доступно через эту команду)```")
                .AddField("/sell", "```Открывает меню продажи предметов, находящихся у пользователя в инвентаре```")
                .AddField("/pay <пользователь> <сумма>", "```Перевести  деньги пользователю```")
                .AddField("/coin-flip <ставка>", "```Бросить монетку на удачу.\nВ случае выигрыша вам зачислиться сумма ставки, в случае проигрыша - спишется.\nШанс выигрыша = 50%```")
                .AddField("/duel <пользователь> <ставка>", "```Вызвать пользователя на дуэль.\nПобедитель дуэли забирает себе сумму ставки, а у проигравшего ставка будет списана.\nШанс выигрыша = 50%```")
                .AddField("/about", "```Показывает подробную информацию о продукте```")
                .AddField("/help", "```Показывает перечень и информацию по всем комнаднам в боте```")
                .WithTimestamp(DateTime.UtcNow)
                ).AsEphemeral());
        }
    }
}
