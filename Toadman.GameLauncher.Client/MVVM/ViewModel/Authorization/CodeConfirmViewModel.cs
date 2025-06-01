using System.Security;
using System.Windows;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class CodeConfirmViewModel : SingUpBaseViewModel
    {
        public ICommand CheckCodeCommand { get; set; }
        public ICommand ResendCodeCommand { get; set; }

        public ConfirmCodeType CodeType { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public SecureString SecurePassword { get; set; }

        public string ButtonCaption
        {
            get { return buttonCaption; }
            set { SetField(ref buttonCaption, value); }
        }

        public Visibility ResendCodeVisibility
        {
            get { return resendCodeVisibility; }
            set { SetField(ref resendCodeVisibility, value); }
        }

        private string buttonCaption;
        private Visibility resendCodeVisibility;

        public override void ClearValues()
        {
            Code = string.Empty;
            ErrorText = string.Empty;
            ResendCodeVisibility = Visibility.Collapsed;
        }
    }
}