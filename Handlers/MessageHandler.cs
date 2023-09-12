using CaseSimulatorBot.Models;
using CaseSimulatorBot.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Handlers
{
    internal class MessageHandler
    {
        public async Task Handle(DiscordClient client, MessageCreateEventArgs e)
        {
            User user = DBService.InitializeUser(e.Message.Author)!;

            if (e.Channel.Id != 1093437511700844574 || e.Message.Author.IsBot)
                return;

            DBService.EditUserMessageAmount(user.Id, user.MessageAmount + 1);
            if (user.MessageAmount >= 20)
            {
                DBService.EditUserMessageAmount(user.Id, 1);
                DBService.EditUserMoney(user.Id, user.Money + 50);

                var congratulationsEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"Пользователь {e.Author.Username} заработал 50$!",
                    Description = "За каждые 20 сообщений в канале <#1093437511700844574> вы получаете по 50$!",
                    Color = DiscordColor.SpringGreen
                }.WithThumbnail(e.Author.AvatarUrl);
                var channel = await client.GetChannelAsync(1129412516406112397);
                await channel.SendMessageAsync(congratulationsEmbed);
            }
        }
    }
}
