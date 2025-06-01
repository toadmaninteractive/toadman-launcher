using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public static class Utils
    {
        public static LocalImageSource GetImageSource(string gameGuid)
        {
            var pathBackground = GetPathImage(gameGuid, "background");
            var pathLogo = GetPathImage(gameGuid, "logo");

            return new LocalImageSource(pathLogo, pathBackground);
        }

        private static string GetPathImage(string gameGuid, string imgFolder)
        {
            var result = $"../../Resources/{imgFolder}/{gameGuid}.png";
            var uri = new Uri(result, UriKind.Relative);

            try
            {
                StreamResourceInfo srcBackground = Application.GetResourceStream(uri);
            }
            catch (Exception)
            {
                result = $"../../Resources/{imgFolder}/default.png";
            }

            return result;
        }

        public static System.Windows.Controls.MenuItem GetMenuItem(string header)
        {
            return new System.Windows.Controls.MenuItem
            {
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                Header = header,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Comfortaa"),
                FontSize = 12
            };
        }

        public static List<string> GetExpiredGameGuids(GameItemList actualGamesInfo)
        {
            var result = new List<string>();
            var lib = Core.Utils.LoadLibrary();
                
            foreach (var localGameInfo in lib.Games)
            {
                var actualGameInfo = actualGamesInfo.Games.SingleOrDefault(x => x.Guid == localGameInfo.GameGuid);
                if (actualGameInfo == null)
                    continue;

                var actualBranchInfo = actualGameInfo.Branches.SingleOrDefault(x => x.Name == localGameInfo.BranchName);
                if (actualBranchInfo == null)
                    continue;

                if (actualBranchInfo.Build != localGameInfo.BranchBuild)
                    result.Add(localGameInfo.GameGuid);
            }
            return result;
        }

        public static void LogToChronos(string level, string message)
        {
            /*
            try
            {
                var config = Core.Utils.LoadConfig<Config>();
                if (config == null)
                    return;

                ChronosApi.Init(
                    Constants.AppName,
                    config.Channel.ToString(),
                    "client");

                var formattedMessage = message
                    .Replace(@"\", @"/")
                    .Replace("\r", "")
                    .Replace("\n", @"\n");

                var logLevel = LogLevel.FromString(level);

                if (logLevel == LogLevel.Fatal)
                    ChronosApi.Log(
                        formattedMessage, 
                        GetChronosMetadata(config.UserName, config.Channel.ToString()),
                        ChronosApi.LogLevel.fatal);
                else if (logLevel == LogLevel.Error)
                    ChronosApi.Log(
                        formattedMessage, 
                        GetChronosMetadata(config.UserName, config.Channel.ToString()), 
                        ChronosApi.LogLevel.error);
                else if (logLevel == LogLevel.Warn)
                    ChronosApi.Log(
                        formattedMessage, 
                        GetChronosMetadata(config.UserName, config.Channel.ToString()), 
                        ChronosApi.LogLevel.warning);
            }
            catch (Exception)
            { }
            */
        }

        private static Dictionary<string, string> GetChronosMetadata(string userName, string updateChannel)
        {
            return new Dictionary<string, string>
            {
                { "version",  Core.Utils.GetAppVersion() },
                { "revision", Core.Utils.GetCurrentRevision() ?? "None" },
                { "channel",  updateChannel ?? "None" },
                { "username", userName ?? "None" }
            };
        }
    }
}