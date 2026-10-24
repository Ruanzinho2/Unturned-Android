using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Steamworks {
    public struct SteamFavoriteEntry {
        public uint IP;
        public ushort ConnPort;
        public ushort QueryPort;
        public uint Flags;
        public uint LastPlayed;

        public string GetIPString() {
            return string.Format("{0}.{1}.{2}.{3}", (IP >> 24) & 0xFF, (IP >> 16) & 0xFF, (IP >> 8) & 0xFF, IP & 0xFF);
        }
    }

    /// <summary>
    /// Manages persistent server favorites and integrates bookmarks/custom servers
    /// for Unturned Android's native server queries.
    /// </summary>
    public static class SteamFavoritesManager {
        private static readonly List<SteamFavoriteEntry> favorites = new List<SteamFavoriteEntry>();
        private static readonly object lockObj = new object();
        private static bool isLoaded = false;
        private static readonly string CloudDir = "/storage/emulated/0/Android/data/com.SmartlyDressedGames.Unturned/files/Cloud";
        private static readonly string FilePath = Path.Combine(CloudDir, "Favorites.dat");

        public static void EnsureLoaded() {
            if (isLoaded) return;
            isLoaded = true;
            lock (lockObj) {
                favorites.Clear();
                try {
                    if (File.Exists(FilePath)) {
                        using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                        using (var br = new BinaryReader(fs)) {
                            byte ver = br.ReadByte();
                            int count = br.ReadInt32();
                            for (int i = 0; i < count; i++) {
                                uint ip = br.ReadUInt32();
                                ushort cp = br.ReadUInt16();
                                ushort qp = br.ReadUInt16();
                                uint fl = br.ReadUInt32();
                                uint lp = br.ReadUInt32();
                                favorites.Add(new SteamFavoriteEntry {
                                    IP = ip,
                                    ConnPort = cp,
                                    QueryPort = qp,
                                    Flags = fl,
                                    LastPlayed = lp
                                });
                            }
                        }
                        SteamShimLogger.Info($"[SteamFavorites] Loaded {favorites.Count} favorites from {FilePath}");
                    }
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamFavorites] Error loading favorites: {ex.Message}");
                }
            }
        }

        public static void Save() {
            lock (lockObj) {
                try {
                    Directory.CreateDirectory(CloudDir);
                    using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                    using (var bw = new BinaryWriter(fs)) {
                        bw.Write((byte)1); // Version
                        bw.Write(favorites.Count);
                        foreach (var fav in favorites) {
                            bw.Write(fav.IP);
                            bw.Write(fav.ConnPort);
                            bw.Write(fav.QueryPort);
                            bw.Write(fav.Flags);
                            bw.Write(fav.LastPlayed);
                        }
                    }
                    SteamShimLogger.Info($"[SteamFavorites] Saved {favorites.Count} favorites to {FilePath}");
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamFavorites] Error saving favorites: {ex.Message}");
                }
            }
        }

        public static int GetCount() {
            EnsureLoaded();
            lock (lockObj) {
                return favorites.Count;
            }
        }

        public static bool GetFavoriteGame(int iGame, out AppId_t pnAppID, out uint pnIP, out ushort pnConnPort, out ushort pnQueryPort, out uint punFlags, out uint pRTime32LastPlayedOnServer) {
            EnsureLoaded();
            lock (lockObj) {
                if (iGame >= 0 && iGame < favorites.Count) {
                    var entry = favorites[iGame];
                    pnAppID = new AppId_t(304930);
                    pnIP = entry.IP;
                    pnConnPort = entry.ConnPort;
                    pnQueryPort = entry.QueryPort;
                    punFlags = entry.Flags;
                    pRTime32LastPlayedOnServer = entry.LastPlayed;
                    return true;
                }
            }
            pnAppID = default;
            pnIP = default;
            pnConnPort = default;
            pnQueryPort = default;
            punFlags = default;
            pRTime32LastPlayedOnServer = default;
            return false;
        }

        public static int AddFavoriteGame(AppId_t nAppID, uint nIP, ushort nConnPort, ushort nQueryPort, uint unFlags, uint rTime32LastPlayedOnServer) {
            EnsureLoaded();
            lock (lockObj) {
                int existingIdx = favorites.FindIndex(f => f.IP == nIP && f.ConnPort == nConnPort);
                if (existingIdx >= 0) {
                    favorites[existingIdx] = new SteamFavoriteEntry {
                        IP = nIP,
                        ConnPort = nConnPort,
                        QueryPort = nQueryPort,
                        Flags = unFlags,
                        LastPlayed = rTime32LastPlayedOnServer
                    };
                } else {
                    favorites.Add(new SteamFavoriteEntry {
                        IP = nIP,
                        ConnPort = nConnPort,
                        QueryPort = nQueryPort,
                        Flags = unFlags,
                        LastPlayed = rTime32LastPlayedOnServer
                    });
                }
                Save();
                SteamShimLogger.Info($"[SteamFavorites] Added favorite server: {string.Format("{0}.{1}.{2}.{3}", (nIP >> 24) & 0xFF, (nIP >> 16) & 0xFF, (nIP >> 8) & 0xFF, nIP & 0xFF)}:{nConnPort} (Query: {nQueryPort})");
                return favorites.Count;
            }
        }

        public static bool RemoveFavoriteGame(AppId_t nAppID, uint nIP, ushort nConnPort, ushort nQueryPort, uint unFlags) {
            EnsureLoaded();
            lock (lockObj) {
                int removed = favorites.RemoveAll(f => f.IP == nIP && f.ConnPort == nConnPort);
                if (removed > 0) {
                    Save();
                    SteamShimLogger.Info($"[SteamFavorites] Removed favorite server: {string.Format("{0}.{1}.{2}.{3}", (nIP >> 24) & 0xFF, (nIP >> 16) & 0xFF, (nIP >> 8) & 0xFF, nIP & 0xFF)}:{nConnPort}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves all endpoints from Favorites, CustomServers.txt, and ServerBookmarks.bin.
        /// </summary>
        public static List<(IPAddress ip, ushort queryPort, ushort connPort)> GetAllFavoriteEndpoints() {
            EnsureLoaded();
            var result = new List<(IPAddress, ushort, ushort)>();
            var seen = new HashSet<string>();

            // 1. Process persisted favorites
            lock (lockObj) {
                foreach (var fav in favorites) {
                    string ipStr = fav.GetIPString();
                    if (IPAddress.TryParse(ipStr, out var parsed)) {
                        string key = $"{parsed}:{fav.QueryPort}";
                        if (seen.Add(key)) {
                            result.Add((parsed, fav.QueryPort, fav.ConnPort));
                        }
                    }
                }
            }

            // 2. Process CustomServers.txt
            try {
                string[] paths = new string[] {
                    Path.Combine(CloudDir, "CustomServers.txt"),
                    "/storage/emulated/0/Android/data/com.SmartlyDressedGames.Unturned/files/CustomServers.txt"
                };
                foreach (var path in paths) {
                    if (File.Exists(path)) {
                        foreach (var line in File.ReadAllLines(path)) {
                            string trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                            string[] parts = trimmed.Split(':');
                            if (parts.Length >= 2 && IPAddress.TryParse(parts[0], out var ip) && ushort.TryParse(parts[1], out var port)) {
                                ushort qPort = (parts.Length >= 3 && ushort.TryParse(parts[2], out var qp)) ? qp : (ushort)(port > 0 ? port - 1 : port);
                                string key = $"{ip}:{qPort}";
                                if (seen.Add(key)) {
                                    result.Add((ip, qPort, port));
                                }
                            }
                        }
                    }
                }
            } catch { }

            // 3. Process ServerBookmarks.bin if present
            try {
                string bookmarksPath = Path.Combine(CloudDir, "ServerBookmarks.bin");
                if (File.Exists(bookmarksPath)) {
                    using (var fs = new FileStream(bookmarksPath, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs)) {
                        byte ver = br.ReadByte();
                        int count = br.ReadInt32();
                        for (int i = 0; i < count; i++) {
                            ulong sId = br.ReadUInt64();
                            string host = ReadRiverString(br);
                            ushort qPort = br.ReadUInt16();
                            string name = ReadRiverString(br);
                            string desc = ReadRiverString(br);
                            string thumb = ReadRiverString(br);

                            if (!string.IsNullOrEmpty(host) && IPAddress.TryParse(host, out var parsedIp)) {
                                ushort cPort = (ushort)(qPort + 1);
                                string key = $"{parsedIp}:{qPort}";
                                if (seen.Add(key)) {
                                    result.Add((parsedIp, qPort, cPort));
                                    SteamShimLogger.Info($"[SteamFavorites] Bookmark discovered: {name} ({parsedIp}:{cPort})");
                                }
                            }
                        }
                    }
                }
            } catch { }

            return result;
        }

        private static string ReadRiverString(BinaryReader reader) {
            if (reader.BaseStream.Position >= reader.BaseStream.Length) return string.Empty;
            byte len = reader.ReadByte();
            if (len == 0) return string.Empty;
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
