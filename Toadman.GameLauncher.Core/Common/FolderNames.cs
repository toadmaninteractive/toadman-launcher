using System;
using System.IO;

namespace Toadman.GameLauncher.Core
{
    public static class FolderNames
    {
        public static readonly string AppDataFolderPath;
        public static readonly string AppManifestPath;
        public static readonly string ConfigXmlFilePath;
        public static readonly string LibraryXmlFilePath;
        public static readonly string GameManifestFolderName;
        public static readonly string InterruptedProcessXmlFilePath;

        static FolderNames()
        {
            AppDataFolderPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Constants.AppDataFolderName);

            AppManifestPath =
                Path.Combine(AppDataFolderPath, Constants.AppManifestFileName);

            ConfigXmlFilePath =
                Path.Combine(AppDataFolderPath, Constants.ConfigFileName);

            LibraryXmlFilePath =
                Path.Combine(AppDataFolderPath, Constants.LibraryFileName);

            InterruptedProcessXmlFilePath =
                Path.Combine(AppDataFolderPath, Constants.InterruptedProcess);

            GameManifestFolderName =
                Path.Combine(AppDataFolderPath, Constants.GameManifestFolderName);
        }
    }
}