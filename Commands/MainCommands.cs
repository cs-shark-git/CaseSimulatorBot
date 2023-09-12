using CaseSimulatorBot.Cases;
using CaseSimulatorBot.Models;
using CaseSimulatorBot.Utils;
using CaseSimulatorBot.Utils.Images;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Text;

namespace CaseSimulatorBot.Commands
{
    internal class MainCommands : ApplicationCommandModule
    {

        [SlashCommand("profile", "Отображает профиль пользователя")]
        public async Task ProfileCommand(InteractionContext ctx)
        {
            var user = DBService.InitializeUser(ctx.User);
            int balance = user!.Money;
            int rank = DBService.GetUserRank(user.Id);

            string avatar = ctx.User.AvatarUrl;
            string banner = "https://media.discordapp.net/attachments/1093995406972686366/1129391132615385168/7016fd7cdd5d227e7a35db792def6f48.png";
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            int inventoryCost = GetInventoryCost(user);

            ProfileCard profile = new ProfileCard()
            {
                Username = ctx.User.Username,
                AvatarUrl = avatar,
                BackgroundUrl = banner,
                Balance = balance,
                Rank = rank,
                InvetoryCost = inventoryCost
            };

            var bytes = await profile.RenderImageAsync();
            using (Stream stream = new MemoryStream(bytes))               
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile($"{user.Id}_profile.png", stream));
            }
        }

        private int GetInventoryCost(User user)
        {
            List<Item> inventory = DBService.GetUserInventory(user.Id)!;
            if (!inventory.Any())
                return 0;
            return inventory.Select(x => x.Fruit!.Price).Sum();
        }

        [SlashCommand("inventory", "Отображает инвентарь пользователя")]
        public async Task InventoryCommand(InteractionContext ctx)
        {
            int currentPage = 1;
            var user = DBService.InitializeUser(ctx.Member);

            List<Item>? items = DBService.GetUserInventory(user!.Id);
            List<List<Item>> pages = new List<List<Item>>();

            int integerValue = items!.Count() / 35;
            int remainderValue = items!.Count() % 35;

            for (int i = 0; i < integerValue; i++)
            {
                pages.Add(items!.Skip(i * 35).Take(35).ToList());
            }

            if (remainderValue != 0)
            {
                pages.Add(items!.TakeLast(remainderValue).ToList());
            }

            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            using (Stream stream = GetInventoryImage(ctx.User.Username, currentPage, pages, false)) 
            {
                DiscordButtonComponent nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", ">");
                DiscordButtonComponent previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, "previous_page", "<");
                DiscordButtonComponent viewInfoButton = new DiscordButtonComponent(ButtonStyle.Secondary, "view_info", "показать/скрыть информацию");
                var webhookBuilder = new DiscordWebhookBuilder();
                webhookBuilder.AddComponents(previousPageButton, nextPageButton).AddComponents(viewInfoButton);
                webhookBuilder.AddFile($"{ctx.User.Id}_inventory.png", stream);
                var msg = await ctx.EditResponseAsync(webhookBuilder);
                var result = await msg.WaitForButtonAsync(OnPageButtonClick(ctx, currentPage, pages, false));
            }
        }       

        [SlashCommand("sell", "Продажа предметов")]
        public async Task SellCommand(InteractionContext ctx)
        {
            var user = DBService.InitializeUser(ctx.Member);

            var sellEmbed = new DiscordEmbedBuilder
            {
                Title = $"Продажа предметов",
                Color = DiscordColor.DarkButNotBlack,
                Description = "\n*Здесь вы можете продать свои предметы, которые находятся у вас в инвнентаре*",
            };
            sellEmbed.WithThumbnail("https://thumb9.shutterstock.com/mosaic_250/169597876/562242331/stock-vector-vector-illustration-of-large-black-tag-icon-with-white-dollor-sign-562242331.jpg")
                .WithAuthor(name: ctx.User.Username, iconUrl: ctx.User.AvatarUrl);
            sellEmbed.AddField("Опция 1 - 'продать предмет по id'", "При нажатии на кнопку 'продать предмет по id' Вы должны ввести значение уникального идентификатора предмета.\n" +
                "Посмотреть эту информацию в Вашем инвентаре (команда ``/inventory``) нажав на кнопку 'показать/скрыть информацию'. Это удобно, когда Вам нужно продать один предмет очень быстро");
            sellEmbed.AddField("Опция 2 - 'продать предмет по по названию'", "При нажатии на кнопку 'продать предмет по по названию' Вы должны ввести название предмета.\n" +
                "Посмотреть имя предмета можно в Вашем инвентаре (команда ``/inventory``) нажав на кнопку 'показать/скрыть информацию'. Этот варант удобен, когда Вам нужно продать много предметов или Вы знаете только название нужного Вам предмета\n\n" +
                "``Примечание:\n" +
                "Введите количество 0, если хотите, чтобы продались все доступные предметы``");
            await ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());
            DiscordButtonComponent sellItemIdButton = new DiscordButtonComponent(ButtonStyle.Success, "sell_item_id", "продать предмет по id");
            DiscordButtonComponent sellItemNameButton = new DiscordButtonComponent(ButtonStyle.Success, "sell_item_name", "продать предмет по названию");
            DiscordButtonComponent sellAllInventory = new DiscordButtonComponent(ButtonStyle.Danger, "sell_all", "продать весь инвентарь");
            var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(sellEmbed)
                .AddComponents(sellItemIdButton, sellItemNameButton).AddComponents(sellAllInventory));

            await msg.WaitForButtonAsync(OnSellButtonClick(ctx, msg, sellEmbed, sellItemIdButton, sellItemNameButton, sellAllInventory));
        }

        private static Func<ComponentInteractionCreateEventArgs, bool> OnSellButtonClick(InteractionContext ctx,
            DiscordMessage msg,
            DiscordEmbedBuilder sellEmbed,
            DiscordButtonComponent sellItemIdButton,
            DiscordButtonComponent sellItemNameButton,
            DiscordButtonComponent sellAllInventory
            )
        {
            return e =>
            {
                var yesButton = new DiscordButtonComponent(ButtonStyle.Secondary, "confirm", "Да", emoji: new DiscordComponentEmoji(999735395245887488));
                var noButton = new DiscordButtonComponent(ButtonStyle.Secondary, "refuse", "Нет", emoji: new DiscordComponentEmoji(999735393450741780));

                if (e.Interaction.User.Id != ctx.Interaction.User.Id)
                    return false;
                var builder = new DiscordInteractionResponseBuilder();
                builder.WithTitle("Продать предмет");

                if (e.Id == "sell_item_id")
                {
                    builder.WithCustomId("sell_modal_id")
                    .AddComponents(new TextInputComponent("Введите ID предмета", "item_id_input", placeholder: "Например: 1234"));
                }
                else if (e.Id == "sell_item_name")
                {
                    builder.WithCustomId("sell_modal_name")
                    .AddComponents(new TextInputComponent("Введите имя предмета", "item_name_input", placeholder: "Например: Kilo"))
                    .AddComponents(new TextInputComponent("Введите количество предметов", "item_amount_input", placeholder: "Например: 5"));
                }
                else if (e.Id == "sell_all")
                {

                    var confirmEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Вы уверены?",
                        Description = "После данного действия вы продадите все свои предметы, которые находятся у вас в инвентаре!",
                        Color = DiscordColor.Yellow
                    };

                    e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(confirmEmbed).AddComponents(yesButton, noButton));
                    var msg = e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(confirmEmbed).AddComponents(yesButton, noButton)).GetAwaiter().GetResult();
                    msg.WaitForButtonAsync(OnCofirmMessageButtonClick(ctx, sellEmbed, sellItemIdButton, sellItemNameButton, sellAllInventory, e, msg));

                }
                e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, builder);
                msg.WaitForButtonAsync(OnSellButtonClick(ctx, msg, sellEmbed, sellItemIdButton, sellItemNameButton, sellAllInventory));
                return true;
            };
        }

        private static Func<ComponentInteractionCreateEventArgs, bool> OnCofirmMessageButtonClick(InteractionContext ctx, DiscordEmbedBuilder sellEmbed, DiscordButtonComponent sellItemIdButton, DiscordButtonComponent sellItemNameButton, DiscordButtonComponent sellAllInventory, ComponentInteractionCreateEventArgs e, DiscordMessage msg)
        {
            return args =>
            {
                if (args.User.Id != ctx.User.Id)
                    return false;

                if (args.Id == "confirm")
                {

                    var confirmEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Вы уверены?",
                        Description = "После данного действия вы продадите все свои предметы, которые находятся у вас в инвентаре!",
                        Color = DiscordColor.Yellow
                    };

                    User user = DBService.GetUser(ctx.User.Id)!;
                    var items = DBService.GetUserInventory(user.Id);
                    if (items!.Count == 0)
                    {
                        var yesButton = new DiscordButtonComponent(ButtonStyle.Secondary, "confirm", "Да", emoji: new DiscordComponentEmoji(999735395245887488));
                        var noButton = new DiscordButtonComponent(ButtonStyle.Secondary, "refuse", "Нет", emoji: new DiscordComponentEmoji(999735393450741780));

                        var emptyInventoryEmbed = new DiscordEmbedBuilder()
                        {
                            Title = "Ошибка",
                            Description = "Ваш инветарь пуст! Продавать нечего!",
                            Color = DiscordColor.Red
                        }
                        .WithThumbnail(ctx.User.AvatarUrl);

                        args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().AddEmbed(emptyInventoryEmbed).AsEphemeral());

                        var msg1 = e.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder().AddEmbed(confirmEmbed).AddComponents(yesButton, noButton)).GetAwaiter().GetResult();
                        msg1.WaitForButtonAsync(OnCofirmMessageButtonClick(ctx, sellEmbed, sellItemIdButton, sellItemNameButton, sellAllInventory, e, msg));
                        return true;
                    }
                    var sum = items!.Select(x => x.Fruit!.Price).Sum();
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Продано успешно!",
                        Description = $"Пользователь {ctx.User.Username} продал весь свой инвентарь и получил {sum}$",
                        Color = DiscordColor.Green
                    }.WithThumbnail(ctx.User.AvatarUrl);

                    DBService.RemoveItemRange(items!);
                    DBService.EditUserMoney(user.Id, user.Money + sum);
                    e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return true;
                }
                args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(sellEmbed).AddComponents(sellItemIdButton, sellItemNameButton).AddComponents(sellAllInventory));
                var message = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(sellEmbed).AddComponents(sellItemIdButton, sellItemNameButton).AddComponents(sellAllInventory)).GetAwaiter().GetResult();
                message.WaitForButtonAsync(OnSellButtonClick(ctx, msg, sellEmbed, sellItemIdButton, sellItemNameButton, sellAllInventory));
                return true;
            };
        }

        private static Func<ComponentInteractionCreateEventArgs, bool> OnPageButtonClick(InteractionContext ctx, int currentPage, List<List<Item>> pages, bool viewInfo)
        {
            var errorEmbed = new DiscordEmbedBuilder
            {
                Title = $"Ошибка!",
                Color = DiscordColor.Red,
                Description = "\n*Что-то пошло не так...*",
            };

            return e =>
            {
                if (e.Interaction.User.Id != ctx.User.Id)
                    return false;

                DiscordButtonComponent nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", ">");
                DiscordButtonComponent previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, "previous_page", "<");
                DiscordButtonComponent viewInfoButton = new DiscordButtonComponent(ButtonStyle.Secondary, "view_info", "показать/скрыть информацию");
                var webhookBuilder = new DiscordWebhookBuilder();
                webhookBuilder.AddComponents(previousPageButton, nextPageButton).AddComponents(viewInfoButton);

                if (e.Id == "next_page" && currentPage < pages.Count)
                {
                    currentPage += 1;

                }
                else if (e.Id == "previous_page" && currentPage > 1)
                {
                    currentPage -= 1;
                }
                else if (e.Id == "view_info")
                {
                    if (!viewInfo)
                        viewInfo = true;
                    else
                        viewInfo = false;
                }
                else
                {
                    var msg = ctx.EditResponseAsync(webhookBuilder).GetAwaiter().GetResult();
                    e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed).AsEphemeral());
                    msg.WaitForButtonAsync(OnPageButtonClick(ctx, currentPage, pages, viewInfo));
                    return true;
                }

                e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                using (Stream stream = GetInventoryImage(ctx.User.Username, currentPage, pages, viewInfo)) 
                {
                    webhookBuilder.AddFile($"{ctx.User.Id}_inventory.png", stream);
                    var message = ctx.EditResponseAsync(webhookBuilder).GetAwaiter().GetResult();
                    message.WaitForButtonAsync(OnPageButtonClick(ctx, currentPage, pages, viewInfo));
                }                         
                return true;
            };
        }

        private static Stream GetInventoryImage(string username, int currentPage, List<List<Item>> pages, bool viewInfo = false)
        {
            List<Item> itemsPage;
            int pagesCount = pages.Count;
            if (pages.FirstOrDefault() == null)
            {
                itemsPage = new List<Item>();
                pagesCount = 1;
            }
            else
            {
                itemsPage = pages[currentPage - 1];
                pagesCount = pages.Count;
            }


            InventoryImage userInventory = new InventoryImage(viewInfo)
            {
                Username = username,
                CurrentPage = currentPage,
                Pages = pagesCount,
                Items = itemsPage,
            };
            byte[] bytes = userInventory.RenderImageAsync().GetAwaiter().GetResult();           
            return new MemoryStream(bytes);
        }

        [SlashCommand("shop", "Магазин")]
        public async Task ShopCommand(InteractionContext ctx)
        {
            var user = DBService.InitializeUser(ctx.Member);
            var service = new LootCaseService(user!);
            var coinEmoji = DiscordEmoji.FromName(ctx.Client, ":coin:");
            var packageEmoji = DiscordEmoji.FromName(ctx.Client, ":package:");

            var shopEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Magenta,
                Description = "**Описание:**\n*В этом разделе вы можете купить различные товары\nза серверную валюту.*",
            }
            .WithAuthor(name: "Магазин", iconUrl: "https://cdn-icons-png.flaticon.com/512/4564/4564300.png")
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/2611/2611215.png")
            .AddField("ㅤ", "ㅤ")
            .AddField("#1. Открыть кейс", $"*Открывает кейс, из которого\nвыпадает фрукт разной редкости.*")
            .AddField("ㅤ", $"> **{coinEmoji} Стоимость:** ```{service.DefaultCasePrice}$```", inline: true)
            .AddField("ㅤ", $"> **{packageEmoji} Количество:** ```1 шт.```", inline: true)
            .AddField("ㅤ", "ㅤ")
            .AddField("#2. Открыть 10 кейсов", $"*Открывает 10 кейсов, из которых выпадет 10 фруктов,\nсреди которых гарантированно выпадет* `Редкий` или выше *фрукт.*")
            .AddField("ㅤ", $"> **{coinEmoji} Стоимость:** ```{service.DefaultCasePrice * 10}$```", inline: true)
            .AddField("ㅤ", $"> **{packageEmoji} Количество:** ```10 шт.```", inline: true)
            .AddField("ㅤ", "ㅤ")
            .WithFooter(text: $"Пользователь {ctx.User.Username} | Баланс: {user!.Money}$", iconUrl: ctx.User.AvatarUrl);

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var buyFirstButton = new DiscordButtonComponent(ButtonStyle.Secondary, "buy_first", "#1", emoji: new DiscordComponentEmoji(1120293108215730266));
            var buySecondButton = new DiscordButtonComponent(ButtonStyle.Secondary, "buy_second", "#2", emoji: new DiscordComponentEmoji(1120293108215730266));
            var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(shopEmbed).AddComponents(buyFirstButton, buySecondButton));

            await msg.WaitForButtonAsync(OnShopButtonClick(ctx, shopEmbed, buyFirstButton, buySecondButton, service));
        }

        private Func<ComponentInteractionCreateEventArgs, bool> OnShopButtonClick(InteractionContext ctx, DiscordEmbedBuilder shopEmbed, DiscordButtonComponent buyFirstButton, DiscordButtonComponent buySecondButton, LootCaseService service)
        {

            return e =>
            {
                if (e.User.Id != ctx.User.Id)
                    return false;

                User user = DBService.GetUser(ctx.User.Id)!;

                var embed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Купить 1 кейс",
                    Description = $"На Вашем балансе `{user.Money}`$, цена товара - `{service.DefaultCasePrice}$`.\nПосле покупки на Вашем балансе останется `{user.Money - service.DefaultCasePrice}$`,\n Вы уверены?"
                }
                .WithThumbnail(e.Interaction.User.AvatarUrl)
                .AddField("ㅤ", "ㅤ")
                .WithFooter(text: "После покупки кейс сразу же будет открыт", iconUrl: "https://cdn-icons-png.flaticon.com/512/2889/2889897.png");

                var yesButton = new DiscordButtonComponent(ButtonStyle.Secondary, "confirm", "Да", emoji: new DiscordComponentEmoji(999735395245887488));
                var noButton = new DiscordButtonComponent(ButtonStyle.Secondary, "refuse", "Нет", emoji: new DiscordComponentEmoji(999735393450741780));

                if (e.Id == "buy_first")
                {
                    if (user.Money - service.DefaultCasePrice >= 0)
                    {
                        e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder());
                        var msg = ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(yesButton, noButton)).GetAwaiter().GetResult();
                        e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                        msg.WaitForButtonAsync(OnConfirmMessageButtonClick(ctx, user, shopEmbed, buyFirstButton, buySecondButton, e, service));
                    }
                    else
                    {
                        var rejectMsg = new DiscordEmbedBuilder
                        {
                            Title = "Ошибка!",
                            Color = DiscordColor.Red,
                            Description = "*Недостаточно средств для совершения дествия!*",
                        }
                        .AddField("Текущий баланс:", user.Money.ToString(), true)
                        .AddField("Необходимо:", (service.DefaultCasePrice).ToString(), true)
                        .WithThumbnail(e.User.AvatarUrl);
                        e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(rejectMsg)
                            .AsEphemeral());
                        var msg = ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .AddEmbed(shopEmbed).AddComponents(buyFirstButton, buySecondButton)).GetAwaiter().GetResult();
                        msg.WaitForButtonAsync(OnShopButtonClick(ctx, shopEmbed, buyFirstButton, buySecondButton, service));
                    }
                }
                else if (e.Id == "buy_second")
                {
                    if (user.Money - service.DefaultCasePrice * 10 >= 0)
                    {
                        embed.Title = "Купить 10 кейсов";
                        embed.Description = $"На Вашем балансе `{user.Money}`$, цена товара - `{service.DefaultCasePrice * 10}$`.\nПосле покупки на Вашем балансе останется `{user.Money - service.DefaultCasePrice * 10}$`,\n Вы уверены?";
                        e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder());
                        var msg = ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(yesButton, noButton)).GetAwaiter().GetResult();
                        e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                        msg.WaitForButtonAsync(OnConfirmMessageButtonClick(ctx, user, shopEmbed, buyFirstButton, buySecondButton, e, service));
                    }
                    else
                    {
                        var rejectMsg = new DiscordEmbedBuilder
                        {
                            Title = "Ошибка!",
                            Color = DiscordColor.Red,
                            Description = "*Недостаточно средств для совершения дествия!*",
                        }
                        .AddField("Текущий баланс:", user.Money.ToString(), true)
                        .AddField("Необходимо:", (service.DefaultCasePrice * 10).ToString(), true)
                        .WithThumbnail(e.User.AvatarUrl);
                        e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                            .AddEmbed(rejectMsg)
                            .AsEphemeral());
                        var msg = ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .AddEmbed(shopEmbed).AddComponents(buyFirstButton, buySecondButton)).GetAwaiter().GetResult();
                        msg.WaitForButtonAsync(OnShopButtonClick(ctx, shopEmbed, buyFirstButton, buySecondButton, service));
                    }

                }

                return true;
            };
        }

        private Func<ComponentInteractionCreateEventArgs, bool> OnConfirmMessageButtonClick(InteractionContext ctx, User user, DiscordEmbedBuilder shopEmbed, DiscordButtonComponent buyFirstButton, DiscordButtonComponent buySecondButton, ComponentInteractionCreateEventArgs e, LootCaseService service)
        {
            return args =>
            {
                if (args.User.Id != e.User.Id)
                    //OnConfirmMessageButtonClick(ctx, user, shopEmbed, buyFirstButton, buySecondButton, e);
                    return false;

                if (args.Id == "confirm" && e.Id == "buy_first")
                {
                    OpenCaseAsync(ctx, args, service);
                    return true;
                }
                else if (args.Id == "confirm" && e.Id == "buy_second")
                {
                    OpenTenCasesAsync(ctx, args, service);
                    return true;
                }
                args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(shopEmbed).AddComponents(buyFirstButton, buySecondButton));
                var msg = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(shopEmbed).AddComponents(buyFirstButton, buySecondButton)).GetAwaiter().GetResult();
                msg.WaitForButtonAsync(OnShopButtonClick(ctx, shopEmbed, buyFirstButton, buySecondButton, service));
                return true;
            };
        }

        private async void OpenCaseAsync(InteractionContext ctx, ComponentInteractionCreateEventArgs e, LootCaseService service)
        {
            var user = DBService.InitializeUser(ctx.Member);
            int balance = user!.Money;
            var openingEmbed = new DiscordEmbedBuilder
            {
                Title = $"Открываем кейс",
                Color = DiscordColor.Green,
                Description = "*...*",
            };

            CaseImage notOpenedcaseImage = new CaseImage(true);
            byte[] notOpenedCaseBytes = await notOpenedcaseImage.RenderImageAsync();
            using (Stream notOpenedCaseStream = new MemoryStream(notOpenedCaseBytes)) 
            {
                try
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(openingEmbed).AddFile("not_opened_case.png", notOpenedCaseStream));
                }
                catch
                {
                    return;
                }
            }
            DBService.EditUserMoney(ctx.Member.Id, balance - service.DefaultCasePrice);
            LootCaseService caseService = new LootCaseService(user);
            Item item = caseService.OpenDefaultCase();

            CaseImage caseImage = new CaseImage();
            caseImage.Fruit = item.Fruit;

            byte[] bytes = await caseImage.RenderImageAsync();
            using (Stream stream = new MemoryStream(bytes))
            {
                await Task.Delay(5000);

                var resultMsg = new DiscordEmbedBuilder
                {
                    Title = $"{item.Fruit!.Name}",
                    Color = DiscordColor.Green,
                    Description = $"\n*Поздравляю, вам выпал `{item.Fruit!.Name}`!*",
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultMsg).AddFile($"{item.Fruit.Name}_opened_case.png", stream));
            }
        }

        private async void OpenTenCasesAsync(InteractionContext ctx, ComponentInteractionCreateEventArgs e, LootCaseService service)
        {
            var user = DBService.InitializeUser(ctx.Member);
            int balance = user!.Money;
            var openingEmbed = new DiscordEmbedBuilder
            {
                Title = $"Открываем 10 кейсов",
                Color = DiscordColor.Green,
                Description = "*...*",
            };

            CaseImage notOpenedcaseImage = new CaseImage(true);
            byte[] notOpenedCaseBytes = await notOpenedcaseImage.RenderImageAsync();
            using (Stream notOpenedCaseStream = new MemoryStream(notOpenedCaseBytes))
            {
                try
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(openingEmbed).AddFile("not_opened_case.png", notOpenedCaseStream));
                }
                catch
                {
                    return;
                }
            }
            DBService.EditUserMoney(ctx.Member.Id, balance - service.DefaultCasePrice * 10);
            List<Item> items = service.OpenDefaultCaseRange(10).ToList();

            ManyCasesImage casesImage = new ManyCasesImage();
            var fruits = items.Select(x => x.Fruit).ToList()!;
            casesImage.Fruits = fruits!;
            byte[] bytes = await casesImage.RenderImageAsync();
            using (Stream stream = new MemoryStream(bytes))
            {
                await Task.Delay(5000);

                StringBuilder stringNameBuilder = new StringBuilder();
                int costs = 0;

                foreach (var fruit in fruits)
                {
                    stringNameBuilder.Append(" " + fruit!.Name);
                    costs += fruit!.Price;
                }

                var resultMsg = new DiscordEmbedBuilder
                {
                    Title = $"10 фруктов стоимостью {costs}$",
                    Color = DiscordColor.Green,
                    Description = $"\n*Поздравляю, вам выпали:\n `{stringNameBuilder}`!*",
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultMsg).AddFile($"ten_opened_cases.png", stream));
            }
        }
    }
}
