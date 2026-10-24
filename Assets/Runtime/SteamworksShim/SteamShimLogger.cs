using System;

namespace Steamworks {
    /// <summary>
    /// Bridges SteamworksShim logs to Unturned's logging system (UnturnedLog.info / Logs.printLine).
    /// </summary>
    public static class SteamShimLogger {
        public static Action<string> LogInfo;
        public static Action<string> LogWarn;
        public static Action<string> LogError;

        public static void Info(string msg) {
            try {
                if (LogInfo != null) LogInfo(msg);
                else Console.WriteLine(msg);
            } catch { }
        }

        public static void Warn(string msg) {
            try {
                if (LogWarn != null) LogWarn(msg);
                else Console.WriteLine(msg);
            } catch { }
        }

        public static void Error(string msg) {
            try {
                if (LogError != null) LogError(msg);
                else Console.WriteLine(msg);
            } catch { }
        }
    }
}
