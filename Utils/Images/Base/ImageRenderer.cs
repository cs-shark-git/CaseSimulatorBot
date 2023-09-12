using CaseSimulatorBot.Configs;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils.Images
{
    internal abstract class ImageRenderer : IImageRenderer
    {
        public abstract Task<byte[]> RenderImageAsync();

        protected Font GetFont(string path, int size, FontStyle style)
        {
            FontCollection collection = new FontCollection();
            FontFamily family = collection.Add(path);
            return family.CreateFont(size, FontStyle.Regular);
        }

        protected float CenterTextHorizontal(string text, int width, Font font)
        {
            return (width / 2) - (TextMeasurer.Measure(text, new TextOptions(font)).Width / 2);
        }

        protected async Task<byte[]> DownloadImageAsync(string? url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            var result = await response.Content.ReadAsByteArrayAsync();
            return result;
        }

        protected string? GetInventoryTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.InventoryTemplate!);
        }

        protected string? GetCaseCommonTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CaseCommonTemplate!);
        }
        protected string? GetCaseUncommonTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CaseUncommonTemplate!);
        }
        protected string? GetCaseRareTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CaseRareTemplate!);
        }
        protected string? GetCaseLegendaryTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CaseLegendaryTemplate!);
        }

        protected string? GetCaseMythicalTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CaseMythicalTemplate!);
        }

        protected string? GetNotOpenedlCasePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.NotOpenedCase!);
        }

        protected string? GetCasesSlotsTemplatePath() 
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.CasesSlotsTemplate!);
        }

        protected string? GetManyCasesTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.ManyCasesTemplate!);
        }
        protected string? GetRobotoFontPath()
        {
            var ctx = new ConfigContext(new FontsConfig());
            var config = ctx.GetInstance() as FontsConfig;
            return config?.RobotoPath;
        }

        protected string? GetProfileTemplatePath()
        {
            var ctx = new ConfigContext(new ImagesConfig());
            var config = ctx.GetInstance() as ImagesConfig;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config?.ProfileTemplate!);
        }

        protected string? GetFruitsDirectory()
        {
            var ctx = new ConfigContext(new SettingsConfig());
            var config = ctx.GetInstance() as SettingsConfig;
            var path = config?.FruitsDirectory!;
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path!);
        }
    }
}
