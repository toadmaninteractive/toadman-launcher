using System.Security;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class RegistrationViewModel : SingUpBaseViewModel
    {
        public ICommand RegistrationCommand { get; set; }

        public string UserName
        {
            get { return userName; }
            set { SetField(ref userName, value); }
        }
        public string Email
        {
            get { return email; }
            set { SetField(ref email, value); }
        }
        public SecureString SecurePassword { get; private set; }
        public bool IsShowUsernameRules
        {
            get { return isShowUsernameRules; }
            set { SetField(ref isShowUsernameRules, value); }
        }

        private string userName;
        private string email;
        private bool isShowUsernameRules;

        public override void ClearValues()
        {
            ErrorText = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
        }

        public ValidationError Validation(SecureString securePassword, SecureString repeatSecurePassword)
        {            
            var matchesUserName = Constants.UserNameRegex.Matches(UserName);
            IsShowUsernameRules = matchesUserName.Count == 0;
            if (matchesUserName.Count == 0)
                return ValidationError.UserNameInvalid;

            var matchesEmail = Constants.EmailRegex.Matches(Email);
            if (matchesEmail.Count == 0)
                return ValidationError.EmailInvalid;

            if (securePassword?.Length == 0)
                return ValidationError.PasswordInvalid;

            if (repeatSecurePassword?.Length == 0)
                return ValidationError.RepeatPasswordInvalid;

            if (!Core.Utils.SecureStringEqual(securePassword, repeatSecurePassword))
                return ValidationError.PasswordMismatch;
            
            SecurePassword = securePassword;
            IsShowUsernameRules = false;
            return ValidationError.Non;
        }
    }
}