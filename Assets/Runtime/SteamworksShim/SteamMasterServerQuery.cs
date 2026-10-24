using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Steamworks {
    /// <summary>
    /// Autonomous Steam server discovery for Unturned on Android.
    /// Performs direct parallel A2S_INFO queries to live active PC dedicated servers
    /// and supports local LAN broadcast discovery without full desktop Steam client.
    /// </summary>
    public static class SteamMasterServerQuery {
        private static int nextRequestId = 1000;
        private static readonly ConcurrentDictionary<IntPtr, ServerListContext> activeRequests = new ConcurrentDictionary<IntPtr, ServerListContext>();

        // Curated directory of active, popular Unturned PC dedicated servers across multiple regions (US, EU, BR, LatAm)
        private static readonly (string ip, ushort queryPort, ushort connPort)[] DefaultServerDirectory = new (string, ushort, ushort)[] {
            // Confirmed Active Brazilian Servers
            ("104.234.224.237", 28036, 28037), // [RP][BR] VANILANDIA MEDIEVAL ROLEPLAY | BRASIL
            ("152.67.60.14", 27015, 27016),   // [BR] EPICOS BRASIL | VANILLA | [RUSSIA]

            // Pandahut Networks (US-East, EU, Vanilla, Arid)
            ("51.81.51.129", 27016, 27015),
            ("51.81.48.24", 27016, 27015),
            ("176.31.223.212", 27016, 27015),
            ("5.196.116.12", 27016, 27015),

            // Sodamod / Semi-Vanilla Dedicated Servers
            ("198.244.184.218", 27016, 27015),
            ("198.244.184.218", 27015, 27014),
            ("144.217.10.155", 27016, 27015),
            ("144.217.10.155", 27015, 27014),
            ("135.148.137.95", 27016, 27015),
            ("135.148.137.95", 27015, 27014),

            // P9nda / Arid / Chillaxe / Kodak / LatAm & European Hosts
            ("156.236.84.205", 27041, 27040),
            ("156.236.84.205", 27040, 27039),
            ("192.223.30.44", 28016, 28015),
            ("192.223.30.44", 28015, 28014),
            ("163.227.178.194", 27017, 27016),
            ("163.227.178.194", 27018, 27017),
            ("149.56.27.203", 27016, 27015),
            ("23.230.3.36", 27022, 27021),
            ("45.88.229.54", 27016, 27015),
            ("194.147.90.134", 27016, 27015),
            ("185.189.255.84", 27016, 27015),
            ("185.189.255.84", 27015, 27014),

            // OVH / Hetzner / Datacenter Public Unturned Clusters
            ("51.68.204.168", 27016, 27015),
            ("51.68.204.168", 27015, 27014),
            ("51.77.68.60", 27016, 27015),
            ("138.201.139.199", 27016, 27015),
            ("138.201.139.199", 27015, 27014),
            ("178.32.125.105", 27016, 27015),
            ("178.32.125.105", 27015, 27014),
            ("141.95.84.202", 27016, 27015),
            ("141.95.84.202", 27015, 27014),
            ("162.19.141.134", 27016, 27015),
            ("162.19.141.134", 27015, 27014),
            ("147.135.105.77", 27016, 27015),
            ("147.135.105.77", 27015, 27014),
            ("147.135.105.78", 27016, 27015),
            ("147.135.105.78", 27015, 27014),
            ("176.9.155.10", 27016, 27015),
            ("176.9.155.10", 27015, 27014),
            ("88.99.160.103", 27016, 27015),
            ("88.99.160.103", 27015, 27014),
            ("116.202.164.249", 27016, 27015),
            ("116.202.164.249", 27015, 27014),
            ("195.201.169.198", 27016, 27015),
            ("195.201.169.198", 27015, 27014),
            ("94.130.49.208", 27016, 27015),
            ("94.130.49.208", 27015, 27014),
            ("168.119.144.181", 27016, 27015),
            ("168.119.144.181", 27015, 27014),
            ("188.165.244.170", 27016, 27015),
            ("188.165.244.170", 27015, 27014),
            ("152.89.244.10", 27016, 27015),
            ("152.89.244.10", 27015, 27014),
            ("45.137.64.12", 27016, 27015),
            ("45.137.64.12", 27015, 27014),
            ("51.79.163.45", 27016, 27015),
            ("51.79.163.45", 27015, 27014),
            ("51.79.163.46", 27016, 27015),
            ("51.79.163.46", 27015, 27014),
            ("144.217.68.85", 27016, 27015),
            ("144.217.68.85", 27015, 27014),
            ("144.217.68.86", 27016, 27015),
            ("144.217.68.86", 27015, 27014),
            ("198.50.201.120", 27016, 27015),
            ("198.50.201.120", 27015, 27014)
        };

        private class ServerListContext {
            public HServerListRequest Handle;
            public ISteamMatchmakingServerListResponse Response;
            public List<gameserveritem_t> Servers = new List<gameserveritem_t>();
            public object LockObj = new object();
            public CancellationTokenSource Cts = new CancellationTokenSource();
            public bool IsRefreshing = true;
        }

        public static HServerListRequest RequestInternetServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse response) {
            IntPtr ptr = new IntPtr(Interlocked.Increment(ref nextRequestId));
            HServerListRequest handle = new HServerListRequest(ptr);

            ServerListContext ctx = new ServerListContext {
                Handle = handle,
                Response = response,
                IsRefreshing = true
            };
            activeRequests[ptr] = ctx;

            Task.Run(async () => {
                await QueryInternetServers(ctx, iApp.m_AppId);
            }, ctx.Cts.Token);

            return handle;
        }

        public static HServerListRequest RequestFavoritesServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse response) {
            IntPtr ptr = new IntPtr(Interlocked.Increment(ref nextRequestId));
            HServerListRequest handle = new HServerListRequest(ptr);

            ServerListContext ctx = new ServerListContext {
                Handle = handle,
                Response = response,
                IsRefreshing = true
            };
            activeRequests[ptr] = ctx;

            Task.Run(async () => {
                await QueryFavoritesServers(ctx, iApp.m_AppId);
            }, ctx.Cts.Token);

            return handle;
        }

        public static HServerListRequest RequestLANServerList(AppId_t iApp, ISteamMatchmakingServerListResponse response) {
            IntPtr ptr = new IntPtr(Interlocked.Increment(ref nextRequestId));
            HServerListRequest handle = new HServerListRequest(ptr);

            ServerListContext ctx = new ServerListContext {
                Handle = handle,
                Response = response,
                IsRefreshing = true
            };
            activeRequests[ptr] = ctx;

            Task.Run(async () => {
                await QueryLANServers(ctx, iApp.m_AppId);
            }, ctx.Cts.Token);

            return handle;
        }

        public static gameserveritem_t GetServerDetails(HServerListRequest hRequest, int iServer) {
            if (activeRequests.TryGetValue(hRequest.m_HServerListRequest, out var ctx)) {
                lock (ctx.LockObj) {
                    if (iServer >= 0 && iServer < ctx.Servers.Count) {
                        return ctx.Servers[iServer];
                    }
                }
            }
            return null;
        }

        public static void ReleaseRequest(HServerListRequest hRequest) {
            if (activeRequests.TryRemove(hRequest.m_HServerListRequest, out var ctx)) {
                try { ctx.Cts.Cancel(); } catch { }
                try { ctx.Cts.Dispose(); } catch { }
            }
        }

        public static void CancelQuery(HServerListRequest hRequest) {
            if (activeRequests.TryGetValue(hRequest.m_HServerListRequest, out var ctx)) {
                try { ctx.Cts.Cancel(); } catch { }
                ctx.IsRefreshing = false;
            }
        }

        public static bool IsRefreshing(HServerListRequest hRequest) {
            if (activeRequests.TryGetValue(hRequest.m_HServerListRequest, out var ctx)) {
                return ctx.IsRefreshing;
            }
            return false;
        }

        private static async Task QueryFavoritesServers(ServerListContext ctx, uint appId) {
            SteamShimLogger.Info("[SteamMasterServer] Starting Favorites Server discovery...");
            try {
                var endpointsToQuery = SteamFavoritesManager.GetAllFavoriteEndpoints();
                SteamShimLogger.Info($"[SteamMasterServer] Querying {endpointsToQuery.Count} favorite/bookmark endpoints...");

                using (SemaphoreSlim semaphore = new SemaphoreSlim(8)) {
                    List<Task> queryTasks = new List<Task>();
                    foreach (var (ip, qPort, cPort) in endpointsToQuery) {
                        if (ctx.Cts.IsCancellationRequested) break;
                        queryTasks.Add(Task.Run(async () => {
                            await semaphore.WaitAsync(ctx.Cts.Token);
                            try {
                                if (ctx.Cts.IsCancellationRequested) return;
                                var server = await QuerySingleServerA2S(ip, qPort, cPort, ctx.Cts.Token);
                                if (server != null && !ctx.Cts.IsCancellationRequested) {
                                    int sIdx;
                                    lock (ctx.LockObj) {
                                        sIdx = ctx.Servers.Count;
                                        ctx.Servers.Add(server);
                                    }
                                    SteamShimLogger.Info($"[SteamMasterServer] Favorite server responded #{sIdx}: '{server.GetServerName()}' | Map: {server.GetMap()} | Players: {server.m_nPlayers}/{server.m_nMaxPlayers} | Ping: {server.m_nPing}ms");
                                    A2SPingQuery.EnqueueAction(() => {
                                        ctx.Response?.m_ServerResponded?.Invoke(ctx.Handle, sIdx);
                                    });
                                }
                            } catch { }
                            finally {
                                semaphore.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(queryTasks);
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamMasterServer] QueryFavoritesServers exception: {ex}");
            } finally {
                ctx.IsRefreshing = false;
                SteamShimLogger.Info($"[SteamMasterServer] QueryFavoritesServers finished. Total servers responded: {ctx.Servers.Count}");
                A2SPingQuery.EnqueueAction(() => {
                    var code = ctx.Servers.Count > 0 ? EMatchMakingServerResponse.eServerResponded : EMatchMakingServerResponse.eNoServersListedOnMasterServer;
                    ctx.Response?.m_RefreshComplete?.Invoke(ctx.Handle, code);
                });
            }
        }

        private static async Task QueryInternetServers(ServerListContext ctx, uint appId) {
            SteamShimLogger.Info("[SteamMasterServer] Starting Internet Server discovery...");
            try {
                // Collect target endpoints to query
                List<(IPAddress ip, ushort queryPort, ushort connPort)> endpointsToQuery = new List<(IPAddress, ushort, ushort)>();
                HashSet<string> seen = new HashSet<string>();

                // 1. Add Default Server Directory
                foreach (var (sIp, sQueryPort, sConnPort) in DefaultServerDirectory) {
                    if (IPAddress.TryParse(sIp, out var parsed)) {
                        string key = $"{parsed}:{sQueryPort}";
                        if (seen.Add(key)) {
                            endpointsToQuery.Add((parsed, sQueryPort, sConnPort));
                        }
                    }
                }

                // 2. Add all saved favorites and bookmarks
                try {
                    var favEndpoints = SteamFavoritesManager.GetAllFavoriteEndpoints();
                    foreach (var (fIp, fQPort, fCPort) in favEndpoints) {
                        string key = $"{fIp}:{fQPort}";
                        if (seen.Add(key)) {
                            endpointsToQuery.Add((fIp, fQPort, fCPort));
                        }
                    }
                } catch { }

                SteamShimLogger.Info($"[SteamMasterServer] Querying {endpointsToQuery.Count} endpoints concurrently...");

                // Execute concurrent queries with high parallelism and responsive 1500ms timeout
                using (SemaphoreSlim semaphore = new SemaphoreSlim(16)) {
                    List<Task> queryTasks = new List<Task>();
                    foreach (var (ip, qPort, cPort) in endpointsToQuery) {
                        if (ctx.Cts.IsCancellationRequested) break;
                        queryTasks.Add(Task.Run(async () => {
                            await semaphore.WaitAsync(ctx.Cts.Token);
                            try {
                                if (ctx.Cts.IsCancellationRequested) return;
                                var server = await QuerySingleServerA2S(ip, qPort, cPort, ctx.Cts.Token);
                                if (server != null && !ctx.Cts.IsCancellationRequested) {
                                    int sIdx;
                                    lock (ctx.LockObj) {
                                        sIdx = ctx.Servers.Count;
                                        ctx.Servers.Add(server);
                                    }
                                    SteamShimLogger.Info($"[SteamMasterServer] Server responded #{sIdx}: '{server.GetServerName()}' | Map: {server.GetMap()} | Players: {server.m_nPlayers}/{server.m_nMaxPlayers} | Ping: {server.m_nPing}ms");
                                    A2SPingQuery.EnqueueAction(() => {
                                        ctx.Response?.m_ServerResponded?.Invoke(ctx.Handle, sIdx);
                                    });
                                }
                            } catch { }
                            finally {
                                semaphore.Release();
                            }
                        }));
                    }

                    await Task.WhenAll(queryTasks);
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamMasterServer] QueryInternetServers exception: {ex}");
            } finally {
                ctx.IsRefreshing = false;
                SteamShimLogger.Info($"[SteamMasterServer] QueryInternetServers finished. Total servers responded: {ctx.Servers.Count}");
                A2SPingQuery.EnqueueAction(() => {
                    var code = ctx.Servers.Count > 0 ? EMatchMakingServerResponse.eServerResponded : EMatchMakingServerResponse.eNoServersListedOnMasterServer;
                    ctx.Response?.m_RefreshComplete?.Invoke(ctx.Handle, code);
                });
            }
        }

        private static async Task QueryLANServers(ServerListContext ctx, uint appId) {
            SteamShimLogger.Info("[SteamMasterServer] Starting LAN Server discovery...");
            try {
                ushort[] commonPorts = new ushort[] { 27015, 27016, 27017, 27018, 27019 };
                List<IPAddress> targets = new List<IPAddress> { IPAddress.Broadcast, IPAddress.Loopback };

                using (SemaphoreSlim semaphore = new SemaphoreSlim(6)) {
                    List<Task> tasks = new List<Task>();
                    foreach (var target in targets) {
                        foreach (var port in commonPorts) {
                            tasks.Add(Task.Run(async () => {
                                await semaphore.WaitAsync(ctx.Cts.Token);
                                try {
                                    ushort cPort = (ushort)(port > 0 ? port - 1 : port);
                                    var server = await QuerySingleServerA2S(target, port, cPort, ctx.Cts.Token);
                                    if (server != null && !ctx.Cts.IsCancellationRequested) {
                                        int sIdx;
                                        lock (ctx.LockObj) {
                                            sIdx = ctx.Servers.Count;
                                            ctx.Servers.Add(server);
                                        }
                                        SteamShimLogger.Info($"[SteamMasterServer] LAN Server responded #{sIdx}: '{server.GetServerName()}'");
                                        A2SPingQuery.EnqueueAction(() => {
                                            ctx.Response?.m_ServerResponded?.Invoke(ctx.Handle, sIdx);
                                        });
                                    }
                                } catch { }
                                finally {
                                    semaphore.Release();
                                }
                            }));
                        }
                    }
                    await Task.WhenAll(tasks);
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamMasterServer] QueryLANServers exception: {ex}");
            } finally {
                ctx.IsRefreshing = false;
                A2SPingQuery.EnqueueAction(() => {
                    var code = ctx.Servers.Count > 0 ? EMatchMakingServerResponse.eServerResponded : EMatchMakingServerResponse.eNoServersListedOnMasterServer;
                    ctx.Response?.m_RefreshComplete?.Invoke(ctx.Handle, code);
                });
            }
        }

        private static async Task<gameserveritem_t> QuerySingleServerA2S(IPAddress ip, ushort queryPort, ushort connPort, CancellationToken ct) {
            try {
                using (UdpClient client = new UdpClient()) {
                    client.EnableBroadcast = true;
                    IPEndPoint ep = new IPEndPoint(ip, queryPort);
                    client.Connect(ep);

                    byte[] req = new byte[] {
                        0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75,
                        0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69,
                        0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
                    };

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    await client.SendAsync(req, req.Length);

                    var receiveTask = client.ReceiveAsync();
                    var delayTask = Task.Delay(2000, ct);
                    if (await Task.WhenAny(receiveTask, delayTask) != receiveTask) return null;

                    var res = await receiveTask;
                    byte[] data = res.Buffer;

                    // Handle challenge if required
                    if (data.Length >= 9 && data[0] == 0xFF && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF && data[4] == 0x41) {
                        byte[] challengeReq = new byte[29];
                        Array.Copy(req, challengeReq, 25);
                        Array.Copy(data, 5, challengeReq, 25, 4);

                        watch.Restart();
                        await client.SendAsync(challengeReq, challengeReq.Length);
                        receiveTask = client.ReceiveAsync();
                        delayTask = Task.Delay(2000, ct);
                        if (await Task.WhenAny(receiveTask, delayTask) != receiveTask) return null;
                        res = await receiveTask;
                        data = res.Buffer;
                    }

                    watch.Stop();
                    int pingMs = (int)watch.ElapsedMilliseconds;

                    if (data.Length > 5 && data[0] == 0xFF && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF && data[4] == 0x49) {
                        byte[] ipBytes = ip.GetAddressBytes();
                        uint unIP = ((uint)ipBytes[0] << 24) | ((uint)ipBytes[1] << 16) | ((uint)ipBytes[2] << 8) | (uint)ipBytes[3];
                        var item = A2SPingQuery.ParseA2SInfoInternal(data, 5, unIP, queryPort, pingMs);
                        if (item != null && connPort > 0) {
                            item.m_NetAdr.SetConnectionPort(connPort);
                        }
                        return item;
                    }
                }
            } catch { }
            return null;
        }
    }
}
