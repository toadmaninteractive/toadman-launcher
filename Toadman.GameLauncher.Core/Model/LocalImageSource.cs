namespace Toadman.GameLauncher.Core
{
    public struct LocalImageSource
    {
        public string IconMenuImage;
        public string BackgroundImage;

        public LocalImageSource(string iconMenuImage, string backgroundImage)
        {
            IconMenuImage = iconMenuImage;
            BackgroundImage = backgroundImage;
        }
    }
}