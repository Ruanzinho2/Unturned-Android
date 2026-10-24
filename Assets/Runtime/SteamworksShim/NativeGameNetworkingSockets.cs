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
                if (GameNetworkingSockets_Init(IntPtr.Zero, err)) {
                    interfacePtr = SteamAPI_SteamNetworkingSockets_v009();
                    interfaceUtilsPtr = SteamAPI_SteamNetworkingUtils_v003();
                    isAvailable = (interfacePtr != IntPtr.Zero);

                    if (interfaceUtilsPtr != IntPtr.Zero) {
                        try {
                            debugOutputCallbackDelegate = OnNativeDebugOutput;
                            IntPtr fnDbg = Marshal.GetFunctionPointerForDelegate(debugOutputCallbackDelegate);
                            SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(interfaceUtilsPtr, 4 /* k_ESteamNetworkingSocketsDebugOutputType_Msg */, fnDbg);
                            SteamShimLogger.Info("[SteamNetworkingSockets] Native debug output function registered.");
                        } catch (Exception dbgEx) {
                            SteamShimLogger.Warn($"[SteamNetworkingSockets] Debug output registration note: {dbgEx.Message}");
                        }

                        try {
                            bool gCfg = SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 2);
                            SteamShimLogger.Info($"[SteamNetworkingSockets] SetGlobalConfigValueInt32 IP_AllowWithoutAuth=2 result: {gCfg}");
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
                    SteamShimLogger.Info($"[SteamNetworkingSockets] Initialized successfully. interfacePtr={interfacePtr} utilsPtr={interfaceUtilsPtr}");
                } else {
                    SteamShimLogger.Error($"[SteamNetworkingSockets] GameNetworkingSockets_Init failed: {err}");
                    isAvailable = false;
                }
            } catch (Exception ex) {
                SteamShimLogger.Error($"[SteamNetworkingSockets] EnsureInitialized exception: {ex}");
                isAvailable = false;
            }
            return isAvailable;
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
                            m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 2 }
                        });
                    }
                    var actualOpts = optList.ToArray();

                    HSteamNetConnection conn = SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(interfacePtr, ref address, actualOpts.Length, actualOpts);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress returned connection handle: {conn.m_HSteamNetConnection}");
                    if (conn != HSteamNetConnection.Invalid && interfaceUtilsPtr != IntPtr.Zero) {
                        try {
                            bool cCfg = SteamAPI_ISteamNetworkingUtils_SetConnectionConfigValueInt32(interfaceUtilsPtr, conn.m_HSteamNetConnection, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 2);
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

        public static void RunCallbacks() {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    SteamAPI_ISteamNetworkingSockets_RunCallbacks(interfacePtr);
                } catch { }
            }
        }
    }
}
