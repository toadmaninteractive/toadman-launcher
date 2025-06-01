using CefSharp;
using MahApps.Metro.Controls;
using NLog;
using System;
using System.Linq;
using System.Windows.Media;

namespace Toadman.GameLauncher.Client
{
    /// <summary>
    /// Interaction logic for PurchaseView.xaml
    /// </summary>
    public partial class PurchaseView : MetroWindow
    {
        public PurchaseView(string gameGuid)
        {
            InitializeComponent();

            Browser.ContentLoaded += ContentCallback;

            var username = Core.Utils.LoadConfig<Config>().UserName;

            Browser.Url = $"https://{Core.Constants.HeliosHost}/stripe/form?username={username}&guid={gameGuid}";

            DataContext = this;

            //Browser.Url = $"https://www.google.com/recaptcha/api2/demo";
            //Browser.Url = $"https://{Core.Constants.HeliosHost}/validation/captcha";
        }

        private void ContentCallback(object sender, string content)
        {
            if (content == null) return;

            bool? res = null;
            var body = content
                .Split(new[] { "<body>" }, StringSplitOptions.None)
                .Last()
                .Split(new[] { "</body>" }, StringSplitOptions.None)
                .First()
                .Trim().ToLower();


            if (body == "success")
            {
                res = true;
            }
            else if (body.Replace(" ", "") == "<prestyle=\"word-wrap:break-word;white-space:pre-wrap;\"></pre>")
            {
                res = false;
            }

            this.Invoke(delegate
            {
                if (!res.HasValue)
                    return;

                if (res.Value)
                    DialogResult = true;
                else if (!res.Value)
                    ErrorText.Visibility = System.Windows.Visibility.Visible;
            });
        }

    }
}
