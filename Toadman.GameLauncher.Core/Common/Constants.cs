using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace Toadman.GameLauncher.Core
{
    public static class Constants
    {
        public const int ReconnectDelay = 1000 * 5; // 5 sec
        public const long ChunkSize = 1024 * 1024 * 32; // 32 Mb
        public const long ChunkingFileSize = 1024 * 1024 * 128; // 128 Mb
        public const string ChunkExtension = ".chunk";
        public const string InvalidMergeChunks = ".mergechunks";
        public const string InvalidCompressedExtension = ".compress";
        public const string InvalidDownloadExtension = ".download";
        public const string CompressedExtension = ".gz";
        public const string RegistryAutoRunPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string LibraryFileName = "library.xml";
        public const string ConfigFileName = "config.xml";
        public const string GameManifestFolderName = "GameManifests";
        public const string InterruptedProcess = "interruptedProcess.xml";
        public const string HeaderNameSession = "X-Session-Id";
        public const string HeliosHost = "helios.yourcompany.com";
        public static Regex UserNameRegex => new Regex("^(?=.{4,32}$)[A-Za-z]+([A-Za-z0-9]|([._-])(?![._-]))+(?<![._-])$", RegexOptions.Singleline);
        public static Regex EmailRegex => new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", RegexOptions.Singleline);

        public static readonly string AppManifestFileName = "AppManifes.xml";

        public static readonly string AppGuid;
        public static readonly string AppName;
        public static readonly int DefaultConnectionLimit;
        public static readonly string RootGamesFolder;
        public static readonly string AppDataFolderName;
        public static readonly string ApiUrl;
        public static readonly string UpdateUrl;
        public static readonly int CheckUpdateInterval;
        public static readonly string SetupFileName;
        public static readonly string TempFolderName;
        public static readonly string TempSetupFileName;
        public static readonly string BloodtiesExeRelPath;

        static Constants()
        {
            AppGuid = ConfigurationManager.AppSettings["AppGuid"];
            AppName = ConfigurationManager.AppSettings["AppName"];
            DefaultConnectionLimit = int.Parse(ConfigurationManager.AppSettings["DefaultConnectionLimit"]);
            RootGamesFolder = ConfigurationManager.AppSettings["RootGamesFolder"];
            var appDataRoot = ConfigurationManager.AppSettings["AppDataRootFolderName"];
            var appDataFolder = ConfigurationManager.AppSettings["AppDataFolderName"];
            AppDataFolderName = Path.Combine(appDataRoot, appDataFolder);
            ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
            UpdateUrl = ConfigurationManager.AppSettings["UpdateUrl"];
            CheckUpdateInterval = int.Parse(ConfigurationManager.AppSettings["CheckUpdateInterval"]);
            TempFolderName = ConfigurationManager.AppSettings["TempFolderName"];
            TempSetupFileName = ConfigurationManager.AppSettings["TempSetupFileName"];
            SetupFileName = ConfigurationManager.AppSettings["SetupFileName"];
            BloodtiesExeRelPath = ConfigurationManager.AppSettings["BloodtiesExeRelPath"];
        }
    }
}