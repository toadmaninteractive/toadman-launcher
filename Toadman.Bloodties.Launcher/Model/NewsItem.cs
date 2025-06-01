namespace Toadman.Bloodties.Launcher
{
    public class NewsItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string ImageSource { get; set; }

        public NewsItem(string title, string text, string imageSource)
        {
            Title = title;
            Text = text;
            ImageSource = imageSource;
        }
    }
}