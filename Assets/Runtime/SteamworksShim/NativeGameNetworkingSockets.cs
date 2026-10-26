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
    internal static class NativeGameNetworkingSockets {
        private const string LibName = "GameNetworkingSockets";
        private static bool isInitialized = false;
        private static bool isAvailable = false;
        private static IntPtr interfacePtr = IntPtr.Zero;
        private static IntPtr interfaceUtilsPtr = IntPtr.Zero;

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
        private static extern IntPtr SteamAPI_SteamNetworkingSockets_v009();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SteamAPI_SteamNetworkingUtils_v003();

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

                                // Allow unauthenticated fallback while attempting authentication (1 = Attempt auth, fallback if not)
                                bool gCfg = SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 1);
                                SteamShimLogger.Info($"[SteamNetworkingSockets] SetGlobalConfigValueInt32 IP_AllowWithoutAuth=1 result: {gCfg}, PacketTraceMaxBytes=512, log levels set to Everything (8)");
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
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress initiating connection to {ipStr}...");

                    var optList = pOptions != null ? new System.Collections.Generic.List<SteamNetworkingConfigValue_t>(pOptions) : new System.Collections.Generic.List<SteamNetworkingConfigValue_t>();
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1 }
                        });
                    }
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_PacketTraceMaxBytes)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_PacketTraceMaxBytes,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 512 }
                        });
                    }
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_PacketDecode)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_PacketDecode,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 8 }
                        });
                    }
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_Message)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_Message,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 8 }
                        });
                    }
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_AckRTT)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_AckRTT,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 8 }
                        });
                    }
                    if (!optList.Exists(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_PacketGaps)) {
                        optList.Add(new SteamNetworkingConfigValue_t {
                            m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_LogLevel_PacketGaps,
                            m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 8 }
                        });
                    }
                    var actualOpts = optList.ToArray();

                    HSteamNetConnection conn = SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(interfacePtr, ref address, actualOpts.Length, actualOpts);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress returned connection handle: {conn.m_HSteamNetConnection}");
                    if (conn != HSteamNetConnection.Invalid && interfaceUtilsPtr != IntPtr.Zero) {
                        try {
                            bool cCfg = SteamAPI_ISteamNetworkingUtils_SetConnectionConfigValueInt32(interfaceUtilsPtr, conn.m_HSteamNetConnection, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 1);
                            SteamShimLogger.Info($"[SteamNetworkingSockets] SetConnectionConfigValueInt32 on conn {conn.m_HSteamNetConnection} result: {cCfg}");
                        } catch (Exception cEx) {
                            SteamShimLogger.Warn($"[SteamNetworkingSockets] Connection config note: {cEx.Message}");
                        }
                    }
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
