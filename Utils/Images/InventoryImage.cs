using CaseSimulatorBot.Configs;
using CaseSimulatorBot.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace CaseSimulatorBot.Utils.Images
{
    internal class InventoryImage : ImageRenderer
    {

        public required string? Username { get; set; }
        public string? BackgroundUrl { get; set; }
        public required int CurrentPage { get; set; }
        public required int Pages { get; set; }

        public List<Item>? Items { get; set; }

        private bool _viewInfo;

        public InventoryImage(bool viewInfo = false) 
        {
            _viewInfo = viewInfo;
        }

        public override async Task<byte[]> RenderImageAsync()
        {
            List<Image> fruitsIcons = LoadFruitsIcons();

            Font roboto42 = GetFont(GetRobotoFontPath()!, 42, FontStyle.Regular);
            Font roboto28 = GetFont(GetRobotoFontPath()!, 28, FontStyle.Regular);

            var (background, template) = await LoadImagesAsync();

            background.Mutate(x => x.Resize(1200, 800).BoxBlur(25).DrawImage(template, 1));
            background.Mutate(x => x.DrawText($"{Username}", roboto42, Color.White, new PointF(575 + CenterTextHorizontal($"{Username}", 315, roboto42), 45))
            .DrawText($"{CurrentPage}/{Pages}", roboto28, Color.Black, new PointF(1055 + CenterTextHorizontal($"{CurrentPage}/{Pages}", 104, roboto28), 746)));

            int xPos = 170;
            int yPos = 118;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i * 7 + j > fruitsIcons.Count - 1)
                    {
                        break;
                    }
                    background.Mutate(x => x.DrawImage(fruitsIcons[i * 7 + j], new Point(xPos, yPos), 1));
                    xPos += 125;
                }
                yPos += 116;
                xPos = 170;
            }

            using (var ms = new MemoryStream())
            {
                await background.SaveAsPngAsync(ms);

                fruitsIcons.ForEach(x => x.Dispose());
                template.Dispose();
                background.Dispose();

                return ms.ToArray();
            }
        }


        private List<Image> LoadFruitsIcons()
        {            
            var images = new List<Image>();

            foreach (var item in Items!)
            {
                var fruit = MakeImage(item);
                images.Add(fruit);
            }

            return images;
        }

        private Image MakeImage(Item item)
        {
            Font roboto11 = GetFont(GetRobotoFontPath()!, 10, FontStyle.Regular);
            Font roboto12 = GetFont(GetRobotoFontPath()!, 12, FontStyle.Regular);
            Font roboto20 = GetFont(GetRobotoFontPath()!, 20, FontStyle.Regular);
            Image image = Image.Load(GetFruitsDirectory()! + "/" + $"{item.Fruit!.Name}.png");
            var color = new ColorPicker();
            image.Mutate(x => x.Resize(110, 102));
            if (_viewInfo)
                image.Mutate(x => x.Brightness(0.38f)
                .DrawText(item.Fruit.Name, roboto20, color.GetRarityColor(item.Fruit.Rarity!), new PointF(CenterTextHorizontal(item.Fruit.Name!, image.Width, roboto20), image.Height - 80))
                .DrawText(item.Fruit.Price.ToString() + "$", roboto12, Color.LightGreen, 
                new PointF(CenterTextHorizontal(item.Fruit.Name!, image.Width, roboto20) + CenterTextHorizontal(item.Fruit.Price.ToString() + "$",
                Convert.ToInt32(TextMeasurer.Measure(item.Fruit.Name!, new TextOptions(roboto20)).Width), roboto12), image.Height - 56))              
                .DrawText("Item ID: " + item.Id.ToString(), roboto11, Color.White, new PointF(10, image.Height - 14)));

            return image;
        }

        private async Task<(Image, Image)> LoadImagesAsync()
        {
            byte[] backgroundBytes = await DownloadImageAsync("https://img.freepik.com/free-vector/set-torii-gates-water_52683-44986.jpg");
            Image background = Image.Load(backgroundBytes);

            string templateImagePath = GetInventoryTemplatePath()!;
            Image template = await Image.LoadAsync(templateImagePath);

            return (background, template);
        }
    }
}
