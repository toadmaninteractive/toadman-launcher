using System;
using System.Collections.Generic;
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
using WebProtocol;

namespace Toadman.GameLauncher.Client.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для CaptchaFrame.xaml
    /// </summary>
    public partial class CaptchaFrame : UserControl
    {
        public CaptchaResponse CaptchaResponse;
        public string CaptchaAnswer
        {
            get { return captchaInput.Text; }
            set { captchaInput.Text = value; }
        }

        public CaptchaFrame()
        {
            InitializeComponent();
            ResetCaptcha();
        }

        private void Button_Reset(object sender, RoutedEventArgs e)
        {
            ResetCaptcha();
        }

        public void ResetCaptcha()
        {
            captchaInput.Text = "";
            RequestCaptcha().Track();
        }

        private async Task RequestCaptcha()
        {
            CaptchaResponse = await HeliosApi.Provider.RequestCaptcha();
            captchaImage.Source = new BitmapImage(new Uri(Constants.ApiUrl + "captcha/" + CaptchaResponse.Filename));
        }
    }
}
