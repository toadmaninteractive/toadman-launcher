using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public partial class GameLibraryViewModel
    {
        private void GameListUpdate(List<GameItem> gamesInfo)
        {
            Games = new ObservableCollection<GameViewModel>();

            var library = Core.Utils.LoadLibrary();
            //remove install data for game without folder and files
            foreach (var installData in Core.Utils.LoadLibrary().Games)
                if (!Directory.Exists(installData.GameDirectory)
                    || Directory.GetFiles(installData.GameDirectory, "*.*", SearchOption.AllDirectories).Length == 0)
                    library.RemoveGame(installData.GameGuid);

            library = Core.Utils.LoadLibrary(); //Reload actual state
            foreach (var gameInfo in gamesInfo.Where(x => x.Branches.Count > 0))
            {
                var installData = library.Games.SingleOrDefault(x => x.GameGuid == gameInfo.Guid);

                //remove install data of not purchased game
                if (gameInfo.Ownership == GameOwnership.None
                    && !string.IsNullOrWhiteSpace(installData?.GameDirectory))
                {
                    library.RemoveGame(gameInfo.Guid);
                    installData = null;
                }

                var gameModel = new GameModel(
                    gameInfo,
                    installData?.GameDirectory,
                    installData?.BranchName,
                    installData?.BranchBuild);

                var imageSource = Utils.GetImageSource(gameInfo.Guid);
                var gameVM = new GameViewModel(
                    gameModel,
                    imageSource.IconMenuImage,
                    imageSource.BackgroundImage);

                gameVM.OnInstallStatusChanged += GameInstallStatusChangedHandler;

                Games.Add(gameVM);
            }

            if (Games.Count > 0)
                SelectedGame = Games[0];

            RaisePropertyChanged(nameof(Games));
        }

        private void RestoreInstallProcesses()
        {
            var interruptedProcesses = Core.Utils.LoadInterruptedProcess();

            foreach (var interruptedProcess in interruptedProcesses.InterruptedProcesses)
            {
                var gameVM = Games.SingleOrDefault(x => x.Model.Guid == interruptedProcess.GameGuid);
                if (gameVM == null)
                    continue;

                if (gameVM.Model.InstallStatus != GameInstallStatus.NotInstalled)
                {
                    logger.Warn($"Restore install for {gameVM.Model.Guid} in state ({gameVM.Model.InstallStatus}).");
                    continue;
                }

                gameVM.RestoreInstall(interruptedProcess.LocalPath).Track();
            }
        }

        private void GameInstallStatusChangedHandler(IGameModel sender)
        {
            if (SelectedGame.EqualModel(sender))
            {
                var newGameVM = new GameViewModel(
                    sender,
                    selectedGame.IconMenuImageSource,
                    selectedGame.BackgroundImageSource);
                newGameVM.OnInstallStatusChanged += GameInstallStatusChangedHandler;

                var found = Games.Single(x => x.Model.Guid == sender.Guid);
                int i = Games.IndexOf(found);
                Games[i] = newGameVM;
                SelectedGame = newGameVM;
            }
        }
    }
}