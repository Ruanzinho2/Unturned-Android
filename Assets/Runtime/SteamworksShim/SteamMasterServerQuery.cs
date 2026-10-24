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
    /// Native C# implementation of the Valve Master Server Query Protocol.
    /// Queries hl2master.steampowered.com:27011 over UDP to discover active Unturned servers
    /// and queries each server via A2S_INFO to populate the server browser on Android.
    /// </summary>
    public static class SteamMasterServerQuery {
        private static int nextRequestId = 1000;
        private static readonly ConcurrentDictionary<IntPtr, ServerListContext> activeRequests = new ConcurrentDictionary<IntPtr, ServerListContext>();

        // Known Master Server endpoints (DNS and fallback IPs)
        private static readonly (string host, int port)[] MasterServers = new[] {
            ("hl2master.steampowered.com", 27011),
            ("208.64.200.52", 27011),
            ("208.64.200.39", 27011),
            ("208.64.200.65", 27011)
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

        private static async Task QueryInternetServers(ServerListContext ctx, uint appId) {
            try {
                List<(IPAddress ip, ushort port)> discoveredEndpoints = new List<(IPAddress, ushort)>();

                // 1. Try querying Master Server for server endpoints
                foreach (var (host, port) in MasterServers) {
                    if (ctx.Cts.IsCancellationRequested) break;
                    try {
                        IPAddress[] addresses;
                        if (IPAddress.TryParse(host, out var parsedIp)) {
                            addresses = new[] { parsedIp };
                        } else {
                            addresses = await Dns.GetHostAddressesAsync(host);
                        }

                        if (addresses.Length == 0) continue;

                        IPEndPoint masterEp = new IPEndPoint(addresses[0], port);
                        using (UdpClient client = new UdpClient()) {
                            client.Connect(masterEp);

                            // Master Server Request packet: \x31 \xFF "0.0.0.0:0\0" "\appid\<id>\0"
                            byte[] reqBytes;
                            using (MemoryStream ms = new MemoryStream()) {
                                ms.WriteByte(0x31);
                                ms.WriteByte(0xFF); // Region: All
                                byte[] seed = Encoding.ASCII.GetBytes("0.0.0.0:0\0");
                                ms.Write(seed, 0, seed.Length);
                                byte[] filter = Encoding.ASCII.GetBytes($"\\appid\\{appId}\\0");
                                ms.Write(filter, 0, filter.Length);
                                reqBytes = ms.ToArray();
                            }

                            await client.SendAsync(reqBytes, reqBytes.Length);

                            var receiveTask = client.ReceiveAsync();
                            var delayTask = Task.Delay(3500, ctx.Cts.Token);
                            if (await Task.WhenAny(receiveTask, delayTask) == receiveTask) {
                                var res = await receiveTask;
                                byte[] data = res.Buffer;

                                // Header: 0xFF 0xFF 0xFF 0xFF 0x66 0x0A
                                if (data.Length >= 6 && data[0] == 0xFF && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF && data[4] == 0x66 && data[5] == 0x0A) {
                                    for (int i = 6; i + 6 <= data.Length; i += 6) {
                                        byte[] ipBytes = new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] };
                                        ushort sPort = (ushort)((data[i + 4] << 8) | data[i + 5]);
                                        if (ipBytes[0] == 0 && ipBytes[1] == 0 && ipBytes[2] == 0 && ipBytes[3] == 0 && sPort == 0) break;
                                        discoveredEndpoints.Add((new IPAddress(ipBytes), sPort));
                                    }
                                }
                            }
                        }

                        if (discoveredEndpoints.Count > 0) {
                            break; // Successfully got endpoints from this master server
                        }
                    } catch {
                        // Try next master server fallback
                    }
                }

                // Limit endpoints to query concurrently (up to 60 servers for mobile bandwidth)
                int maxServersToQuery = Math.Min(discoveredEndpoints.Count, 60);
                var endpointsToQuery = discoveredEndpoints.GetRange(0, maxServersToQuery);

                using (SemaphoreSlim semaphore = new SemaphoreSlim(12)) {
                    List<Task> queryTasks = new List<Task>();
                    foreach (var (ip, qPort) in endpointsToQuery) {
                        if (ctx.Cts.IsCancellationRequested) break;
                        queryTasks.Add(Task.Run(async () => {
                            await semaphore.WaitAsync(ctx.Cts.Token);
                            try {
                                if (ctx.Cts.IsCancellationRequested) return;
                                var server = await QuerySingleServerA2S(ip, qPort, ctx.Cts.Token);
                                if (server != null && !ctx.Cts.IsCancellationRequested) {
                                    int sIdx;
                                    lock (ctx.LockObj) {
                                        sIdx = ctx.Servers.Count;
                                        ctx.Servers.Add(server);
                                    }
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
            } catch { }
            finally {
                ctx.IsRefreshing = false;
                A2SPingQuery.EnqueueAction(() => {
                    var code = ctx.Servers.Count > 0 ? EMatchMakingServerResponse.eServerResponded : EMatchMakingServerResponse.eNoServersListedOnMasterServer;
                    ctx.Response?.m_RefreshComplete?.Invoke(ctx.Handle, code);
                });
            }
        }

        private static async Task QueryLANServers(ServerListContext ctx, uint appId) {
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
                                    var server = await QuerySingleServerA2S(target, port, ctx.Cts.Token);
                                    if (server != null && !ctx.Cts.IsCancellationRequested) {
                                        int sIdx;
                                        lock (ctx.LockObj) {
                                            sIdx = ctx.Servers.Count;
                                            ctx.Servers.Add(server);
                                        }
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
            } catch { }
            finally {
                ctx.IsRefreshing = false;
                A2SPingQuery.EnqueueAction(() => {
                    var code = ctx.Servers.Count > 0 ? EMatchMakingServerResponse.eServerResponded : EMatchMakingServerResponse.eNoServersListedOnMasterServer;
                    ctx.Response?.m_RefreshComplete?.Invoke(ctx.Handle, code);
                });
            }
        }

        private static async Task<gameserveritem_t> QuerySingleServerA2S(IPAddress ip, ushort queryPort, CancellationToken ct) {
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
                        return A2SPingQuery.ParseA2SInfoInternal(data, 5, unIP, queryPort, pingMs);
                    }
                }
            } catch { }
            return null;
        }
    }
}
