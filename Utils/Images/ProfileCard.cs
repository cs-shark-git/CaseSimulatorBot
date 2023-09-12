using CaseSimulatorBot.Configs;
using CaseSimulatorBot.Utils.Images;
using DSharpPlus;
using DSharpPlus.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils
{

    internal class ProfileCard : ImageRenderer
    {
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BackgroundUrl { get; set; }
        public int Balance { get; set; }
        public int Rank { get; set; }
        public int InvetoryCost {get; set;}

        public override async Task<byte[]> RenderImageAsync()
        {
            Font roboto64 = GetFont(GetRobotoFontPath()!, 64, FontStyle.Regular);
            Font roboto52 = GetFont(GetRobotoFontPath()!, 52, FontStyle.Regular);

            var (background, avatar, template) = await LoadFilesAsync();

            avatar.Mutate(x => x.ConvertToAvatar());
            background.Mutate(x => x.Resize(1600, 900).BoxBlur(25).DrawCenteredImage(avatar).DrawText(Username, roboto64, Color.White,
                new PointF(CenterTextHorizontal(Username!, background.Width, roboto64),
            background.Height / 2 + (avatar.Height / 2) + 10))
            .DrawImage(template, 1)
            .DrawText(Balance + "$", roboto52, Color.White, new PointF(159, 60))
            .DrawText($"#{Rank}", roboto52, Color.White, new PointF(159, 215))
            .DrawText(InvetoryCost + "$", roboto52, Color.White, new PointF(159, 370)));


            using (var ms = new MemoryStream())
            {
                await background.SaveAsPngAsync(ms);

                template.Dispose();
                background.Dispose();
                avatar.Dispose();

                return ms.ToArray();
            }

        }

        private async Task<(Image, Image, Image)> LoadFilesAsync()
        {
            byte[] backgroundBytes = await DownloadImageAsync(BackgroundUrl!);
            byte[] avatarBytes = await DownloadImageAsync(AvatarUrl!);
            Image background = Image.Load(backgroundBytes);
            Image avatar = Image.Load(avatarBytes);

            string templateImagePath = GetProfileTemplatePath()!;
            Image template = await Image.LoadAsync(templateImagePath);

            return (background, avatar, template);
        }
    }
}
