using System;
using System.Linq;
using System.Collections.Generic;
using WebProtocol;

namespace Toadman.GameLauncher.Client
{
    public static class FileServerMock
    {
        public static bool ImmortalBetaLocked = true;
        public static bool ChessBetaLocked = false;

        public static GameItemList GetGameListMock()
        {
            var ImmortalBranches = new List<GameBranchItem>
            {
                new GameBranchItem
                {
                    Name = "master",
                    Build = "2.1",
                    BuildTime = "21.08.2018",
                    CompressedSize = 4408627,
                    Size = 16632840,
                    ExePath = "immortal.exe",
                    IsDefault = true,
                    LogPath = "logs.txt",
                    RootUrl = "http://tl.yourcompany.com/chess/4/"
                }
            };

            if (!ImmortalBetaLocked)
                ImmortalBranches.Add(new GameBranchItem
                {
                    Name = "beta",
                    Build = "48",
                    BuildTime = "29.08.2018",
                    CompressedSize = 4290805,
                    Size = 13942520,
                    ExePath = "immortal.exe",
                    IsDefault = false,
                    LogPath = "logs.txt",
                    RootUrl = "http://tl.yourcompany.com/chess/4/"
                });

            //============================================================

            var chessBranches = new List<GameBranchItem>
            {
                new GameBranchItem
                {
                    Name = "master",
                    Build = "5",
                    BuildTime = "1.02.2017",
                    CompressedSize = 5508257,
                    Size = 5663284400,
                    ExePath = "chess.exe",
                    IsDefault = true,
                    LogPath = "logs.txt",
                    RootUrl = "http://tl.yourcompany.com/chess/4/"
                }
            };

            if (!ChessBetaLocked)
                chessBranches.Add(new GameBranchItem
                {
                    Name = "beta",
                    Build = "8",
                    BuildTime = "1.02.2017",
                    CompressedSize = 4408627,
                    Size = 2663284400,
                    ExePath = "chess.exe",
                    IsDefault = false,
                    LogPath = "logs.txt",
                    RootUrl = "http://tl.yourcompany.com/chess/4/"
                });
            //============================================================

            return new GameItemList
            {
                Games = new List<GameItem>
                {
                    new GameItem
                    {
                        Guid = "immortal",
                        Title = "Immortal",
                        Description = "Immortal: Unchained is the first hardcore shooter for the Action-RPG genre. Guard yourself with a range of unique guns, armor and special weapons as you fight your way across the nine worlds to find the heart of decay."
                                        + Environment.NewLine + Environment.NewLine +
                                        "Immortal is Toadman’s most extensive title so far and their first own title. The game falls within the AAA game classification and will be out in 2018.",

                        Ownership = GameOwnership.None,
                        OwnershipUntil = string.Empty,
                        Price = new Price { Amount = 15, Currency = Currency.Usd },
                        Branches = ImmortalBranches
                    },
                    new GameItem
                    {
                        Guid = "chess",
                        Title = "Chess",
                        Description = "This was an consultant assignment for Fatshark’s largest title and the game has sold more than 1 million copies.",
                        Ownership = GameOwnership.Employee,
                        OwnershipUntil = string.Empty,
                        Price = new Price { Amount = 10, Currency = Currency.Usd },
                        Branches = chessBranches
                    }
                }
            };
        }

        public static List<GameFile> GetFilesBetaMock()
        {
            var files = new List<GameFile>()
            {
                new GameFile
                {
                    CompressedSize = 7248431,
                    Md5 = "08c1fe2b610e8b2a9577fbd843452887",
                    RelativeCompressedPath = "chess.exe.gz",
                    RelativePath = "chess.exe",
                    Size =16793088
                },
                new GameFile
                {
                    CompressedSize = 35030680,
                    Md5 = "ee62753234d6daacc804876e49b898a8",
                    RelativeCompressedPath = "chess_Data/sharedassets2.assets.resS.gz",
                    RelativePath = "chess_Data/sharedassets2.assets.resS",
                    Size = 119996136
                },
                new GameFile
                {
                    CompressedSize = 395484,
                    Md5 = "6cf0589f029974e31e2608d050ab6a7f",
                    RelativeCompressedPath = "chess_Data/Managed/System.dll.gz",
                    RelativePath = "chess_Data/Managed/System.dll",
                    Size = 1069056
                },
                new GameFile
                {
                    CompressedSize = 233810,
                    Md5 = "cfe0216ba19878b6110ee38ccbacd2c5",
                    RelativeCompressedPath = "chess_Data/Resources/unity default resources.gz",
                    RelativePath = "chess_Data/Resources/unity default resources",
                    Size = 1567340
                }
            };

            return files;
        }

        public static List<GameFile> GetFilesMasterMock()
        {
            var files = new List<GameFile>()
            {
                new GameFile
                {
                    CompressedSize = 7248431,
                    Md5 = "08c1fe2b610e8b2a9577fbd843452887",
                    RelativeCompressedPath = "chess.exe.gz",
                    RelativePath = "chess.exe",
                    Size =16793088
                },
                new GameFile
                {
                    CompressedSize = 29760820,
                    Md5 = "5c098466f4d1bc6a22906341495b75fa",
                    RelativeCompressedPath = "player_win_x86.pdb.gz",
                    RelativePath = "player_win_x86.pdb",
                    Size = 126659584
                },
                new GameFile
                {
                    CompressedSize = 4861146,
                    Md5 = "059ac97805ec4a29c9d8a0a95bb92900",
                    RelativeCompressedPath = "player_win_x86_s.pdb.gz",
                    RelativePath = "player_win_x86_s.pdb",
                    Size = 18672640
                },
                new GameFile //4
                {
                    CompressedSize = 2131129,
                    Md5 = "5de3ba319eaedcf46cd859274d0d8830",
                    RelativeCompressedPath ="chess_Data/resources.assets.gz",
                    RelativePath = "chess_Data/resources.assets",
                    Size = 3921456
                },
                new GameFile //8
                {
                    CompressedSize = 84731,
                    Md5 = "ef077b66069c89661756835745a5160f",
                    RelativeCompressedPath = "chess_Data/resources.assets.resS.gz",
                    RelativePath = "chess_Data/resources.assets.resS",
                    Size = 281672
                }
            };

            return files;
        }
    }
}