using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.GameLauncher.Client
{
    public class DriveSelectorViewModel : NotifyPropertyChanged
    {
        public ICommand InstallCommand { get; set; }

        public List<string> Folders { get; set; } = new List<string>();
        public string SelectedFolder
        {
            get { return selectedFolder; }
            set
            {
                SetField(ref selectedFolder, value);
                CheckAvailableSpace();
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { SetField(ref errorMessage, value); }
        }
        public string SelectedDiskSize
        {
            get { return selectedDiskSize; }
            set { SetField(ref selectedDiskSize, value); }
        }
        public string GameSize { get; set; }

        private string selectedFolder;
        private string errorMessage;
        private string gameGuid;
        private string gameTitle;
        private long installationSize;
        private string selectedDiskSize;
        private SizeFormatConverter converter = new SizeFormatConverter();

        public DriveSelectorViewModel(string gameGuid, string gameTitle, long installationSize)
        {
            this.gameGuid = gameGuid;
            this.gameTitle = gameTitle;
            this.installationSize = installationSize;

            InstallCommand = new RelayCommand(() => { }, () => string.IsNullOrEmpty(ErrorMessage));

            GameSize = converter.ConvertToStr(installationSize);
            Folders = DriveInfo
                .GetDrives()
                .Where(x => x.IsReady && (x.DriveType == DriveType.Fixed || x.DriveType == DriveType.Removable))
                .Select(x => Path.Combine(x.Name, Constants.RootGamesFolder, gameGuid))
                .ToList();

            foreach (var folder in Folders)
            {
                SelectedFolder = folder;
                CheckAvailableSpace();
                if (string.IsNullOrEmpty(ErrorMessage))
                    break; 
            }
        }
        
        public void CheckAvailableSpace()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(SelectedFolder);
            DriveInfo drive = new DriveInfo(directoryInfo.Root.FullName);
            SelectedDiskSize = converter.ConvertToStr(drive.AvailableFreeSpace);

            if (drive.AvailableFreeSpace <= installationSize)
                ErrorMessage = "Insufficient disk space on the selected drive";
            else
                ErrorMessage = string.Empty;
        }
    }
}