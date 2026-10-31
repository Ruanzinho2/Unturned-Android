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
        private static bool trustedRootCAsInstalled = false;

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
        private static extern bool SteamAPI_ISteamNetworkingSockets_AddTrustedRootCA(IntPtr self, string pszKey, StringBuilder errMsg);

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
        private static extern HSteamNetConnection SteamAPI_ISteamNetworkingSockets_ConnectP2P(IntPtr self, ref SteamNetworkingIdentity identityRemote, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions);

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

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_SteamNetworkingMessage_t_Release(IntPtr pMsg);

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
            // Native callback registration is intentionally omitted on Android IL2CPP because GNS invokes
            // debug spew directly from unattached C++ native pthreads (ServiceThread, UDP thread), causing SIGSEGV crashes.
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

                            // NOTE: We deliberately do NOT register SetDebugOutputFunction because GNS calls it directly
                            // from its C++ native ServiceThread and UDP worker pthreads. Calling into IL2CPP and allocating
                            // managed objects (strings, logs) from an unattached pthread causes SIGSEGV crashes on Android.
                            try {
                                SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(interfaceUtilsPtr, 0 /* None */, IntPtr.Zero);
                            } catch { }

                            try {
                                // Keep standard log levels for clean performance
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 15 /* LogLevel_Message */, 5);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 17 /* LogLevel_P2PRendezvous */, 5);
                                SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 18 /* LogLevel_SDRRelayPings */, 5);

                                // IP_AllowWithoutAuth = 0: Enforce strict cryptographic authentication. Do not allow unauthorized peers or fallback to anonymous identity.
                                bool gCfg = SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(interfaceUtilsPtr, 23 /* k_ESteamNetworkingConfig_IP_AllowWithoutAuth */, 0);
                                SteamShimLogger.Info($"[SteamNetworkingSockets] SetGlobalConfigValueInt32 IP_AllowWithoutAuth=0 result: {gCfg}, log levels set to Msg (5)");
                            } catch (Exception cfgEx) {
                                SteamShimLogger.Warn($"[SteamNetworkingSockets] Global config note: {cfgEx.Message}");
                            }

                            // NOTE: We deliberately do NOT register SetGlobalCallback_SteamNetConnectionStatusChanged
                            // because GNS calls it directly on the C++ ServiceThread. Calling into IL2CPP and marshaling
                            // complex structs with managed arrays from an unattached pthread causes SIGSEGV crashes.
                            // ClientTransport_SteamNetworkingSockets polls GetConnectionInfo on the Unity main thread in
                            // OnUpdate(), which is 100% thread-safe and deterministic.
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
                        EnsureTrustedRootCAs();
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
                    trustedRootCAsInstalled = false;
                    interfacePtr = IntPtr.Zero;
                    interfaceUtilsPtr = IntPtr.Zero;
                }
                if (newSteamId != 0 && newSteamId != 76561198000000000UL) {
                    SteamUser.ConfiguredSteamID = newSteamId;
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

        private static readonly string[] ValveRootCACertificates = new string[] {
            "Ii4IARIgSJbwDpn/07/GHiGMKio0Vh18VN3D/hKzQGh6n0Yx9qpF6fLfak3p9at9KT6tfJIvitz8MkDZrsE/NVxl0eoEuTVZ72bCyYd7GSNgStdROJkzgt5SjoGMgxjdkZBjuYTw9fbD+x+mzzA+ilPYEXk2XNjFyj8D",
            "Ii4IARIgmuygThdRzmJo1WkALKHh+hstvCbTa06joAg603KCm4RF6fLfak3p9at9KT6tfJIvitz8MkCxnMbUaj1AXHqWTAtEMMttjkG/6UIT4NDVpYhW+w0usRQFvP3wtQ1qtCodTWYBwrTTFmcMJMqtFTEoakp2lcsJ",
            "Ij4IARIgpdtY6tZuJOfB3FHhsMDlJWo6EFDmRXVNMgEkptCg+YpF6vLfak3qWaJuUJrVEFCO0iNQ+pQ+UIy8TSk+rXySL4rc/DJAbDCKfD9SOBRTcV+QuebKqHo8TtgEOgXxB84b3ILrklThrm9Mhr2qtfa+eR4VrNIpRga5byOnE8rosFMdu/v0BA==",
            "IjIIARIgCXkgLFarXpHydo8Rkn7V662Ab+ndxPq6NlzEzrN+H+tF6vLfak3qWaJuUICsESk+rXySL4rc/DJAS3bROFd9MfPg//Q7PhrhtWXLrn1wbfQQh9TBLrXxFO6kmQakIZBoOgnC98gKlS0BqxwZa6A8yRWCo0Yu30wiAQ==",
            "IjIIARIgOvk0Xp/JOKg3+dJKrWpMrnjH9rWWTi9Evq3srROsjbxF6vLfak3qWaJuUICsESk+rXySL4rc/DJAWVOMH53bGAc7Ln/Y3HU3cQ2SLG3HAFqfUBFuKO5sdL0HJLMQVYfmx0PL7DF+99h8QkfJPxbUr260x1ZhxPT+BQ==",
            "IjIIARIgQUoxHO4Sbn+7AsTGgtZ8J+dKW6pFwFhUKvU6Trq6AJFF6vLfak3qWaJuUPbHRyk+rXySL4rc/DJAB07M2CDF0kT/VR26uqGLM0C/NGO8MXV1dM7dfPyHpAtL7LILSrAGjxut0ZppfUpONNzfD0q3WM36V+5zXXTwAw==",
            "IjIIARIgZ8afoPHQ1SJkbDTSbbaz9J7t6WpG2HYOmpw1d1Q49r1F6vLfak3qWaJuUP6JcSk+rXySL4rc/DJAllt1Tq9of5ZD8hUuxddbiAGCVJLiAuxCwHx1dx4gx+8COo5F9zXNejQoZ9duYOdIOQYf2pzHFjqCDwfd2B7gBQ==",
            "IjIIARIgbQrsJLXGcVefebGt+C8PI+LV7KD9bCGsbotu1YvhgwVF6vLfak3qWaJuUPbHRyk+rXySL4rc/DJAxTe9cgreWBthFCxXAdAq/telZr0Qx+SHDto049sHQ6CkHB7/+kYyje1ukGf0rh2VFbcNJhn3v3T3suY7yICfDQ==",
            "IjIIARIgbcqFwE6OBNLPespNB9D6gAZ6T1JWxBX03eiwVJBA9hdF6vLfak3qWaJuUPbHRyk+rXySL4rc/DJAtlP8v6KXExwfs89NVWJRHRelzEdmWyW3LlUmdobdhEi74qP87G3+I4DoGBYBNoS7l3KUuSch3HRNQDbZwOykDQ==",
            "IjIIARIgmu5Nv72Qm74PVBQ58hay4v13bw1B2qvq4mTP71HW9WpF6vLfak3qWaJuUPi6WSk+rXySL4rc/DJAMBF4R6RQFlTvj34aPoNXvHTHd+sKrG0MElMV4dSJVchlIm13f8bUgv5RCVkOztSlBuHCSfPbgi5gQ2WxMIbBBg==",
            "IjIIARIgo8xjxfGYHv0LfOsSTcywSSree/xaQqB8AAdatIKnNuhF6vLfak3qWaJuUJTedSk+rXySL4rc/DJADbXadOIf+GljTveWZKPqGEbQY64lQe2z/UiUPPmsDLbuWTREmyes32VnRetLfFp+ObTW4jMV2BqrNwk2SNzABg==",
            "IjIIARIgpPJHtQVj8i8katUqP+SUJg9ii7nf00CWUrvoxrvqwa9F6vLfak3qWaJuUNiiQSk+rXySL4rc/DJAQeQePNXFvyW94R8VDz1yxnD98DxBkMTnaKlC31VDZ1LSRdqnq3zZSb4OCfUFbmFu7uOKkUM3A0gg4NNPf0YOAQ==",
            "IjIIARIgqRRbODhmNhl/jHaVnHb/9myxt84TD3GD3KjVe6pLpDRF6vLfak3qWaJuUJK0cik+rXySL4rc/DJAsUW8M16Z6nhMNnjWr1LFMeqG+y/XgF9l7vn9qNlc0JE8lgVqCIhelMP5QOTXaHyWHkeIQi46qtHvQPw/Q+niAA==",
            "IjMIARIg80iF8RBZg0jnEN6I3NiWbNXup1MvMBidYjwJVPyAPLtF6vLfak3qWaJuUM7AkQEpPq18ki+K3PwyQJVV5w2WchsAXDv2vIAwaLgX0N20ZmbrO6DKlhTrN9sRQKiqH7hS+0Q5+zGoTv/A6OWuChy9dY7cSzvZ8MFx7w8=",
            "IjMIARIgFP9yak4w+jgCDBbjGFeU8oRFI+G8OWGliDO0wVxrpwxF6vLfak3qWaJuULbRwgIpPq18ki+K3PwyQDokGp6l2thxKG47kfZVADqn2IdK9vNgIsjixHyS2bfuiYovPBvBwreni3UOMOBhaxS1CQzy1KsrDMca7iNFlQg=",
            "IjMIARIgFuS2j9q4cnG1eHFSW8Gci1kGCMzKZ/LL3O7zz5+U2FdF6vLfak3qWaJuUOb49wEpPq18ki+K3PwyQE0xn4Zm1LLggzDajlYx3BakTfX2VfW1SstdAq9Q3c94HeXgey40o/QTabZUoDvxiiiPBXzYJZZsmu9mXKlikww=",
            "IjMIARIgMDTWOBwBKK6HG/vjBBNfBSZx7ZnH69k5t/imAS7A/GZF6vLfak3qWaJuULDd6AEpPq18ki+K3PwyQL1ZQonSW84PIIsDxwojkuHKKhmp2umaZn6Evj2v1pLekXcHotsxsQrJwsjzyksGgZN7TTa7VjaxVB22zWi9AA4=",
            "IjMIARIgPy905SuobryVubUHOcWDvgbZiQvkt2q4X8a0l/pK01hF6vLfak3qWaJuUPS91gEpPq18ki+K3PwyQLrcLsCx13682tL+dDbThiAWqBETQyv+oa7GgFX2zauZOGLb94DuiDnH1VoM+9KLNmVoykI0PhqXRgQ42MWEPwo=",
            "IjMIARIgYt+q0NM81evo81wuCdv4Xr+Lvmqjg3or5pwbeKvnqVJF6vLfak3qWaJuUOKdvgEpPq18ki+K3PwyQMjWQst+IodcmUtk3Oe25xxJpJpit2M3AaaV8nh1UsSoCoCLx9Q01IwKooFt+boTuEPk7gZGMirA/C3K7YJ/SAY=",
            "IjMIARIgaByRvuB84Jr3T7QNyFT/G/0xmsndKSc6I7Ksp3wSU/JF6vLfak3qWaJuUJzaigEpPq18ki+K3PwyQCkLos0scg0UzH03A7j2AO405ENjat9gHk2UJlRI4Vzu+vbq90O/cdtvtlW3B6aFFuFAFc5JtAxZFFFF79q1dww=",
            "IjMIARIgjqKuiWU1m88lCRYjbHbpVFX1txwHUrTFFbrtNcuJFyNF6vLfak3qWaJuUKrE2AEpPq18ki+K3PwyQAshwpItfTqFVWT5iZZ9es1ycSw82sYe+Vkai4kLOF8Z+54ZOdLinrfyehoy8TSaNBWBKV2ImPppZ6d8ccxZ5Q4=",
            "IjMIARIgjvmzhZM/7rEuz0Xf4kej6l83AyVeRelJRMUt8wGo8oRF6vLfak3qWaJuUIachwEpPq18ki+K3PwyQAyvh50cNsa+3gUo0mIOF6D86ztyV0zhAjv3r4CbtwOuGamPfKXSZypeHuie4ft1/6Oqmq8ohzhmY5g6+EVHJQ4=",
            "IjMIARIgsgr6La9ki5fLbw8wZU2tdnvhqThmmRK3RgpZvhsLdW5F6vLfak3qWaJuUMiPuwEpPq18ki+K3PwyQIYKcQKK+0PxRNM9CFjk6b98mb6jG54/7y2UqE6ICj5cLN0fhHUwuRGCc5oekeLolg+edc3jzvkDQCQNfcuhDgs=",
            "IjMIARIgspqlc1x7gEdCkENc5knGJkzBzleqB65WrYi+onCYIyJF6vLfak3qWaJuUJbUgwIpPq18ki+K3PwyQLCc32Hgqf1Mw9LKRVIS5xDM67Cu9uc57VFOAaX2kilTPl0GP7kaecOzS8S1v9I+Zv04k6S45WkP3QQ8OmKE8gE=",
            "IjMIARIguWbwz66bn2wstN2G5VqZk9fvaJO3tjVlugOfXvv/rIBF6vLfak3qWaJuUMri6QEpPq18ki+K3PwyQL+nnaTsmypuROMzH+sH31hX8o9/sQVRGd4b3tptIlcdQYIlugEDvJMewqQjfvOL2EWUli6lN9J+1bDmMx8KkAE=",
            "IjYIARIg++XfexpF6c9GsYI97ywrpHyPZHvM5KeaY5k/c1NGgkpF6vLfak3qWaJuUKSZFVDcoUIpPq18ki+K3PwyQP9J7c51bvDLkp4wdeemt6jD1z4tsgMdRt9xleG/ObN8Eu1SM1ZRkiObJD0SsdYISLCFnTa8I1E/Y6jogi6Rdgs=",
            "IjYIARIgGekPLsQIN1v2tQMkBDUvDTZEWLgupOPUejPUjH2k9JtF6vLfak3qWaJuUPbHR1DsqmgpPq18ki+K3PwyQIc3ZqfzBMSNf/50itRjXLquxn7PmEM16Bg+0/ej6Dqp0TTDog9th82fmyXp8gyhJFfO3BE353BvqfAlmHq7dAo=",
            "IjYIARIgW0/njCUOhyUxNFu2OH4blGOOymFUKvVUuitYBC2WL7VF6vLfak3qWaJuUJrWE1CK+BQpPq18ki+K3PwyQFvrrmO1VYsetWPqKSSzeKvn04nCmwcGzObLBluFIQdS6Fs9lYlMXrAcREXu/e1vw6nUJgXLdxGC1fdKfbarsQM=",
            "IjYIARIgXGZn3LJaYaLSiDnoQYGkHWpnY+M0Q00fHh9ni6+NLcBF6vLfak3qWaJuUKSZFVDcoUIpPq18ki+K3PwyQM/YL9GUHOfBHWNOEzIT56qYB16PsncXJYh6QfTyHrE+1TEiArIHKVAjRrD5vH3BuepYWeQW8ZlOKbPCz3nWvgE=",
            "IjoIARIg8+9JL2vE+dNmhBPauZ/QZg7uQfZxiNCseZkk4XzGPlJF6vLfak3qWaJuUKLCPlCS8z9Qmr5DKT6tfJIvitz8MkDOWauRgdT5hw1LTqe/oQAqzgPFY02e3cSMa2h+iQtn91uU8+N09J2rYndb/2vQ8Ul4/oFCqCcXnSmxnnwoCKsP",
            "IjsIARIgeRDuFHa02m4l6xEIdUXY2jNxMKAPNn++rjUXeGxctqlF6vLfak3qWaJuUMYFUNAFUNoFUJixECk+rXySL4rc/DJADG2d0iwK8b+NBxBvhSXq1dhyOAyMNajJULrmCFHigxQzG1iL+Ofek0KMHzPff9+1dXVicaYweM0OLZIepbgoBA==",
            "IjwIARIgIlvA41Mpb2Ja+PfJVrFEYx90ECXf55F5FxI1Pk79iY5F6vLfak3qWaJuULgDUIgEUKoGUNyGiQIpPq18ki+K3PwyQBOj/C4TmzMiW5vtKRHb/DXLdcVGIPucVFbQmQUKhoI/KOzXgn39XrUSZQpRSxqvItgQffpsYC9E1MqmtfhwhAc=",
            "IjwIARIgiIPS+pCa8KK7WWSyvqzy90FXzxy2vMAucaDKr3QU72FF6vLfak3qWaJuUOj7cVDKyaUCUPrguwIpPq18ki+K3PwyQNjRF2y2WGNfREdBMOtzPEvRH3QchYkxImR4sJhswa82aFGOOQFb+ErUARhX+ZRxDbnMpiApg9I1ipg/q5DXnQM=",
            "Ik0IARIg1Dgd2R8FszAqcaMuEmzMoli/lZC8dEqopsWXmQRE8dlF6vLfak3qWaJuULoEULAGUICKD1DexwxQosI+UJLzP1CavkNQ9tmMASk+rXySL4rc/DJAydsgE0WAULK/UBhf/F+9AOJdrAbLwQz3V/d5ORFrh+Hnx6O3BtJpH/khAU4bhlYzhFCGJJ4wB0Zw6iWDR8BKDg==",
            "IkIIARIgDxeHAqWkjxGksWMGHdS8WfKQ/jdZleJrxRVDxO3J7ltF6vLfak3qWaJuUOTv5AFQhoHlAVDg5cwBUMai7QEpPq18ki+K3PwyQIl2vsBM9+vMvYolSRYNZ1ElimYNC6qT3DsnmL/P+6mOW/fS0r27SDMOGfeLVOD2FwbAo2S05HXPzWgqQOnmMgw=",
            "IkIIARIgZYahIxt98dknWHu347FPCiDbJkNbXmNCMqAJlMHuIN1F6vLfak3qWaJuUOTv5AFQhoHlAVDg5cwBUMai7QEpPq18ki+K3PwyQNCbZP0ESnC0KiFYIr9jnW9rUz1BmrujcLcol85C29H7wIWWc73/h02RnddHOBwdG6cgxs9Vk0VzOSwiRj2Heg4=",
            "IkIIARIgms3UDFAUcBufuFqb+oPt3dky5O1U+1a+D80YVMZ8MjRF6vLfak3qWaJuUOTv5AFQhoHlAVDg5cwBUMai7QEpPq18ki+K3PwyQEPQ86P1zneK8XK4eqHkcb/N83eNJ5qD6cKwaB4Fr0OCxZX94IfQvMnJAgtqAyvYfBpuKnTO8XJTCFP0GLXPQw8=",
            "IkUIARIgaMVHur3UExsppAQNmlq0lEm4VJJ2R+HAu3V2JFKxV8FF6vLfak3qWaJuULLKNFDy6FZQqN++AVDQ8tQBUNrp5gEpPq18ki+K3PwyQK2yfjgIq70u2EeEHkmtB2PTr4C65njTYiwCqOF5+ifR6qcqYT2nvcEK8Kb/pMUW8vEVQr+0Y314HpF6AF4h0As=",
            "IkoIARIgAMdpGLGVnpcF3RtvKZld2zISFKdkviQmghM9RBMrurdF6vLfak3qWaJuUOyjHFDtoxxQ7qMcULLkHlC41DVQxIsaUNrcOCk+rXySL4rc/DJAjp+0p3FX9Uzh8khTf3bl0yDoASPJEHWW6pyXuiryz+mXTuQFjRX3acWTOn7GuqLXMOrd/HGj3LSk2coHq4yBAg==",
        };

        public static void EnsureTrustedRootCAs() {
            if (trustedRootCAsInstalled) return;
            if (!EnsureInitialized() || interfacePtr == IntPtr.Zero) return;

            trustedRootCAsInstalled = true;
            int successCount = 0;
            StringBuilder errMsg = new StringBuilder(1024);

            foreach (string caCert in ValveRootCACertificates) {
                errMsg.Clear();
                try {
                    if (SteamAPI_ISteamNetworkingSockets_AddTrustedRootCA(interfacePtr, caCert, errMsg)) {
                        successCount++;
                    } else if (errMsg.Length > 0) {
                        SteamShimLogger.Warn($"[SteamNetworkingSockets] AddTrustedRootCA note: {errMsg}");
                    }
                } catch (Exception ex) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] AddTrustedRootCA exception: {ex.Message}");
                    break;
                }
            }
            SteamShimLogger.Info($"[SteamNetworkingSockets] Installed {successCount}/{ValveRootCACertificates.Length} Valve Root CAs into GNS.");
        }

        public static bool EnsureNetworkingCertificate(uint appId = 304930) {
            if (hasValveSignedCert) return true;
            if (!EnsureInitialized() || interfacePtr == IntPtr.Zero) return false;

            EnsureTrustedRootCAs();

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

                SteamShimLogger.Info($"[SteamNetworkingSockets] Obtained signed certificate ({signedCertBlob.Length} bytes): {BitConverter.ToString(signedCertBlob).Replace("-", "")}. Installing into GNS...");

                // Step 4: Install signed certificate into GNS
                errMsg.Clear();
                bool setOk = SteamAPI_ISteamNetworkingSockets_SetCertificate(interfacePtr, signedCertBlob, signedCertBlob.Length, errMsg);
                if (setOk && !errMsg.ToString().Contains("not known to us")) {
                    hasValveSignedCert = true;
                    SteamShimLogger.Info($"[SteamNetworkingSockets] Valve-signed certificate successfully verified and installed for AppID {appId}! (note/err: '{errMsg}')");
                    return true;
                } else if (setOk) {
                    SteamShimLogger.Warn($"[SteamNetworkingSockets] Valve-signed certificate installed with warning/note: '{errMsg}'.");
                    hasValveSignedCert = false;
                    return false;
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
                    EnsureNetworkingCertificate(304930);
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
                    ulong expectedId = SteamUser.GetSteamID().m_SteamID;
                    EnsureLocalIdentity(expectedId);
                    EnsureNetworkingCertificate(304930);
                    if (GetIdentity(out SteamNetworkingIdentity localId)) {
                        SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress Local Identity: type={localId.m_eType}, steamId={localId.GetSteamID64()} (expected: {expectedId})");
                    }
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectByIPAddress initiating connection to {ipStr}...");

                    var optList = pOptions != null ? new System.Collections.Generic.List<SteamNetworkingConfigValue_t>(pOptions) : new System.Collections.Generic.List<SteamNetworkingConfigValue_t>();
                    // Remove any global-only options (such as PacketTraceMaxBytes) that fail per-connection validation
                    optList.RemoveAll(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_PacketTraceMaxBytes);

                    // IP_AllowWithoutAuth = 0: Strictly require authentication; prevent fallback to anonymous SteamID which causes STEAM_ID_MISMATCH (rejection 49)
                    optList.RemoveAll(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth);
                    optList.Add(new SteamNetworkingConfigValue_t {
                        m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
                        m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                        m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 0 }
                    });
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

        public static HSteamNetConnection ConnectP2P(ref SteamNetworkingIdentity identityRemote, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            if (EnsureInitialized() && interfacePtr != IntPtr.Zero) {
                try {
                    ulong expectedId = SteamUser.GetSteamID().m_SteamID;
                    EnsureLocalIdentity(expectedId);
                    EnsureNetworkingCertificate(304930);
                    if (GetIdentity(out SteamNetworkingIdentity localId)) {
                        SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectP2P Local Identity: type={localId.m_eType}, steamId={localId.GetSteamID64()} (expected: {expectedId})");
                    }
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectP2P initiating connection to remote SteamID={identityRemote.GetSteamID64()} (vport={nRemoteVirtualPort})...");

                    var optList = pOptions != null ? new System.Collections.Generic.List<SteamNetworkingConfigValue_t>(pOptions) : new System.Collections.Generic.List<SteamNetworkingConfigValue_t>();
                    // Remove any global-only options (such as PacketTraceMaxBytes) that fail per-connection validation
                    optList.RemoveAll(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_PacketTraceMaxBytes);

                    // IP_AllowWithoutAuth = 0: Strictly require authentication; prevent fallback to anonymous SteamID which causes STEAM_ID_MISMATCH (rejection 49)
                    optList.RemoveAll(o => o.m_eValue == ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth);
                    optList.Add(new SteamNetworkingConfigValue_t {
                        m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
                        m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                        m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 0 }
                    });
                    var actualOpts = optList.ToArray();

                    HSteamNetConnection conn = SteamAPI_ISteamNetworkingSockets_ConnectP2P(interfacePtr, ref identityRemote, nRemoteVirtualPort, actualOpts.Length, actualOpts);
                    SteamShimLogger.Info($"[SteamNetworkingSockets] ConnectP2P returned connection handle: {conn.m_HSteamNetConnection}");
                    return conn;
                } catch (Exception ex) {
                    SteamShimLogger.Error($"[SteamNetworkingSockets] ConnectP2P exception: {ex}");
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

        public static void ReleaseMessage(IntPtr pMsg) {
            if (pMsg != IntPtr.Zero) {
                try {
                    SteamAPI_SteamNetworkingMessage_t_Release(pMsg);
                } catch { }
            }
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
