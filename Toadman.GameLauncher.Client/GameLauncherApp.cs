using NLog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public class GameLauncherApp
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private GameLibraryView gameLibraryView = new GameLibraryView();
        private GameLibraryViewModel gameLibraryVM;

        public GameLauncherApp()
        {
            gameLibraryVM = new GameLibraryViewModel();
            gameLibraryVM.SignInChange += OnSignInChange;
            gameLibraryView.Closing += ClosingHandler;
            gameLibraryView.DataContext = gameLibraryVM;

            App.Current.DispatcherUnhandledException += UnhandledException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;

            logger.Info("Start");
        }
        
        public async Task Run(bool isSilent = false)
        {
            gameLibraryVM.IsGameLoad = false;
            await SessionRefrash();
            Login(isSilent).Track();
            CheckUpdatesRoutine().Track();
        }

        public void RestoreFromTray()
        {
            Login(false).Track();
        }

        private void OnSignInChange()
        {
            Login(false).Track();
        }

        private async Task Login(bool isSilent)
        {
            bool isLoggedIn = false;
            try
            {
                isLoggedIn = await TryAuthorization(isSilent);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.NameResolutionFailure)
                    throw;
            }
            catch (HttpRequestException ex)
            {
                var webEx = ex.GetBaseException() as WebException;
                if (webEx == null || webEx.Status != WebExceptionStatus.NameResolutionFailure)
                    throw;
            }

            if (!isSilent)
            {
                gameLibraryView.Show();
                gameLibraryView.WindowState = WindowState.Normal;
            }
            
            var config = Core.Utils.LoadConfig<Config>();
            gameLibraryVM.LoadGames(config.UserName, config.SessionId).Track();
        }

        private async Task<bool> TryAuthorization(bool isSilent)
        {
            bool isLoggedIn;

            do
            {
                var config = Core.Utils.LoadConfig<Config>();
                var status = await HeliosApi.Provider.VerificationSessionId(config.SessionId);
                isLoggedIn = status.LoggedIn;
                if (!isLoggedIn)
                {
                    if (isSilent)
                        break;
                    
                    gameLibraryView.Hide();

                    AuthorizationView authorizationView = new AuthorizationView(AuthorizationScreen.Authorization);
                    var authorizationCancel = !(authorizationView.ShowDialog() ?? false);
                    if (authorizationCancel)
                        Application.Current.Shutdown();
                }
            } while (!isLoggedIn);

            return isLoggedIn;
        }

        private async Task SessionRefrash()
        {
            var config = Core.Utils.LoadConfig<Config>();

            try
            {
                var response = await HeliosApi.Provider.VerificationSessionId(config.SessionId);
                if (response.LoggedIn)
                    return;
            }
            catch (Exception)
            { }

            try
            {
                if (string.IsNullOrEmpty(config.UserName) || (config.SecurePassword?.Length ?? 0) == 0)
                    return;

                var response = await HeliosApi.Provider.GetSessionIdByCredential(config.UserName, config.SecurePassword);
                if (response.Result)
                {
                    config.SessionId = response.SessionId;
                    config.Save();
                }
            }
            catch (Exception)
            { }
        }

        public void ClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            gameLibraryView.WindowState = WindowState.Minimized;
            gameLibraryView.Hide();
            e.Cancel = true;
        }

        private void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception.GetBaseException();

            if (ex is WebException)
            {
                switch (((WebException)ex).Status)
                {
                    case WebExceptionStatus.RequestCanceled:                    
                        break;
                    default:
                        logger.Error(ex);
                        break;
                }
            }
            else
                logger.Error(ex);

            e.Handled = true;
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception.GetBaseException();
            
            var cancelEx = ex as WebException;
            if (cancelEx != null && cancelEx.Status == WebExceptionStatus.RequestCanceled)
                return;            

            logger.Error(ex);
        }
    
        private async Task CheckUpdatesRoutine()
        {
            do
            {
                try
                {
                    ApplicationUpdater appUpdater = new ApplicationUpdater();

                    var config = Core.Utils.LoadConfig<Config>();
                    var actualRev = await appUpdater.GetRemoteRevisionAsync(config.Channel);
                    var currentRev = Core.Utils.GetCurrentRevision();

                    if (string.IsNullOrEmpty(currentRev) || actualRev != currentRev)
                    {
                        if (!appUpdater.CheckForSetupFile(config.Channel, actualRev))
                            await appUpdater.DownloadUpdatesAsync(config.Channel, actualRev);

                        if (!string.IsNullOrEmpty(appUpdater.UpdateFileName))
                        {
                            gameLibraryVM.HasUpdate = true;
                            gameLibraryVM.AppUpdateFileName = appUpdater.UpdateFileName;
                        }
                    }
                    else
                        gameLibraryVM.HasUpdate = false;

                    //====================================================================================================
                    
                    var actualGamesInfo = await Manifest.Instance.DownloadAndSaveAppManifest(config.SessionId);
                    if (actualGamesInfo != null)
                    {
                        if (gameLibraryVM.IsGameLoad)
                        {
                            foreach (var gameVM in gameLibraryVM.Games)
                            {
                                var actualGameInfo = actualGamesInfo.Games.SingleOrDefault(x => x.Guid == gameVM.Model.Guid);
                                if (actualGameInfo != null)
                                    gameVM.Model.SetGameInfo(actualGameInfo);
                            }

                            var expiredGameGuids = Utils.GetExpiredGameGuids(actualGamesInfo);
                            foreach (var gameVM in gameLibraryVM.Games.Where(x => expiredGameGuids.Contains(x.Model.Guid)))
                                gameVM.HasUpdate = true;
                        }
                        else
                            await Login(false);
                    }
                }
                catch (WebException) { }
                catch (SocketException) { }
                catch (HttpRequestException ex)
                {
                    var webEx = ex.GetBaseException() as WebException;
                    if (webEx == null || webEx.Status != WebExceptionStatus.NameResolutionFailure)
                        throw;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    await Task.Delay(Constants.CheckUpdateInterval);
                }
                
            } while (true);
        }
    }
}