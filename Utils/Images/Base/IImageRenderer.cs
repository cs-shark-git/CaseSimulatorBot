namespace CaseSimulatorBot.Utils.Images
{
    internal interface IImageRenderer
    {
        public Task<byte[]> RenderImageAsync();
    }
}
