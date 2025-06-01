using System.Windows;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public abstract class SingUpBaseViewModel : NotifyPropertyChanged
    {
        public ICommand ToBackCommand { get; set; }

        public string ErrorText
        {
            get { return errorText; }
            set
            {
                SetField(ref errorText, value);
                ErrorVisibility = string.IsNullOrWhiteSpace(errorText)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public Visibility ErrorVisibility
        {
            get { return errorVisibility; }
            set { SetField(ref errorVisibility, value); }
        }

        private string errorText;
        private Visibility errorVisibility = Visibility.Collapsed;

        public abstract void ClearValues();
    }
}