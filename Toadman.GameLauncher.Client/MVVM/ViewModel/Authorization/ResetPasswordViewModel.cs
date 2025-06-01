using System.Security;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class ResetPasswordViewModel : SingUpBaseViewModel
    {
        public ICommand ResetPasswordCommand { get; set; }

        public string UserName
        {
            get { return userName; }
            set { SetField(ref userName, value); }
        }

        private string userName;

        public override void ClearValues()
        {
            UserName = Core.Utils.LoadConfig<Config>().UserName;
            ErrorText = string.Empty;
        }

        public ValidationError Validation(SecureString password, SecureString repeatePassword)
        {
            var matches = Constants.UserNameRegex.Matches(UserName);
            if (matches.Count == 0)
                return ValidationError.UserNameInvalid;

            if (password?.Length == 0)
                return ValidationError.PasswordInvalid;

            if (repeatePassword?.Length == 0)
                return ValidationError.RepeatPasswordInvalid;

            if (!Core.Utils.SecureStringEqual(password, repeatePassword))
                return ValidationError.PasswordMismatch;

            return ValidationError.Non;
        }
    }
}