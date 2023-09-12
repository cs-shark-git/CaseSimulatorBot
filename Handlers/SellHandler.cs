using CaseSimulatorBot.Models;
using CaseSimulatorBot.Utils;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CaseSimulatorBot.Handlers
{
    internal class SellHandler
    {
        const string _sellIdModal = "sell_modal_id";
        const string _sellNameModal = "sell_modal_name";

        readonly DiscordEmbedBuilder _succesMessage;

        readonly DiscordEmbedBuilder _errorMessage;

        public SellHandler()
        {
            _succesMessage = new DiscordEmbedBuilder
            {
                Title = $"Предмет продан!",
                Color = DiscordColor.Green,
                Description = "Ваш предмет успешно продан, чтобы обновить инвентарь - используйте /inventory, посмотреть текущий баланс - /profile",
            };

            _errorMessage = new DiscordEmbedBuilder
            {
                Title = $"Ошибка!",
                Color = DiscordColor.Red,
                Description = "Вы не являетесь владельцем этого предмета!",
            };
        }

        public async Task Handle(DiscordClient client, ModalSubmitEventArgs e)
        {
            ValidationService validationService = new ValidationService();

            if (e.Interaction.Data.CustomId == _sellIdModal)
            {
                var value = validationService.ValidateId(e.Values.First().Value);
                if (value == null)
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("Некорректно введённые данные!"))
                        .AsEphemeral());
                    return;
                }

                var itemId = Convert.ToInt32(value);
                Item itemObject = DBService.GetItemById(itemId)!;
                if (itemObject == null)
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("Предмета с таким id не существует!"))
                        .AsEphemeral());
                    return;
                }

                var itemOwner = DBService.GetItemOwner(itemId);
                if (itemOwner.Id == e.Interaction.User.Id)
                {
                    DBService.RemoveItemById(itemId);
                    DBService.EditUserMoney(e.Interaction.User.Id, itemOwner.Money + itemObject!.Fruit!.Price);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(_succesMessage
                        .WithTitle($"Предмет {itemObject.Fruit.Name} продан за {itemObject.Fruit.Price}$"))
                        .AsEphemeral());
                    return;
                }
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(_errorMessage)
                    .AsEphemeral());
            }
            else if (e.Interaction.Data.CustomId == _sellNameModal)
            {
                var name = validationService.ValidateName(e.Values["item_name_input"]);
                var amountValue = validationService.ValidateAmount(e.Values["item_amount_input"]);
                if (name == null || amountValue == null)
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("Некорректно введённые данные!"))
                        .AsEphemeral());
                    return;
                }

                var amount = Convert.ToInt32(amountValue);
                var userObject = DBService.GetUser(e.Interaction.User.Id);

                if (!DBService.IsFruitExistByName(name))
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("Предмета с таким именем не существует!"))
                        .AsEphemeral());
                    return;
                }

                if (!DBService.IsItemExistInUserInventoryByFruitName(e.Interaction.User.Id, name))
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("У вас нет предмета с таким именем в инвентаре!"))
                        .AsEphemeral());
                    return;
                }

                var items = DBService.RemoveItemsFromUserInventoryByFruitName(e.Interaction.User.Id, name, amount);
                if (items != null)
                {
                    if (amount == 0)
                    {
                        amount = items.Count;
                    }

                    int sum = items.Select(x => x.Fruit!.Price).Sum();
                    DBService.EditUserMoney(e.Interaction.User.Id, userObject!.Money + sum);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(_succesMessage
                        .WithTitle($"Предмет {name} в количестве {amount} продан за {sum}$"))
                        .AsEphemeral());
                }
                else
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AddEmbed(_errorMessage.WithDescription("Введенное количество предметов превышает доступное!"))
                        .AsEphemeral());
                }
            }
        }       
    }
}