using System;
using System.Collections.Concurrent;
using UnityEngine;
using Steamworks;
using SDG.Unturned;

namespace SDG.Unturned
{
    public static class SteamBridge
    {
        private static readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();
        private static bool isInitialized = false;

        public static void EnsureInitialized()
        {
            if (isInitialized) return;
            isInitialized = true;

            SDG.Framework.Utilities.TimeUtility.updated += ProcessMainThreadQueue;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                {
                    bridgeClass.CallStatic("init");
                }
                UnturnedLog.info("[SteamBridge] JavaSteam initialized.");
                CheckAutoLogin();
            }
            catch (Exception ex)
            {
                UnturnedLog.warn($"[SteamBridge] Java init note: {ex.Message}");
            }
#endif
        }

        private static void ProcessMainThreadQueue()
        {
            while (mainThreadQueue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    UnturnedLog.exception(ex, "[SteamBridge] Main thread dispatch error:");
                }
            }
        }

        public static void EnqueueMainThread(Action action)
        {
            if (action != null)
            {
                mainThreadQueue.Enqueue(action);
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private class JavaSteamLoginCallbackProxy : AndroidJavaProxy
        {
            private readonly Action<string, Texture2D> onQrReady;
            private readonly Action<ulong, string> onLoginSuccess;
            private readonly Action<string> onLoginFailed;

            public JavaSteamLoginCallbackProxy(
                Action<string, Texture2D> onQrReady,
                Action<ulong, string> onLoginSuccess,
                Action<string> onLoginFailed)
                : base("com.smartlydressedgames.unturned.SteamBridge$SteamLoginCallback")
            {
                this.onQrReady = onQrReady;
                this.onLoginSuccess = onLoginSuccess;
                this.onLoginFailed = onLoginFailed;
            }

            public void onQrReady(string url, byte[] pngBytes)
            {
                EnqueueMainThread(() =>
                {
                    Texture2D tex = null;
                    if (pngBytes != null && pngBytes.Length > 0)
                    {
                        try
                        {
                            tex = new Texture2D(384, 384, TextureFormat.RGBA32, false);
                            tex.filterMode = FilterMode.Point;
                            tex.LoadImage(pngBytes);
                        }
                        catch (Exception ex)
                        {
                            UnturnedLog.warn($"[SteamBridge] Error loading QR image: {ex.Message}");
                        }
                    }
                    onQrReady?.Invoke(url, tex);
                });
            }

            public void onLoginSuccess(long steamId, string accountName, string refreshToken)
            {
                EnqueueMainThread(() =>
                {
                    ulong uSteamId = (ulong)steamId;
                    PlayerPrefs.SetString("SteamAccountName", accountName);
                    PlayerPrefs.SetString("SteamRefreshToken", refreshToken);
                    PlayerPrefs.SetString("SteamID64", uSteamId.ToString());
                    PlayerPrefs.SetString("SteamPersonaName", accountName);
                    PlayerPrefs.Save();

                    SteamUser.ConfiguredSteamID = uSteamId;
                    SteamFriends.ConfiguredPersonaName = accountName;
                    Provider.UpdateClientSteamCredentials();

                    UnturnedLog.info($"[SteamBridge] Successfully logged on via JavaSteam! SteamID: {uSteamId}, Account: {accountName}");
                    onLoginSuccess?.Invoke(uSteamId, accountName);
                });
            }

            public void onLoginFailed(string error)
            {
                EnqueueMainThread(() =>
                {
                    UnturnedLog.warn($"[SteamBridge] Login failed: {error}");
                    onLoginFailed?.Invoke(error);
                });
            }
        }
#endif

        public static void StartQrLogin(
            Action<string, Texture2D> onQrReady,
            Action<ulong, string> onLoginSuccess,
            Action<string> onLoginFailed)
        {
            EnsureInitialized();
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                {
                    bridgeClass.CallStatic("startQrLogin", new JavaSteamLoginCallbackProxy(onQrReady, onLoginSuccess, onLoginFailed));
                }
            }
            catch (Exception ex)
            {
                UnturnedLog.error($"[SteamBridge] StartQrLogin exception: {ex}");
                onLoginFailed?.Invoke(ex.Message);
            }
#else
            onLoginFailed?.Invoke("Steam QR Login está disponível na compilação Android.");
#endif
        }

        public static void LoginWithToken(
            string accountName,
            string refreshToken,
            Action<string, Texture2D> onQrReady,
            Action<ulong, string> onLoginSuccess,
            Action<string> onLoginFailed)
        {
            EnsureInitialized();
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                {
                    bridgeClass.CallStatic("loginWithToken", accountName, refreshToken, new JavaSteamLoginCallbackProxy(onQrReady, onLoginSuccess, onLoginFailed));
                }
            }
            catch (Exception ex)
            {
                UnturnedLog.warn($"[SteamBridge] LoginWithToken exception: {ex.Message}");
                onLoginFailed?.Invoke(ex.Message);
            }
#endif
        }

        public static void CheckAutoLogin()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (PlayerPrefs.HasKey("SteamAccountName") && PlayerPrefs.HasKey("SteamRefreshToken"))
            {
                string accountName = PlayerPrefs.GetString("SteamAccountName");
                string refreshToken = PlayerPrefs.GetString("SteamRefreshToken");
                if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(refreshToken))
                {
                    UnturnedLog.info($"[SteamBridge] Attempting silent auto-login for '{accountName}'...");
                    LoginWithToken(accountName, refreshToken, null, (sId, name) =>
                    {
                        UnturnedLog.info($"[SteamBridge] Silent auto-login succeeded for {name} ({sId})");
                    }, (err) =>
                    {
                        UnturnedLog.warn($"[SteamBridge] Silent auto-login note: {err}");
                    });
                }
            }
#endif
        }

        public static byte[] GetAuthSessionTicket(uint appId)
        {
            EnsureInitialized();
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                {
                    return bridgeClass.CallStatic<byte[]>("getAuthSessionTicket", (int)appId);
                }
            }
            catch (Exception ex)
            {
                UnturnedLog.warn($"[SteamBridge] GetAuthSessionTicket exception: {ex.Message}");
            }
#endif
            return null;
        }

        public static bool IsLoggedOn
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                    {
                        return bridgeClass.CallStatic<bool>("isLoggedOn");
                    }
                }
                catch
                {
                    return false;
                }
#else
                return false;
#endif
            }
        }

        public static ulong SteamId
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                    {
                        long val = bridgeClass.CallStatic<long>("getSteamId");
                        return (ulong)val;
                    }
                }
                catch
                {
                    return 0;
                }
#else
                return 0;
#endif
            }
        }

        public static string AccountName
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                    {
                        return bridgeClass.CallStatic<string>("getAccountName");
                    }
                }
                catch
                {
                    return string.Empty;
                }
#else
                return string.Empty;
#endif
            }
        }

        public static void LogOff()
        {
            PlayerPrefs.DeleteKey("SteamRefreshToken");
            PlayerPrefs.Save();
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass("com.smartlydressedgames.unturned.SteamBridge"))
                {
                    bridgeClass.CallStatic("logOff");
                }
            }
            catch (Exception ex)
            {
                UnturnedLog.warn($"[SteamBridge] LogOff exception: {ex.Message}");
            }
#endif
        }
    }
}
