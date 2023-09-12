using CaseSimulatorBot.Models;
using CaseSimulatorBot.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Commands
{
    internal class MoneyCommands : ApplicationCommandModule
    {
        Random _random;
        public MoneyCommands() 
        {
            _random = new Random();
        }
        [SlashCommand("coin-flip", "Подбросить монетку")]
        public async Task ProfileCommand(InteractionContext ctx, [Option("bet", "Ставка")] long bet)
        {
            var user = DBService.InitializeUser(ctx.Member);
            var random = new Random();

            var gameEmbed = new DiscordEmbedBuilder()
                .WithThumbnail(ctx.User.AvatarUrl)
                .WithImageUrl("https://media.tenor.com/WH5-sVVBlaEAAAAd/toss-coin-flip.gif")
                .AddField("ㅤ", "*Монетка упала именно так, что-ж...\nНадеюсь вам повезло...или...может быть, нет?*");

            var errorMsg = new DiscordEmbedBuilder
            {
                Title = "Ошибка!",
                Color = DiscordColor.Red,
            }.WithThumbnail(ctx.User.AvatarUrl);


            if (bet < 100)
            {

                errorMsg.Description = "*Минимальная ставка - 100$*";
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            if (user!.Money - bet < 0)
            {
                errorMsg.Description = "*Недостаточно средств для совершения дествия!*";
                errorMsg.AddField("Текущий баланс:", user!.Money.ToString(), true)
                    .AddField("Необходимо:", bet.ToString(), true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            int randomNumber = random.Next(0, 2);
            int action = randomNumber == 0 ? -1 : randomNumber;

            if (action == -1)
            {
                gameEmbed.Title = "Вы проиграли!";
                gameEmbed.Color = DiscordColor.Red;
                gameEmbed.Description = $"**С вашего счета будет списано: {bet}$**!";

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(gameEmbed));
            }
            else
            {
                gameEmbed.Title = "Вы выиграли!";
                gameEmbed.Color = DiscordColor.Green;
                gameEmbed.Description = $"**На ваш счет будет начислено: {bet}$**!";

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(gameEmbed));
            }

            DBService.EditUserMoney(user.Id, user.Money + (int)bet * action);


        }
        [SlashCommand("pay", "Перевести деньги пользователю")]
        public async Task PayCommand(InteractionContext ctx, [Option("user", "Пользователь")] DiscordUser user, [Option("sum", "Сумма")] long sum)
        {
            User sender = DBService.InitializeUser(ctx.User)!;
            User accepter = DBService.InitializeUser(user)!;

            if (sender.Id == accepter.Id || user.IsBot)
            {
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);
                errorMsg.Description = "Вы не можете отправить деньги самому себе!\n Возможно вы также пытаетесь отправить деньги бот-аккаунту!";
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            if (sender!.Money - sum < 0)
            {
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);

                errorMsg.Description = "*Недостаточно средств для совершения дествия!*";
                errorMsg.AddField("Текущий баланс:", sender!.Money.ToString(), true)
                    .AddField("Необходимо:", sum.ToString(), true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            if (sum < 10)
            {
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Description = "Минимальная сумма платежа - 10$",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            DBService.EditUserMoney(accepter.Id, accepter.Money + (int)sum);
            DBService.EditUserMoney(sender.Id, sender.Money - (int)sum);

            var succesMsg = new DiscordEmbedBuilder
            {
                Title = "Перевод выполнен успешно!",
                Description = $"Пользователь {ctx.User.Username} перечислил {sum}$ пользователю {user.Username}!",
                Color = DiscordColor.Green,
            }.WithThumbnail("https://   cdn-icons-png.flaticon.com/512/4682/4682662.png");

            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(succesMsg));
        }

        [SlashCommand("duel", "Бросить вызов учатснику")]
        public async Task DuelCommand(InteractionContext ctx, [Option("user", "Пользователь")] DiscordUser user, [Option("sum", "Сумма")] long sum)
        {
            User sender = DBService.InitializeUser(ctx.User)!;
            User accepter = DBService.InitializeUser(user)!;

            if (sender.Id == accepter.Id || user.IsBot)
            {   
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);
                errorMsg.Description = "Вы не можете кинуть вызов самому самому себе!\n Возможно вы также пытаетесь отправить вызов бот-аккаунту!";
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            if (sender!.Money - sum < 0)
            {
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);

                errorMsg.Description = "*Недостаточно средств для совершения дествия!*";
                errorMsg.AddField("Текущий баланс:", sender!.Money.ToString(), true)
                    .AddField("Необходимо:", sum.ToString(), true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            if (sum < 100)
            {
                var errorMsg = new DiscordEmbedBuilder
                {
                    Title = "Ошибка!",
                    Description = "Минимальная сумма ставки - 100$",
                    Color = DiscordColor.Red,
                }.WithThumbnail(ctx.User.AvatarUrl);              
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorMsg).AsEphemeral());
                return;
            }

            var duelRequestMessage = new DiscordEmbedBuilder
            {
                Title = "Дуэль",
                Description = $"<@{sender.Id}> вызывает на дуэль пользователя <@{accepter.Id}>!" // <@{sender.Id}> 
            }
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/3564/3564353.png")
            .AddField("Ставка:", $"**{sum}$**", true)
            .AddField("Шанс выигрыша:", "**50%**", true)
            .WithTimestamp(DateTime.UtcNow);

            var acceptButton = new DiscordButtonComponent(ButtonStyle.Success, "accept_duel_button", "Принять");
            var rejectButton = new DiscordButtonComponent(ButtonStyle.Danger, "reject_duel_button", "Отклонить");

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent(ctx.User.Mention).AddEmbed(duelRequestMessage).AddComponents(acceptButton, rejectButton));

            var accepterMember = await ctx.Guild.GetMemberAsync(user.Id);
            var dm = await accepterMember.CreateDmChannelAsync();

            //var DMChannel = await ctx.Member.CreateDmChannelAsync();
            //await DMChannel.SendMessageAsync(duelRequestMessage);

            await dm.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Пользователь *{ctx.User.Username}* вызывает Вас на дуэль!",
                Description = $"Принять вызов: [*клик*]({msg.JumpLink})",
                Color = DiscordColor.Orange
            }
            .WithAuthor(name: ctx.Guild.Name, iconUrl: ctx.Guild.IconUrl, url: ctx.Guild.SplashUrl)
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/3564/3564353.png")
            .WithTimestamp(DateTime.UtcNow));

            await msg.WaitForButtonAsync(e =>
            {
                if (e.User.Id != user.Id) 
                {
                    return false;
                }
    
                if (e.Id == "accept_duel_button")
                {
                    if (accepter.Money - sum < 0) 
                    {
                        ctx.EditResponseAsync(
                            new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Title = "Ошибка, отмена дуэли!",
                                Description = $"У пользователя <@{accepter.Id}> на балансе меньше средств, чем сумма ставки!",
                                Color = DiscordColor.Red
                            }.WithThumbnail("https://cdn-icons-png.flaticon.com/512/6056/6056831.png")));
                        return true;
                    }

                    int randomNumber = _random.Next(0, 2);
                    var gameEmbed = new DiscordEmbedBuilder 
                    {
                        Color = DiscordColor.Goldenrod
                    }
                    .WithTimestamp(DateTime.UtcNow);


                    if (randomNumber == 0)
                    {
                        var coinEmoji = DiscordEmoji.FromName(ctx.Client, ":coin:");

                        gameEmbed.Title = "Результат дуэли";
                        gameEmbed.Description = $"Пользователь <@{sender.Id}> сделал точный выстрел и победил!";
                        gameEmbed.WithThumbnail(ctx.User.AvatarUrl);
                        gameEmbed.AddField($"Победитель - {ctx.User.Username}:", $"{coinEmoji} Начислено: ``{sum}$``");
                        gameEmbed.AddField($"Проигравший - {e.User.Username}:", $"{coinEmoji} Списано: ``{sum}$``");
                        ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(gameEmbed));

                        DBService.EditUserMoney(sender.Id, sender.Money + (int)sum);
                        DBService.EditUserMoney(accepter.Id, accepter.Money - (int)sum);
                        return true;
                    }
                    else 
                    {
                        var coinEmoji = DiscordEmoji.FromName(ctx.Client, ":coin:");

                        gameEmbed.Title = "Результат дуэли";
                        gameEmbed.Description = $"Пользователь <@{accepter.Id}> сделал точный выстрел и победил!";
                        gameEmbed.WithThumbnail(e.User.AvatarUrl);
                        gameEmbed.AddField($"Победитель - {e.User.Username}:", $"{coinEmoji} Начислено: ``{sum}$``", true);
                        gameEmbed.AddField($"Проигравший - {ctx.User.Username}:", $"{coinEmoji} Списано: ``{sum}$``", true);
                        ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(gameEmbed));

                        DBService.EditUserMoney(accepter.Id, accepter.Money + (int)sum);
                        DBService.EditUserMoney(sender.Id, sender.Money - (int)sum);
                        return true;
                    }
                }
                if (e.Id == "reject_duel_button")
                {

                    ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = "Отказ",
                        Description = $"<@{sender.Id}>, пользователь <@{accepter.Id}> отклонил Ваш запрос на дуэль!",
                        Color = DiscordColor.Red
                    }
                    .WithThumbnail("https://cdn-icons-png.flaticon.com/512/6056/6056831.png")
                    .WithTimestamp(DateTime.UtcNow)));
                    return true;
                }
                return false;
            });
        }

        //[SlashCommand("horny", "...")]
        //public async Task HornyCommand(InteractionContext ctx)
        //{
        //    var packageEmoji = DiscordEmoji.FromName(ctx.Client, ":heart:");
        //    var embed = new DiscordEmbedBuilder()
        //    {
        //        Title = $"Хорни тяночка {packageEmoji}",
        //        Color = DiscordColor.HotPink,
        //        Description = "Уффф..."
        //    }.WithImageUrl("https://media.discordapp.net/attachments/1122553958595051532/1122554745228374096/104874907_p1.jpg?width=455&height=683");

        //    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
        //}      
    }
}
