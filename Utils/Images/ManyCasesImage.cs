using CaseSimulatorBot.Models;
using SixLabors.ImageSharp.Drawing.Processing;

namespace CaseSimulatorBot.Utils.Images
{
    internal class ManyCasesImage : ImageRenderer
    {
        public List<Fruit>? Fruits { get; set; }

        public override async Task<byte[]> RenderImageAsync()
        {
            List<Image> fruitsImages = LoadFruitsIcons().ToList();
            var (casesSlots, manyCases) = await LoadTemplateImagesAsync();

            int xPos = 114;
            int yPos = 85;

            ColorPicker picker = new ColorPicker();
            Rectangle rectangle = new Rectangle(xPos, yPos, 131, 131);


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i * 4 + j > fruitsImages.Count - 1)
                        break;

                    if (i == 2 && j == 0)
                        xPos += 146;

                    var color = picker.GetRarityColor(Fruits![i * 4 + j].Rarity!);
                    rectangle.X = xPos;
                    rectangle.Y = yPos;

                    manyCases.Mutate(x => x.Fill(color, rectangle));
                    manyCases.Mutate(x => x.DrawImage(fruitsImages[i * 4 + j], new Point(xPos + 6, yPos + 12), 1));
                    xPos += 146;
                }
                yPos += 150;
                xPos = 114;
            }
            manyCases.Mutate(x => x.DrawImage(casesSlots, 1));

            using (var ms = new MemoryStream())
            {
                await manyCases.SaveAsPngAsync(ms);

                fruitsImages.ForEach(x => x.Dispose());
                casesSlots.Dispose();
                manyCases.Dispose();

                return ms.ToArray();
            }
        }

        private IEnumerable<Image> LoadFruitsIcons()
        {
            if (Fruits!.Count > 10)
                throw new InvalidOperationException("'Fruits' field length must be less than or equal to 10!");

            var names = Fruits!.Select(x => x!.Name);
            var images = new List<Image>();
            var dir = GetFruitsDirectory()!;

            foreach (var fruit in Fruits)
            {
                var img = MakeImage(fruit);
                yield return img;
            }
        }

        private Image MakeImage(Fruit fruit)
        {
            Image image = Image.Load(GetFruitsDirectory()! + "\\" + $"{fruit.Name}.png");
            image.Mutate(x => x.Resize(110, 102));
            return image;
        }

        private async Task<(Image, Image)> LoadTemplateImagesAsync()
        {
            Image casesSlots = await Image.LoadAsync(GetCasesSlotsTemplatePath()!);
            Image manyCases = await Image.LoadAsync(GetManyCasesTemplatePath()!);
            return (casesSlots, manyCases);
        }
    }
}
