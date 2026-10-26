using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AOT {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute {
        public MonoPInvokeCallbackAttribute(Type type) {}
    }
}

namespace Steamworks {
    /// <summary>
    /// P/Invoke bindings to Valve's open-source libGameNetworkingSockets.so for Android ARM64.
    /// Provides real SteamNetworkingSockets network transport on Android without full Steam client.
    /// </summary>
    public static class NativeGameNetworkingSockets {
        private const string LibName = "GameNetworkingSockets";
        private static bool isInitialized = false;
        private static bool isAvailable = false;
        private static IntPtr interfacePtr = IntPtr.Zero;
        private static IntPtr interfaceUtilsPtr = IntPtr.Zero;
        private static bool hasValveSignedCert = false;

        public delegate byte[] CustomNetworkingCertificateProviderDelegate(uint appId, byte[] publicKey);
        public static CustomNetworkingCertificateProviderDelegate CustomCertificateProvider = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FnSteamNetConnectionStatusChanged(IntPtr pInfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FnSteamNetAuthenticationStatusChanged(IntPtr pInfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FnSteamNetworkingSocketsDebugOutput(int nType, IntPtr pszMsg);

        private static FnSteamNetConnectionStatusChanged statusChangedCallbackDelegate;
        private static FnSteamNetAuthenticationStatusChanged authChangedCallbackDelegate;
        private static FnSteamNetworkingSocketsDebugOutput debugOutputCallbackDelegate;
        public static FSteamNetworkingSocketsDebugOutput UserDebugOutputCallback;

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GameNetworkingSockets_Init(IntPtr pIdentity, StringBuilder errMsg);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GameNetworkingSockets_Kill();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_SteamNetworkingIdentity_Clear(IntPtr pIdentity);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_SteamNetworkingIdentity_SetSteamID64(IntPtr pIdentity, ulong steamID);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong SteamAPI_SteamNetworkingIdentity_GetSteamID64(IntPtr pIdentity);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingSockets_GetIdentity(IntPtr self, IntPtr pIdentity);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern ESteamNetworkingAvailability SteamAPI_ISteamNetworkingSockets_InitAuthentication(IntPtr self);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern ESteamNetworkingAvailability SteamAPI_ISteamNetworkingSockets_GetAuthenticationStatus(IntPtr self, out SteamNetAuthenticationStatus_t pDetails);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingSockets_GetCertificateRequest(IntPtr self, ref int pcbBlob, IntPtr pBlob, StringBuilder errMsg);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingSockets_SetCertificate(IntPtr self, byte[] pCertificate, int cbCertificate, StringBuilder errMsg);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SteamAPI_SteamNetworkingSockets_v009();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SteamAPI_SteamNetworkingUtils_v003();

        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern int setenv(string name, string value, int overwrite);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(IntPtr self, IntPtr fnCallback);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetAuthenticationStatusChanged(IntPtr self, IntPtr fnCallback);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(IntPtr self, int eDetailLevel, IntPtr pfnFunc);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(IntPtr self, int eValue, int val);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingUtils_SetConnectionConfigValueInt32(IntPtr self, uint hConn, int eValue, int val);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingUtils_SetConfigValue(IntPtr self, int eValue, int eScopeType, IntPtr scopeObj, int eDataType, ref int pArg);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern HSteamNetConnection SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(IntPtr self, ref SteamNetworkingIPAddr address, int nOptions, SteamNetworkingConfigValue_t[] pOptions);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingSockets_CloseConnection(IntPtr self, HSteamNetConnection hPeer, int nReason, string pszDebug, bool bEnableLinger);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern EResult SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(IntPtr self, HSteamNetConnection hConn, IntPtr pData, uint cbData, int nSendFlags, out long pOutMessageNumber);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(IntPtr self, HSteamNetConnection hConn, IntPtr[] ppOutMessages, int nMaxMessages);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool SteamAPI_ISteamNetworkingSockets_GetConnectionInfo(IntPtr self, HSteamNetConnection hConn, out SteamNetConnectionInfo_t pInfo);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern EResult SteamAPI_ISteamNetworkingSockets_GetConnectionRealTimeStatus(IntPtr self, HSteamNetConnection hConn, ref SteamNetConnectionRealTimeStatus_t pStatus, int nLanes, ref SteamNetConnectionRealTimeLaneStatus_t pLanes);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SteamAPI_ISteamNetworkingSockets_GetDetailedConnectionStatus(IntPtr self, HSteamNetConnection hConn, [Out] byte[] pszBuf, int cbBuf);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_ISteamNetworkingSockets_RunCallbacks(IntPtr self);

        [AOT.MonoPInvokeCallback(typeof(FnSteamNetConnectionStatusChanged))]
        private static void OnNativeConnectionStatusChanged(IntPtr pInfo) {
            try {
                if (pInfo != IntPtr.Zero) {
                    SteamNetConnectionStatusChangedCallback_t cb = Marshal.PtrToStructure<SteamNetConnectionStatusChangedCallback_t>(pInfo);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] StatusChanged Callback: Conn={cb.m_hConn.m_HSteamNetConnection} State={cb.m_eOldState} -> {cb.m_info.m_eState} Reason={cb.m_info.m_eEndReason} (Debug: '{cb.m_info.m_szEndDebug}')");
                    A2SPingQuery.EnqueueAction(() => {
                        Callback<SteamNetConnectionStatusChangedCallback_t>.Dispatch(cb);
                    });
                }
            } catch (Exception ex) {
                SteamShimLogger.Warn($"[SteamNetworkingSockets] OnNativeConnectionStatusChanged error: {ex.Message}");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(FnSteamNetAuthenticationStatusChanged))]
        private static void OnNativeAuthenticationStatusChanged(IntPtr pInfo) {
            try {
                if (pInfo != IntPtr.Zero) {
                    SteamNetAuthenticationStatus_t cb = Marshal.PtrToStructure<SteamNetAuthenticationStatus_t>(pInfo);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] AuthChanged Callback: Avail={cb.m_eAvail} Msg='{cb.m_debugMsg}'");
                    A2SPingQuery.EnqueueAction(() => {
                        Callback<SteamNetAuthenticationStatus_t>.Dispatch(cb);
                    });
                }
            } catch (Exception ex) {
                SteamShimLogger.Warn($"[SteamNetworkingSockets] OnNativeAuthenticationStatusChanged error: {ex.Message}");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(FnSteamNetworkingSocketsDebugOutput))]
        private static void OnNativeDebugOutput(int nType, IntPtr pszMsg) {
            try {
                if (pszMsg != IntPtr.Zero) {
                    string msg = Marshal.PtrToStringAnsi(pszMsg);
                    SteamShimLogger.Info($"[GNS-DBG-{nType}] {msg}");
                    UserDebugOutputCallback?.Invoke((ESteamNetworkingSocketsDebugOutputType)nType, pszMsg);
                }
            } catch { }
        }

