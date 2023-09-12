using CaseSimulatorBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils.Images
{
    internal class CaseImage : ImageRenderer
    {
        public Fruit? Fruit { get; set; }

        private readonly bool _isNotOpened;

        private readonly int _amount;

        public CaseImage(bool isNotOpened = false, int amount = 1)
        {
            _isNotOpened = isNotOpened;
            _amount = amount;
        }

        public override async Task<byte[]> RenderImageAsync()
        {
            if (_isNotOpened)
            {
                Image image = await LoadNotOpenedCaseImageAsync();

                using (var ms = new MemoryStream())
                {
                    await image.SaveAsPngAsync(ms);

                    image.Dispose();

                    return ms.ToArray();
                }
            }
            Image templateImage = await LoadTemplateImageAsync();
            Image fruitImage = await LoadFruitImageAsync();

            templateImage.Mutate(x => x.DrawImage(fruitImage, new Point(319, 214), 1));

            using (var ms = new MemoryStream())
            {
                await templateImage.SaveAsPngAsync(ms);

                fruitImage.Dispose();
                templateImage.Dispose();

                return ms.ToArray();
            }
        }
        public async Task<Image> LoadFruitImageAsync()
        {
            Image image = await Image.LoadAsync(GetFruitsDirectory()! + "\\" + $"{Fruit!.Name}.png");
            return image;
        }

        private async Task<Image> LoadTemplateImageAsync()
        {
            switch (Fruit!.Rarity)
            {
                case "Common":
                    Image commonCase = await Image.LoadAsync(GetCaseCommonTemplatePath()!);
                    return commonCase;
                case "Uncommon":
                    Image uncommonCase = await Image.LoadAsync(GetCaseUncommonTemplatePath()!);
                    return uncommonCase;
                case "Rare":
                    Image rareCase = await Image.LoadAsync(GetCaseRareTemplatePath()!);
                    return (rareCase);
                case "Legendary":
                    Image legendaryCase = await Image.LoadAsync(GetCaseLegendaryTemplatePath()!);
                    return legendaryCase;
                case "Mythical":
                    Image mythicalCase = await Image.LoadAsync(GetCaseMythicalTemplatePath()!);
                    return mythicalCase;
                default:
                    return null!;
            }
        }

        private async Task<Image> LoadNotOpenedCaseImageAsync()
        {
            Console.WriteLine(GetNotOpenedlCasePath());
            Image commonCase = await Image.LoadAsync(GetNotOpenedlCasePath()!);
            return commonCase;
        }
    }
}
