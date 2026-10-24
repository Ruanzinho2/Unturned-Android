using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks {
    /// <summary>
    /// Native C# UDP implementation of the Valve A2S_INFO query protocol.
    /// Provides live server querying for Direct Connect and Server List without native binaries.
    /// </summary>
    public static class A2SPingQuery {
        private static int nextQueryId = 1;
        private static readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

        public static void RunCallbacks() {
            while (mainThreadActions.TryDequeue(out var action)) {
                try {
                    action?.Invoke();
                } catch {
                    // Suppress exceptions in callback dispatch
                }
            }
        }

        public static HServerQuery PingServer(uint unIP, ushort usPort, ISteamMatchmakingPingResponse response) {
            int queryId = nextQueryId++;
            HServerQuery handle = new HServerQuery(queryId);

            Task.Run(async () => {
                try {
                    // unIP is in big-endian (e.g. 192.168.1.10)
                    byte[] ipBytes = new byte[] {
                        (byte)((unIP >> 24) & 0xFF),
                        (byte)((unIP >> 16) & 0xFF),
                        (byte)((unIP >> 8) & 0xFF),
                        (byte)(unIP & 0xFF)
                    };
                    IPAddress ip = new IPAddress(ipBytes);
                    IPEndPoint endPoint = new IPEndPoint(ip, usPort);

                    using (UdpClient client = new UdpClient()) {
                        client.Connect(endPoint);

                        // A2S_INFO payload: 0xFF 0xFF 0xFF 0xFF 'T' "Source Engine Query\0"
                        byte[] request = new byte[] {
                            0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75,
                            0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69,
                            0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
                        };

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        await client.SendAsync(request, request.Length);

                        var receiveResult = await ReceiveWithTimeout(client, 3000);
                        if (!receiveResult.HasValue || receiveResult.Value.Buffer.Length < 5) {
                            Fail(response);
                            return;
                        }

                        byte[] data = receiveResult.Value.Buffer;

                        // Check for challenge response: 0xFF 0xFF 0xFF 0xFF 0x41 [4 bytes challenge]
                        if (data.Length >= 9 && data[0] == 0xFF && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF && data[4] == 0x41) {
                            byte[] challengeRequest = new byte[29];
                            Array.Copy(request, challengeRequest, 25);
                            Array.Copy(data, 5, challengeRequest, 25, 4);

                            watch.Restart();
                            await client.SendAsync(challengeRequest, challengeRequest.Length);
                            receiveResult = await ReceiveWithTimeout(client, 3000);
                            if (!receiveResult.HasValue || receiveResult.Value.Buffer.Length < 5) {
                                Fail(response);
                                return;
                            }
                            data = receiveResult.Value.Buffer;
                        }

                        watch.Stop();
                        int pingMs = (int)watch.ElapsedMilliseconds;

                        // Check for A2S_INFO response: 0xFF 0xFF 0xFF 0xFF 0x49 ('I')
                        if (data.Length > 5 && data[0] == 0xFF && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF && data[4] == 0x49) {
                            gameserveritem_t server = ParseA2SInfo(data, 5, unIP, usPort, pingMs);
                            mainThreadActions.Enqueue(() => {
                                response?.m_ServerResponded?.Invoke(server);
                            });
                        } else {
                            Fail(response);
                        }
                    }
                } catch {
                    Fail(response);
                }
            });

            return handle;
        }

        private static async Task<UdpReceiveResult?> ReceiveWithTimeout(UdpClient client, int timeoutMs) {
            var receiveTask = client.ReceiveAsync();
            var delayTask = Task.Delay(timeoutMs);
            if (await Task.WhenAny(receiveTask, delayTask) == receiveTask) {
                return await receiveTask;
            }
            return null;
        }

        private static void Fail(ISteamMatchmakingPingResponse response) {
            mainThreadActions.Enqueue(() => {
                response?.m_ServerFailedToRespond?.Invoke();
            });
        }

        private static string ReadNullTerminatedString(BinaryReader reader) {
            var sb = new StringBuilder();
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                byte b = reader.ReadByte();
                if (b == 0) break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        private static gameserveritem_t ParseA2SInfo(byte[] data, int offset, uint unIP, ushort usQueryPort, int pingMs) {
            using (MemoryStream ms = new MemoryStream(data)) {
                ms.Seek(offset, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8)) {
                    byte protocol = reader.ReadByte();
                    string serverName = ReadNullTerminatedString(reader);
                    string map = ReadNullTerminatedString(reader);
                    string folder = ReadNullTerminatedString(reader);
                    string game = ReadNullTerminatedString(reader);
                    short appId = reader.ReadInt16();
                    byte players = reader.ReadByte();
                    byte maxPlayers = reader.ReadByte();
                    byte bots = reader.ReadByte();
                    byte serverType = reader.ReadByte();
                    byte environment = reader.ReadByte();
                    byte visibility = reader.ReadByte();
                    byte vac = reader.ReadByte();
                    string version = ReadNullTerminatedString(reader);

                    ushort connectionPort = (ushort)(usQueryPort + 1);
                    ulong steamId = 0;
                    string tags = "";

                    if (ms.Position < ms.Length) {
                        byte edf = reader.ReadByte();
                        if ((edf & 0x80) != 0 && ms.Position + 2 <= ms.Length) {
                            connectionPort = reader.ReadUInt16();
                        }
                        if ((edf & 0x10) != 0 && ms.Position + 8 <= ms.Length) {
                            steamId = reader.ReadUInt64();
                        }
                        if ((edf & 0x40) != 0 && ms.Position + 2 <= ms.Length) {
                            reader.ReadInt16(); // tvPort
                            ReadNullTerminatedString(reader); // tvName
                        }
                        if ((edf & 0x20) != 0) {
                            tags = ReadNullTerminatedString(reader);
                        }
                    }

                    gameserveritem_t item = new gameserveritem_t();
                    item.m_NetAdr.Init(unIP, usQueryPort, connectionPort);
                    item.SetServerName(serverName);
                    item.SetMap(map);
                    item.SetGameDir(folder);
                    item.SetGameDescription(game);
                    item.m_nAppID = (uint)appId;
                    item.m_nPlayers = players;
                    item.m_nMaxPlayers = maxPlayers;
                    item.m_nBotPlayers = bots;
                    item.m_bPassword = visibility != 0;
                    item.m_bSecure = vac != 0;
                    item.m_nPing = pingMs;
                    item.m_bHadSuccessfulResponse = true;
                    item.m_steamID = new CSteamID(steamId);
                    item.SetGameTags(tags ?? "");
                    return item;
                }
            }
        }
    }
}