        public static void SetUserDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType eDetailLevel, FSteamNetworkingSocketsDebugOutput pfnFunc) {
            UserDebugOutputCallback = pfnFunc;
            if (EnsureInitialized() && interfaceUtilsPtr != IntPtr.Zero) {
                try {
                    if (debugOutputCallbackDelegate == null) {
                        debugOutputCallbackDelegate = OnNativeDebugOutput;
                    }
                    IntPtr fnDbg = Marshal.GetFunctionPointerForDelegate(debugOutputCallbackDelegate);
                    SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(interfaceUtilsPtr, (int)eDetailLevel, fnDbg);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] SetDebugOutputFunction updated to level {eDetailLevel}.");
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] SetDebugOutputFunction note: {ex.Message}");
                }
            }
        }

        public static bool EnsureInitialized() {
            if (isInitialized) return isAvailable;
            isInitialized = true;
            try {
                try {
                    setenv("SteamAppId", "304930", 1);
                    setenv("SteamGameId", "304930", 1);
                } catch { }

                StringBuilder err = new StringBuilder(1024);
                IntPtr pIdentity = Marshal.AllocHGlobal(136);
                try {
                    for (int i = 0; i < 136; i++) Marshal.WriteByte(pIdentity, i, 0);
                    ulong steamId = SteamUser.GetSteamID().m_SteamID;
                    if (steamId == 0) steamId = 76561198000000000UL;
                    SteamAPI_SteamNetworkingIdentity_SetSteamID64(pIdentity, steamId);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] GameNetworkingSockets_Init with local SteamID: {steamId}");

                    if (GameNetworkingSockets_Init(pIdentity, err)) {
                        interfacePtr = SteamAPI_SteamNetworkingSockets_v009();
                        interfaceUtilsPtr = SteamAPI_SteamNetworkingUtils_v003();
                        isAvailable = (interfacePtr != IntPtr.Zero);

                        if (interfaceUtilsPtr != IntPtr.Zero) {
                            try {
                                // IMPORTANT: libGameNetworkingSockets.so stores CSteamNetworkingUtils.m_nAppID at offset +8!
                                // Standalone GNS defaults this to 0. When 0, client self-signed certs are generated with app_ids: [ 0 ].
                                // Dedicated Unturned servers (AppID 304930) reject any ConnectRequest whose cert does not include AppID 304930!
                                Marshal.WriteInt32(interfaceUtilsPtr, 8, 304930);
                                int configuredAppId = Marshal.ReadInt32(interfaceUtilsPtr, 8);
                                SteamShimLogger.Info($"[SteamNetworkingSockets] Configured CSteamNetworkingUtils.m_nAppID = {configuredAppId} at offset +8 (for Unturned AppID 304930).");
                            } catch (Exception appIdEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] Note setting AppID in CSteamNetworkingUtils: {appIdEx.Message}");
                            }

                            try {
                                debugOutputCallbackDelegate = OnNativeDebugOutput;
                                IntPtr fnDbg = Marshal.GetFunctionPointerForDelegate(debugOutputCallbackDelegate);
                                // Set debug detail to 8 (Everything: full packet breakdown, crypto state, network transitions)
                                SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(interfaceUtilsPtr, 8 /* k_ESteamNetworkingSocketsDebugOutputType_Everything */, fnDbg);
                                SteamShimLogger.Info("[SteamNetworkingSockets] Native debug output function registered at level Everything (8).");
                            } catch (Exception dbgEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] Debug output registration note: {dbgEx.Message}");
                            }

                            try {
                                // Enable verbose packet decoding and transport logging
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 13 /* LogLevel_AckRTT */, 8);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 14 /* LogLevel_PacketDecode */, 8);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 15 /* LogLevel_Message */, 8);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 16 /* LogLevel_PacketGaps */, 8);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 17 /* LogLevel_P2PRendezvous */, 8);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 18 /* LogLevel_SDRRelayPings */, 8);

                                // Trace raw UDP packets on the wire (up to 512 bytes)
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 41 /* k_ESteamNetworkingConfig_PacketTraceMaxBytes */, 512);

                                // On Android without a desktop Steam client CA, IP_AllowWithoutAuth must be 2 (AllowLocalUnsignedCert)
                                // to use self-signed local certs with our verified SteamID, preventing "Need a cert authority!" (5002) error.
                                bool gCfg = SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 2);
                                SteamShimLogger.Info($"[SteamNetworkingSockets] SetGlobalConfigValueInt32 IP_AllowWithoutAuth=2 result: {gCfg}, PacketTraceMaxBytes=512, log levels set to Everything (8)");
                            } catch (Exception cfgEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] Global config note: {cfgEx.Message}");
                            }

                            try {
                                statusChangedCallbackDelegate = OnNativeConnectionStatusChanged;
                                IntPtr fnStatus = Marshal.GetFunctionPointerForDelegate(statusChangedCallbackDelegate);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(interfaceUtilsPtr, fnStatus);

                                authChangedCallbackDelegate = OnNativeAuthenticationStatusChanged;
                                IntPtr fnAuth = Marshal.GetFunctionPointerForDelegate(authChangedCallbackDelegate);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetAuthenticationStatusChanged(interfaceUtilsPtr, fnAuth);

                                SteamShimLogger.Info("[SteamNetworkingSockets] Native global status and auth callbacks registered.");
                            } catch (Exception cbEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] Callback registration note: {cbEx.Message}");
                            }
                        }

                        if (interfacePtr != IntPtr.Zero) {
                            try {
                                IntPtr pCheckIdent = Marshal.AllocHGlobal(136);
                                for (int i = 0; i < 136; i++) Marshal.WriteByte(pCheckIdent, i, 0);
                                if (SteamAPI_ISteamNetworkingSockets_GetIdentity(interfacePtr, pCheckIdent)) {
                                    ulong resolvedSteamId = SteamAPI_SteamNetworkingIdentity_GetSteamID64(pCheckIdent);
                                    SteamShimLogger.Info($"[SteamNetworkingSockets] GNS verified local identity SteamID: {resolvedSteamId}");
                                }
                                Marshal.FreeHGlobal(pCheckIdent);
                            } catch (Exception idEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] GetIdentity verification note: {idEx.Message}");
                            }
                        }

                        SteamShimLogger.Info($"[SteamNetworkingSockets] Initialized successfully. interfacePtr={interfacePtr} utilsPtr={interfaceUtilsPtr}");
                        if (SteamUser.BLoggedOn()) {
                            EnsureNetworkingCertificate(304930);
                        }
                    } else {
                        SteamShimLogger.Error($"[SteamNetworkingSockets] GameNetworkingSockets_Init failed: {err}");
                        isAvailable = false;
                    }
                } finally {
                    if (pIdentity != IntPtr.Zero) {
                        Marshal.FreeHGlobal(pIdentity);
                    }
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamNetworkingSockets] EnsureInitialized exception: {ex}");
                isAvailable = false;
            }
            return isAvailable;
        }

        public static void Reinitialize(ulong newSteamId) {
            try {
                if (isInitialized) {
                    SteamShimLogger.Info($"[SteamNetworkingSockets] Reinitializing GameNetworkingSockets with new SteamID: {newSteamId}...");
                    try {
                        GameNetworkingSockets_Kill();
                    } catch (Exception killEx) {
                        SteamShimLogger.Warn($"[SteamNetworkingSockets] GameNetworkingSockets_Kill note: {killEx.Message}");
                    }
                    isInitialized = false;
                    isAvailable = false;
                    hasValveSignedCert = false;
                    interfacePtr = IntPtr.Zero;
                    interfaceUtilsPtr = IntPtr.Zero;
                }
                EnsureInitialized();
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamNetworkingSockets] Reinitialize exception: {ex}");
            }
        }

        public static void EnsureLocalIdentity(ulong expectedSteamId) {
            if (expectedSteamId == 0 || expectedSteamId == 76561198000000000UL) return;
            if (!EnsureInitialized() || interfacePtr == IntPtr.Zero) return;
            try {
                IntPtr pCheckIdent = Marshal.AllocHGlobal(136);
                for (int i = 0; i < 136; i++) Marshal.WriteByte(pCheckIdent, i, 0);
                if (SteamAPI_ISteamNetworkingSockets_GetIdentity(interfacePtr, pCheckIdent)) {
                    ulong currentSteamId = SteamAPI_SteamNetworkingIdentity_GetSteamID64(pCheckIdent);
                    Marshal.FreeHGlobal(pCheckIdent);
                    if (currentSteamId != expectedSteamId) {
                        SteamShimLogger.Info($"[SteamNetworkingSockets] Local identity out of date (current: {currentSteamId}, expected: {expectedSteamId}). Reinitializing GNS...");
                        Reinitialize(expectedSteamId);
                    }
                } else {
                    Marshal.FreeHGlobal(pCheckIdent);
                }
            } catch (Exception ex) {
                SteamShimLogger.Warn($"[SteamNetworkingSockets] EnsureLocalIdentity note: {ex.Message}");
            }
        }

        public static void EnsureAppID(uint appId = 304930) {
            EnsureInitialized();
            if (interfaceUtilsPtr != IntPtr.Zero) {
                try {
                    Marshal.WriteInt32(interfaceUtilsPtr, 8, (int)appId);
                    int current = Marshal.ReadInt32(interfaceUtilsPtr, 8);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] Verified CSteamNetworkingUtils.m_nAppID = {current} (Unturned AppID {appId})");
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] EnsureAppID note: {ex.Message}");
                }
            }
        }

        public static bool EnsureNetworkingCertificate(uint appId = 304930) {
            if (hasValveSignedCert) return true;
            if (!EnsureInitialized() || interfacePtr == IntPtr.Zero) return false;

            if (!SteamUser.BLoggedOn()) {
                SteamShimLogger.Info("[SteamNetworkingSockets] JavaSteam is not logged on. Using local self-signed certificate.");
                return false;
            }

            try {
                SteamShimLogger.Info($"[SteamNetworkingSockets] Requesting Steam networking certificate for AppID {appId}...");

                // Step 1: Call GetCertificateRequest on GNS to generate local private key and get request blob
                int cbBlob = 0;
                StringBuilder errMsg = new StringBuilder(1024);
                SteamAPI_ISteamNetworkingSockets_GetCertificateRequest(interfacePtr, ref cbBlob, IntPtr.Zero, errMsg);
                if (cbBlob <= 0) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] GetCertificateRequest returned non-positive size: {cbBlob}, err: {errMsg}");
                    return false;
                }

                IntPtr pBlob = Marshal.AllocHGlobal(cbBlob);
                byte[] certReqBlob = new byte[cbBlob];
                try {
                    bool reqOk = SteamAPI_ISteamNetworkingSockets_GetCertificateRequest(interfacePtr, ref cbBlob, pBlob, errMsg);
                    if (!reqOk) {
                        SteamShimLogger.Warn($"[SteamNetworkingSockets] GetCertificateRequest failed: {errMsg}");
                        return false;
                    }
                    Marshal.Copy(pBlob, certReqBlob, 0, cbBlob);
                } finally {
                    Marshal.FreeHGlobal(pBlob);
                }

                // Step 2: Extract 32-byte Ed25519 public key from certReqBlob
                byte[] publicKey = ExtractPublicKeyFromCertRequest(certReqBlob);
                if (publicKey == null || publicKey.Length != 32) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] Could not extract 32-byte public key from GetCertificateRequest blob (extracted: {(publicKey != null ? publicKey.Length : 0)} bytes)");
                    return false;
                }

                SteamShimLogger.Info($"[SteamNetworkingSockets] Extracted Ed25519 public key (32 bytes). Contacting Steam CM via JavaSteam...");

                // Step 3: Request signed certificate from Valve via JavaSteam
                byte[] signedCertBlob = CustomCertificateProvider != null ? CustomCertificateProvider(appId, publicKey) : null;
                if (signedCertBlob == null || signedCertBlob.Length == 0) {
                    SteamShimLogger.Warn("[SteamNetworkingSockets] JavaSteam failed to obtain signed certificate from Steam CM.");
                    return false;
                }

                SteamShimLogger.Info($"[SteamNetworkingSockets] Obtained signed certificate ({signedCertBlob.Length} bytes). Installing into GNS...");

                // Step 4: Install signed certificate into GNS
                errMsg.Clear();
                bool setOk = SteamAPI_ISteamNetworkingSockets_SetCertificate(interfacePtr, signedCertBlob, signedCertBlob.Length, errMsg);
                if (setOk) {
                    hasValveSignedCert = true;
                    SteamShimLogger.Info($"[SteamNetworkingSockets] Valve-signed certificate successfully installed for AppID {appId}! (note/err: '{errMsg}')");
                    return true;
                } else {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] SteamAPI_ISteamNetworkingSockets_SetCertificate failed: {errMsg}");
                    return false;
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamNetworkingSockets] EnsureNetworkingCertificate exception: {ex}");
                return false;
            }
        }

        private static byte[] ExtractPublicKeyFromCertRequest(byte[] blob) {
            if (blob == null || blob.Length == 0) return null;
            try {
                // Protobuf format:
                // CMsgSteamDatagramCertificateRequest: field 1 (cert, wire type 2)
                // Inside cert: field 2 (key_data, wire type 2, length 32)
                int offset = 0;
                while (offset < blob.Length) {
                    int tag = ReadVarint(blob, ref offset);
                    int fieldNum = tag >> 3;
                    int wireType = tag & 7;
                    if (fieldNum == 1 && wireType == 2) {
                        int certLen = ReadVarint(blob, ref offset);
                        int certEnd = offset + certLen;
                        while (offset < certEnd && offset < blob.Length) {
                            int innerTag = ReadVarint(blob, ref offset);
                            int innerField = innerTag >> 3;
                            int innerWire = innerTag & 7;
                            if (innerField == 2 && innerWire == 2) {
                                int keyLen = ReadVarint(blob, ref offset);
                                if (keyLen == 32 && offset + keyLen <= blob.Length) {
                                    byte[] key = new byte[32];
                                    Buffer.BlockCopy(blob, offset, key, 0, 32);
                                    return key;
                                }
                            }
                            SkipProtobufField(blob, ref offset, innerWire);
                        }
                    } else {
                        SkipProtobufField(blob, ref offset, wireType);
                    }
                }
            } catch (Exception ex) {
                SteamShimLogger.Warn($"[SteamNetworkingSockets] ExtractPublicKeyFromCertRequest exception: {ex.Message}");
            }
            return null;
        }

        private static int ReadVarint(byte[] buf, ref int offset) {
            int result = 0;
            int shift = 0;
            while (offset < buf.Length) {
                byte b = buf[offset++];
                result |= (b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }

        private static void SkipProtobufField(byte[] buf, ref int offset, int wireType) {
            switch (wireType) {
                case 0: // Varint
                    while (offset < buf.Length && (buf[offset++] & 0x80) != 0) { }
                    break;
                case 1: // 64-bit
                    offset += 8;
                    break;
                case 2: // Length-delimited
                    int len = ReadVarint(buf, ref offset);
                    offset += len;
                    break;
                case 5: // 32-bit
                    offset += 4;
                    break;
                default:
                    offset = buf.Length;
                    break;
            }
        }

        public static bool GetIdentity(out SteamNetworkingIdentity pIdentity) {
            pIdentity = default;
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                IntPtr pNativeIdentity = Marshal.AllocHGlobal(136);
                try {
                    for (int i = 0; i < 136; i++) Marshal.WriteByte(pNativeIdentity, i, 0);
                    if (SteamAPI_ISteamNetworkingSockets_GetIdentity(interfacePtr, pNativeIdentity)) {
                        pIdentity = Marshal.PtrToStructure<SteamNetworkingIdentity>(pNativeIdentity);
                        return true;
                    }
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] GetIdentity exception: {ex.Message}");
                } finally {
                    Marshal.FreeHGlobal(pNativeIdentity);
                }
            }
            return false;
        }

        public static ESteamNetworkingAvailability InitAuthentication() {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_InitAuthentication(interfacePtr);
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] InitAuthentication note: {ex.Message}");
                }
            }
            return ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current;
        }

        public static ESteamNetworkingAvailability GetAuthenticationStatus(out SteamNetAuthenticationStatus_t pDetails) {
            pDetails = default;
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_GetAuthenticationStatus(interfacePtr, out pDetails);
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] GetAuthenticationStatus note: {ex.Message}");
                }
            }
            return ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current;
        }

        public static HSteamNetConnection ConnectByIPAddress(ref SteamNetworkingIPAddr address, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    if (address.m_ipv6 == null || address.m_ipv6.Length != 16) {
                        address.Clear();
                    }
                    address.ToString(out string ipStr, true);
                    EnsureNetworkingCertificate(304930);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress initiating connection to {ipStr}...");

                    var optList = pOptions != null ? new System.Collections.Generic.List<SteamNetworkingConfigValue_t>(pOptions) : new System.Collections.Generic.List<SteamNetworkingConfigValue_t>();
                    // Remove any global-only options (such as PacketTraceMaxBytes) that fail per-connection validation
                    optList.RemoveAll(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_PacketTraceMaxBytes);

                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 2 }
                        });
                    }
                    var actualOpts = optList.ToArray();

                    HSteamNetConnection conn = SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(interfacePtr, ref address, actualOpts.Length, actualOpts);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress returned connection handle: {conn.m_HSteamNetConnection}");
                    return conn;
                } catch (Exception ex) {
                    SteamShimLogger.Error($"[SteamNetworkingSockets] ConnectByIPAddress exception: {ex}");
                }
            }
            return HSteamNetConnection.Invalid;
        }

        public static bool CloseConnection(HSteamNetConnection hPeer, int nReason, string pszDebug, bool bEnableLinger) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    SteamShimLogger.Info($"[SteamNetworkingSockets] CloseConnection: Conn={hPeer.m_HSteamNetConnection} Reason={nReason} Debug='{pszDebug}'");
                    return SteamAPI_ISteamNetworkingSockets_CloseConnection(interfacePtr, hPeer, nReason, pszDebug, bEnableLinger);
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] CloseConnection exception: {ex}");
                }
            }
            return false;
        }

        public static EResult SendMessageToConnection(HSteamNetConnection hConn, IntPtr pData, uint cbData, int nSendFlags, out long pOutMessageNumber) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(interfacePtr, hConn, pData, cbData, nSendFlags, out pOutMessageNumber);
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] SendMessageToConnection exception: {ex.Message}");
                }
            }
            pOutMessageNumber = 0;
            return EResult.k_EResultFail;
        }

        public static int ReceiveMessagesOnConnection(HSteamNetConnection hConn, IntPtr[] ppOutMessages, int nMaxMessages) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(interfacePtr, hConn, ppOutMessages, nMaxMessages);
                } catch { }
            }
            return 0;
        }

        public static bool GetConnectionInfo(HSteamNetConnection hConn, out SteamNetConnectionInfo_t pInfo) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_GetConnectionInfo(interfacePtr, hConn, out pInfo);
                } catch { }
            }
            pInfo = default;
            return false;
        }

        public static EResult GetConnectionRealTimeStatus(HSteamNetConnection hConn, ref SteamNetConnectionRealTimeStatus_t pStatus, int nLanes, ref SteamNetConnectionRealTimeLaneStatus_t pLanes) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    return SteamAPI_ISteamNetworkingSockets_GetConnectionRealTimeStatus(interfacePtr, hConn, ref pStatus, nLanes, ref pLanes);
                } catch { }
            }
            return EResult.k_EResultFail;
        }

        public static int GetDetailedConnectionStatus(HSteamNetConnection hConn, out string pszBuf, int cbBuf) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    int bufferSize = (cbBuf > 0) ? cbBuf : 4096;
                    byte[] buf = new byte[bufferSize];
                    int ret = SteamAPI_ISteamNetworkingSockets_GetDetailedConnectionStatus(interfacePtr, hConn, buf, buf.Length);
                    if (ret >= 0) {
                        int nullIdx = Array.IndexOf(buf, (byte)0);
                        int len = (nullIdx >= 0) ? nullIdx : Math.Min(ret, buf.Length);
                        pszBuf = System.Text.Encoding.UTF8.GetString(buf, 0, len);
                        return ret;
                    }
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] GetDetailedConnectionStatus note: {ex.Message}");
                }
            }
            pszBuf = string.Empty;
            return -1;
        }

        public static void RunCallbacks() {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    SteamAPI_ISteamNetworkingSockets_RunCallbacks(interfacePtr);
                } catch { }
            }
        }
    }
}
