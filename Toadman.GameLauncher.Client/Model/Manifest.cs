using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toadman.GameLauncher.Core;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public class Manifest
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
        private static Manifest instance = null;

        public static Manifest Instance
        {
            get
            {
                if (instance == null)
                    instance = new Manifest();
                return instance;
            }
        }


        public GameItemList LoadFromFile()
        {
            var gameList = new GameItemList();
            
            if (File.Exists(FolderNames.AppManifestPath))
                using (FileStream fsSource = new FileStream(FolderNames.AppManifestPath, FileMode.Open, FileAccess.Read))
                    gameList = Core.Utils.Deserialize<GameItemList>(fsSource);

            if (gameList == null)
                gameList = new GameItemList { Games = new List<GameItem>() };
            
            return gameList;
        }
        
        public  async Task<GameItemList> DownloadAndSaveAppManifest(string sessionId)
        {
            try
            {
                var gameList = await HeliosApi.Provider.DownloadGameInfoList(sessionId);
                for (int i = 0; i < gameList.Games.Count; i++)
                    gameList.Games[i].Description = 
                        Regex.Replace(gameList.Games[i].Description, r, "", RegexOptions.Compiled);

                Core.Utils.SerializeAndSave<GameItemList>(FolderNames.AppManifestPath, gameList);
                return gameList;
            }
            catch (HttpRequestException ex)
            { }
            catch (WebException ex)
            { }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return null;
        }
    }
}