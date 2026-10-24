// Steamworks Shim Callbacks for Android
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
    public abstract class Callback {
        public abstract bool IsGameServer { get; }
    }

    public sealed class Callback<T> : Callback, IDisposable {
        public delegate void DispatchDelegate(T param);
        public static Callback<T> Create(DispatchDelegate func) => new Callback<T>(func, false);
        public static Callback<T> CreateGameServer(DispatchDelegate func) => new Callback<T>(func, true);

        private bool m_bGameServer;
        public Callback(DispatchDelegate func, bool bGameServer = false) {
            m_bGameServer = bGameServer;
        }

        public override bool IsGameServer => m_bGameServer;
        public void Dispose() {}
        public void Register(DispatchDelegate func) {}
        public void Unregister() {}
    }

    public abstract class CallResult {}

    public sealed class CallResult<T> : CallResult, IDisposable {
        public delegate void APIDispatchDelegate(T param, bool bIOFailure);
        public static CallResult<T> Create(APIDispatchDelegate func = null) => new CallResult<T>(func);

        private SteamAPICall_t m_hAPICall = SteamAPICall_t.Invalid;
        public SteamAPICall_t Handle => m_hAPICall;

        public CallResult(APIDispatchDelegate func = null) {}
        public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate func = null) {
            m_hAPICall = hAPICall;
        }
        public bool IsActive() => false;
        public void Cancel() {}
        public void Dispose() {}
    }

    public static class CallbackDispatcher {
        public static bool IsInitialized => true;
        public static void ExceptionHandler(Exception e) {}
    }

    // Matchmaking response callback classes
    public class ISteamMatchmakingPingResponse {
        public delegate void ServerResponded(gameserveritem_t server);
        public delegate void ServerFailedToRespond();
        public ServerResponded m_ServerResponded;
        public ServerFailedToRespond m_ServerFailedToRespond;
        public ISteamMatchmakingPingResponse(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond) {
            m_ServerResponded = onServerResponded;
            m_ServerFailedToRespond = onServerFailedToRespond;
        }
    }

    public class ISteamMatchmakingPlayersResponse {
        public delegate void AddPlayerToList(string pchName, int nScore, float flTimePlayed);
        public delegate void PlayersFailedToRespond();
        public delegate void PlayersRefreshComplete();
        public ISteamMatchmakingPlayersResponse(AddPlayerToList onAddPlayerToList, PlayersFailedToRespond onPlayersFailedToRespond, PlayersRefreshComplete onPlayersRefreshComplete) {}
    }

    public class ISteamMatchmakingRulesResponse {
        public delegate void RulesResponded(string pchRule, string pchValue);
        public delegate void RulesFailedToRespond();
        public delegate void RulesRefreshComplete();
        public ISteamMatchmakingRulesResponse(RulesResponded onRulesResponded, RulesFailedToRespond onRulesFailedToRespond, RulesRefreshComplete onRulesRefreshComplete) {}
    }

    public class ISteamMatchmakingServerListResponse {
        public delegate void ServerResponded(HServerListRequest hRequest, int iServer);
        public delegate void ServerFailedToRespond(HServerListRequest hRequest, int iServer);
        public delegate void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response);
        public ServerResponded m_ServerResponded;
        public ServerFailedToRespond m_ServerFailedToRespond;
        public RefreshComplete m_RefreshComplete;
        public ISteamMatchmakingServerListResponse(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond, RefreshComplete onRefreshComplete) {
            m_ServerResponded = onServerResponded;
            m_ServerFailedToRespond = onServerFailedToRespond;
            m_RefreshComplete = onRefreshComplete;
        }
    }
}
