using System.Security;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class ChangePasswordViewModel : SingUpBaseViewModel
    {
        public ICommand ChangePasswordCommand { get; set; }

        public string Email
        {
            get { return email; }
            set { SetField(ref email, value); }
        }

        private string email;

        public override void ClearValues()
        {
            ErrorText = string.Empty;
        }

        public ValidationError Validation(SecureString password, SecureString repeatePassword)
        {
            var matches = Constants.EmailRegex.Matches(Email);
            if (matches.Count == 0)
                return ValidationError.EmailInvalid;

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