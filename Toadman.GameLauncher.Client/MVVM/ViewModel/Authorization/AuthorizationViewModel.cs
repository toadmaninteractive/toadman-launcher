using System.Security;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class AuthorizationViewModel : SingUpBaseViewModel
    {
        public ICommand ToRegistrationCommand { get; set; }
        public ICommand ForgotPasswordCommand { get; set; }
        public ICommand LoginCommand { get; set; }       
        public string UserName
        {
            get { return userName; }
            set { SetField(ref userName, value); }
        }

        private string userName;

        public AuthorizationViewModel()
        {
            ClearValues();
        }

        public override void ClearValues()
        {
            UserName = Core.Utils.LoadConfig<Config>().UserName;
            ErrorText = string.Empty;
        }

        public ValidationError Validation(SecureString securePassword)
        {
            var matches = Constants.UserNameRegex.Matches(UserName);
            if (matches.Count == 0)
                return ValidationError.UserNameInvalid;

            if (securePassword.Length == 0)
                return ValidationError.PasswordInvalid;

            return ValidationError.Non;
        }
    }
}