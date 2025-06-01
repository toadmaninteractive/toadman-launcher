using CefSharp;
using CefSharp.Wpf;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public partial class BrowserFrame : UserControl
    {
        public bool UseChromium { get; set; }

        string url;
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                if (loaded)
                {
                    Navigate(url);
                }
                else
                {
                    cb.FrameLoadEnd += WebBrowserFrameLoadEnded;
                    cb.Address = url;
                }
            }
        }

        bool loaded = false;

        public BrowserFrame()
        {
            DataContext = this;
            UseChromium = !Core.Utils.CheckEdgeBrowser();

            InitializeComponent();

            if (UseChromium)
            {
                cb.BrowserSettings.ApplicationCache = CefState.Enabled;
                cb.BrowserSettings.Javascript = CefState.Enabled;
                cb.BrowserSettings.WebSecurity = CefState.Enabled;
            }
            else
            {
                loaded = true;
            }
        }

        public async void RequestProperty<T>(string variable, Action<T> callback) where T : IConvertible
        {
            object result = null;
            object response = null;

            if (UseChromium)
            {
                response = await cb.GetMainFrame().EvaluateScriptAsync("(function () { return window." + variable + "; })();").ContinueWith(t => {
                    var r = t.Result;
                    return r.Result?.ToString();
                });
            }
            else
            {
                mshtml.IHTMLDocument2 doc = eb.Document as mshtml.IHTMLDocument2;
                response = doc.parentWindow.GetType().InvokeMember(variable, System.Reflection.BindingFlags.GetProperty, null, doc.parentWindow, new object[] { }).ToString();
            }

            try
            {
                result = Convert.ChangeType(response, typeof(T));
            }
            catch { }


            callback((T)result);
        }

        // response HTML code
        public event EventHandler<string> ContentLoaded;

        public async void Navigate(string url)
        {
            if (UseChromium)
            {
                cb.Address = url;
                string html = await cb.GetSourceAsync();
                ContentLoaded(this, html);
            }
            else
            {
                eb.Navigate(url);
            }
        }

        private async void WebBrowserFrameLoadEnded(object sender, FrameLoadEndEventArgs e)
        {
            loaded = true;
            string html = await cb.GetSourceAsync();
            ContentLoaded(this, html);
        }

        private void eb_Navigated(object sender, NavigationEventArgs e)
        {
            var wb = sender as WebBrowser;
            dynamic doc = wb.Document;
            var htmlText = doc.documentElement.InnerHtml;
            ContentLoaded(this, htmlText);
        }
    }
}
