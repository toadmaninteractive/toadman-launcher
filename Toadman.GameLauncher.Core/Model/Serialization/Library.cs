using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Toadman.GameLauncher.Core
{
    /// <summary>
    /// Local XML data about installed Games
    /// </summary>
    [XmlRoot(IsNullable = false)]
    public class Library
    {
        [XmlArrayItem("Game", Type = typeof(GameInstallData))]
        public List<GameInstallData> Games { get; set; }

        public Library()
        {
            if (Games == null)
                Games = new List<GameInstallData>();
        }

        public void RemoveGame(string gameGuid)
        {
            if (string.IsNullOrEmpty(gameGuid))
                throw new ArgumentNullException(nameof(gameGuid));

            var gameNode = Games.SingleOrDefault(x => x.GameGuid == gameGuid);
            if (gameNode != null)
            {
                Games.Remove(gameNode);
                Utils.SerializeAndSave<Library>(FolderNames.LibraryXmlFilePath, this);
            }
        }

        public void Add(GameInstallData game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            
            if (Games.Any(x => x.GameGuid == game.GameGuid))
                return;

            Games.Add(game);
            Utils.SerializeAndSave<Library>(FolderNames.LibraryXmlFilePath, this);
        }

        public void Update(
            string gameGuid,
            string branchName = null,
            string branchBuild = null,
            string branchExePath = null,
            string gameDirectory = null)
        {

            var gameNode = Games.Single(x => x.GameGuid == gameGuid);
            
            if (branchName != null)
                gameNode.BranchName = branchName;
            if (branchBuild != null)
                gameNode.BranchBuild = branchBuild;
            if (branchExePath != null)
                gameNode.BranchExePath = branchExePath;
            if (gameDirectory != null)
                gameNode.GameDirectory = gameDirectory;

            Utils.SerializeAndSave<Library>(FolderNames.LibraryXmlFilePath, this);
        }
    }
}