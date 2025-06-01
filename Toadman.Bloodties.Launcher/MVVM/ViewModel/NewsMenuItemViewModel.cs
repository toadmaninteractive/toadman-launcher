using Toadman.GameLauncher.Core;

namespace Toadman.Bloodties.Launcher
{
    public class NewsMenuItemViewModel : PageBaseViewModel
    {
        public ICommand<int> SelectCommand { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetField(ref isSelected, value); }
        }

        public int Index { get; internal set; }

        private bool isSelected;

        public NewsMenuItemViewModel()
        {
        }
    }
}