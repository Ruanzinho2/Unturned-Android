// Steamworks Shim API Classes for Android
using System;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Steamworks {
    public static class SteamAppList {
        public static uint GetNumInstalledApps() {
            return 0;
        }
        public static uint GetInstalledApps(AppId_t[] pvecAppID, uint unMaxAppIDs) {
            return 0;
        }
        public static int GetAppName(AppId_t nAppID, out string pchName, int cchNameMax) {
            pchName = default;
            return 0;
        }
        public static int GetAppInstallDir(AppId_t nAppID, out string pchDirectory, int cchNameMax) {
            pchDirectory = default;
            return 0;
        }
        public static int GetAppBuildId(AppId_t nAppID) {
            return 0;
        }
    }

    public static class SteamApps {
        public static bool BIsSubscribed() {
            return false;
        }
        public static bool BIsLowViolence() {
            return false;
        }
        public static bool BIsCybercafe() {
            return false;
        }
        public static bool BIsVACBanned() {
            return false;
        }
        public static string GetCurrentGameLanguage() {
            return string.Empty;
        }
        public static string GetAvailableGameLanguages() {
            return string.Empty;
        }
        public static bool BIsSubscribedApp(AppId_t appID) {
            return false;
        }
        public static bool BIsDlcInstalled(AppId_t appID) {
            return false;
        }
        public static uint GetEarliestPurchaseUnixTime(AppId_t nAppID) {
            return 0;
        }
        public static bool BIsSubscribedFromFreeWeekend() {
            return false;
        }
        public static int GetDLCCount() {
            return 0;
        }
        public static bool BGetDLCDataByIndex(int iDLC, out AppId_t pAppID, out bool pbAvailable, out string pchName, int cchNameBufferSize) {
            pAppID = default;
            pbAvailable = default;
            pchName = default;
            return false;
        }
        public static void InstallDLC(AppId_t nAppID) {
            return;
        }
        public static void UninstallDLC(AppId_t nAppID) {
            return;
        }
        public static void RequestAppProofOfPurchaseKey(AppId_t nAppID) {
            return;
        }
        public static bool GetCurrentBetaName(out string pchName, int cchNameBufferSize) {
            pchName = default;
            return false;
        }
        public static bool MarkContentCorrupt(bool bMissingFilesOnly) {
            return false;
        }
        public static uint GetInstalledDepots(AppId_t appID, DepotId_t[] pvecDepots, uint cMaxDepots) {
            return 0;
        }
        public static uint GetAppInstallDir(AppId_t appID, out string pchFolder, uint cchFolderBufferSize) {
#if UNITY_ANDROID
            pchFolder = null;
            return 0;
#else
            pchFolder = System.IO.Directory.GetCurrentDirectory();
            return (uint)pchFolder.Length;
#endif
        }
        public static bool BIsAppInstalled(AppId_t appID) {
            return false;
        }
        public static CSteamID GetAppOwner() {
            return new CSteamID(76561198000000000UL);
        }
        public static string GetLaunchQueryParam(string pchKey) {
            return string.Empty;
        }
        public static bool GetDlcDownloadProgress(AppId_t nAppID, out ulong punBytesDownloaded, out ulong punBytesTotal) {
            punBytesDownloaded = default;
            punBytesTotal = default;
            return false;
        }
        public static int GetAppBuildId() {
            return 0;
        }
        public static void RequestAllProofOfPurchaseKeys() {
            return;
        }
        public static SteamAPICall_t GetFileDetails(string pszFileName) {
            return SteamAPICall_t.Invalid;
        }
        public static int GetLaunchCommandLine(out string pszCommandLine, int cubCommandLine) {
            pszCommandLine = default;
            return 0;
        }
        public static bool BIsSubscribedFromFamilySharing() {
            return false;
        }
        public static bool BIsTimedTrial(out uint punSecondsAllowed, out uint punSecondsPlayed) {
            punSecondsAllowed = default;
            punSecondsPlayed = default;
            return false;
        }
        public static bool SetDlcContext(AppId_t nAppID) {
            return false;
        }
    }

    public static class SteamClient {
        public static HSteamPipe CreateSteamPipe() {
            return default;
        }
        public static bool BReleaseSteamPipe(HSteamPipe hSteamPipe) {
            return false;
        }
        public static HSteamUser ConnectToGlobalUser(HSteamPipe hSteamPipe) {
            return default;
        }
        public static HSteamUser CreateLocalUser(out HSteamPipe phSteamPipe, EAccountType eAccountType) {
            phSteamPipe = default;
            return default;
        }
        public static void ReleaseUser(HSteamPipe hSteamPipe, HSteamUser hUser) {
            return;
        }
        public static IntPtr GetISteamUser(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameServer(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static void SetLocalIPBinding(ref SteamIPAddress_t unIP, ushort usPort) {
            return;
        }
        public static IntPtr GetISteamFriends(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUtils(HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMatchmaking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMatchmakingServers(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGenericInterface(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUserStats(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameServerStats(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamApps(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamNetworking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamRemoteStorage(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamScreenshots(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameSearch(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static uint GetIPCCallCount() {
            return 0;
        }
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) {
            return;
        }
        public static bool BShutdownIfAllPipesClosed() {
            return false;
        }
        public static IntPtr GetISteamHTTP(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamController(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUGC(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamAppList(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMusic(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMusicRemote(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamHTMLSurface(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamInventory(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamVideo(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamParentalSettings(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamInput(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamParties(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamRemotePlay(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
    }

    public static class SteamFriends {
        public static string GetPersonaName() {
            return "Survivor";
        }
        public static SteamAPICall_t SetPersonaName(string pchPersonaName) {
            return SteamAPICall_t.Invalid;
        }
        public static EPersonaState GetPersonaState() {
            return default;
        }
        public static int GetFriendCount(EFriendFlags iFriendFlags) {
            return 0;
        }
        public static CSteamID GetFriendByIndex(int iFriend, EFriendFlags iFriendFlags) {
            return new CSteamID(76561198000000000UL);
        }
        public static EFriendRelationship GetFriendRelationship(CSteamID steamIDFriend) {
            return default;
        }
        public static EPersonaState GetFriendPersonaState(CSteamID steamIDFriend) {
            return default;
        }
        public static string GetFriendPersonaName(CSteamID steamIDFriend) {
            return string.Empty;
        }
        public static bool GetFriendGamePlayed(CSteamID steamIDFriend, out FriendGameInfo_t pFriendGameInfo) {
            pFriendGameInfo = default;
            return false;
        }
        public static string GetFriendPersonaNameHistory(CSteamID steamIDFriend, int iPersonaName) {
            return string.Empty;
        }
        public static int GetFriendSteamLevel(CSteamID steamIDFriend) {
            return 0;
        }
        public static string GetPlayerNickname(CSteamID steamIDPlayer) {
            return string.Empty;
        }
        public static int GetFriendsGroupCount() {
            return 0;
        }
        public static FriendsGroupID_t GetFriendsGroupIDByIndex(int iFG) {
            return default;
        }
        public static string GetFriendsGroupName(FriendsGroupID_t friendsGroupID) {
            return string.Empty;
        }
        public static int GetFriendsGroupMembersCount(FriendsGroupID_t friendsGroupID) {
            return 0;
        }
        public static void GetFriendsGroupMembersList(FriendsGroupID_t friendsGroupID, CSteamID[] pOutSteamIDMembers, int nMembersCount) {
            return;
        }
        public static bool HasFriend(CSteamID steamIDFriend, EFriendFlags iFriendFlags) {
            return false;
        }
        public static int GetClanCount() {
            return 0;
        }
        public static CSteamID GetClanByIndex(int iClan) {
            return new CSteamID(76561198000000000UL);
        }
        public static string GetClanName(CSteamID steamIDClan) {
            return string.Empty;
        }
        public static string GetClanTag(CSteamID steamIDClan) {
            return string.Empty;
        }
        public static bool GetClanActivityCounts(CSteamID steamIDClan, out int pnOnline, out int pnInGame, out int pnChatting) {
            pnOnline = default;
            pnInGame = default;
            pnChatting = default;
            return false;
        }
        public static SteamAPICall_t DownloadClanActivityCounts(CSteamID[] psteamIDClans, int cClansToRequest) {
            return SteamAPICall_t.Invalid;
        }
        public static int GetFriendCountFromSource(CSteamID steamIDSource) {
            return 0;
        }
        public static CSteamID GetFriendFromSourceByIndex(CSteamID steamIDSource, int iFriend) {
            return new CSteamID(76561198000000000UL);
        }
        public static bool IsUserInSource(CSteamID steamIDUser, CSteamID steamIDSource) {
            return false;
        }
        public static void SetInGameVoiceSpeaking(CSteamID steamIDUser, bool bSpeaking) {
            return;
        }
        public static void ActivateGameOverlay(string pchDialog) {
            return;
        }
        public static void ActivateGameOverlayToUser(string pchDialog, CSteamID steamID) {
            return;
        }
        public static void ActivateGameOverlayToWebPage(string pchURL, EActivateGameOverlayToWebPageMode eMode = EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default) {
            return;
        }
        public static void ActivateGameOverlayToStore(AppId_t nAppID, EOverlayToStoreFlag eFlag) {
            return;
        }
        public static void SetPlayedWith(CSteamID steamIDUserPlayedWith) {
            return;
        }
        public static void ActivateGameOverlayInviteDialog(CSteamID steamIDLobby) {
            return;
        }
        public static int GetSmallFriendAvatar(CSteamID steamIDFriend) {
            return 0;
        }
        public static int GetMediumFriendAvatar(CSteamID steamIDFriend) {
            return 0;
        }
        public static int GetLargeFriendAvatar(CSteamID steamIDFriend) {
            return 0;
        }
        public static bool RequestUserInformation(CSteamID steamIDUser, bool bRequireNameOnly) {
            return false;
        }
        public static SteamAPICall_t RequestClanOfficerList(CSteamID steamIDClan) {
            return SteamAPICall_t.Invalid;
        }
        public static CSteamID GetClanOwner(CSteamID steamIDClan) {
            return new CSteamID(76561198000000000UL);
        }
        public static int GetClanOfficerCount(CSteamID steamIDClan) {
            return 0;
        }
        public static CSteamID GetClanOfficerByIndex(CSteamID steamIDClan, int iOfficer) {
            return new CSteamID(76561198000000000UL);
        }
        public static uint GetUserRestrictions() {
            return 0;
        }
        public static bool SetRichPresence(string pchKey, string pchValue) {
            return false;
        }
        public static void ClearRichPresence() {
            return;
        }
        public static string GetFriendRichPresence(CSteamID steamIDFriend, string pchKey) {
            return string.Empty;
        }
        public static int GetFriendRichPresenceKeyCount(CSteamID steamIDFriend) {
            return 0;
        }
        public static string GetFriendRichPresenceKeyByIndex(CSteamID steamIDFriend, int iKey) {
            return string.Empty;
        }
        public static void RequestFriendRichPresence(CSteamID steamIDFriend) {
            return;
        }
        public static bool InviteUserToGame(CSteamID steamIDFriend, string pchConnectString) {
            return false;
        }
        public static int GetCoplayFriendCount() {
            return 0;
        }
        public static CSteamID GetCoplayFriend(int iCoplayFriend) {
            return new CSteamID(76561198000000000UL);
        }
        public static int GetFriendCoplayTime(CSteamID steamIDFriend) {
            return 0;
        }
        public static AppId_t GetFriendCoplayGame(CSteamID steamIDFriend) {
            return new AppId_t(304930);
        }
        public static SteamAPICall_t JoinClanChatRoom(CSteamID steamIDClan) {
            return SteamAPICall_t.Invalid;
        }
        public static bool LeaveClanChatRoom(CSteamID steamIDClan) {
            return false;
        }
        public static int GetClanChatMemberCount(CSteamID steamIDClan) {
            return 0;
        }
        public static CSteamID GetChatMemberByIndex(CSteamID steamIDClan, int iUser) {
            return new CSteamID(76561198000000000UL);
        }
        public static bool SendClanChatMessage(CSteamID steamIDClanChat, string pchText) {
            return false;
        }
        public static int GetClanChatMessage(CSteamID steamIDClanChat, int iMessage, out string prgchText, int cchTextMax, out EChatEntryType peChatEntryType, out CSteamID psteamidChatter) {
            prgchText = default;
            peChatEntryType = default;
            psteamidChatter = default;
            return 0;
        }
        public static bool IsClanChatAdmin(CSteamID steamIDClanChat, CSteamID steamIDUser) {
            return false;
        }
        public static bool IsClanChatWindowOpenInSteam(CSteamID steamIDClanChat) {
            return false;
        }
        public static bool OpenClanChatWindowInSteam(CSteamID steamIDClanChat) {
            return false;
        }
        public static bool CloseClanChatWindowInSteam(CSteamID steamIDClanChat) {
            return false;
        }
        public static bool SetListenForFriendsMessages(bool bInterceptEnabled) {
            return false;
        }
        public static bool ReplyToFriendMessage(CSteamID steamIDFriend, string pchMsgToSend) {
            return false;
        }
        public static int GetFriendMessage(CSteamID steamIDFriend, int iMessageID, out string pvData, int cubData, out EChatEntryType peChatEntryType) {
            pvData = default;
            peChatEntryType = default;
            return 0;
        }
        public static SteamAPICall_t GetFollowerCount(CSteamID steamID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t IsFollowing(CSteamID steamID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumerateFollowingList(uint unStartIndex) {
            return SteamAPICall_t.Invalid;
        }
        public static bool IsClanPublic(CSteamID steamIDClan) {
            return false;
        }
        public static bool IsClanOfficialGameGroup(CSteamID steamIDClan) {
            return false;
        }
        public static int GetNumChatsWithUnreadPriorityMessages() {
            return 0;
        }
        public static void ActivateGameOverlayRemotePlayTogetherInviteDialog(CSteamID steamIDLobby) {
            return;
        }
        public static bool RegisterProtocolInOverlayBrowser(string pchProtocol) {
            return false;
        }
        public static void ActivateGameOverlayInviteDialogConnectString(string pchConnectString) {
            return;
        }
        public static SteamAPICall_t RequestEquippedProfileItems(CSteamID steamID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool BHasEquippedProfileItem(CSteamID steamID, ECommunityProfileItemType itemType) {
            return false;
        }
        public static string GetProfileItemPropertyString(CSteamID steamID, ECommunityProfileItemType itemType, ECommunityProfileItemProperty prop) {
            return string.Empty;
        }
        public static uint GetProfileItemPropertyUint(CSteamID steamID, ECommunityProfileItemType itemType, ECommunityProfileItemProperty prop) {
            return 0;
        }
    }

    public static class SteamGameServer {
        public static void SetProduct(string pszProduct) {
            return;
        }
        public static void SetGameDescription(string pszGameDescription) {
            return;
        }
        public static void SetModDir(string pszModDir) {
            return;
        }
        public static void SetDedicatedServer(bool bDedicated) {
            return;
        }
        public static void LogOn(string pszToken) {
            return;
        }
        public static void LogOnAnonymous() {
            return;
        }
        public static void LogOff() {
            return;
        }
        public static bool BLoggedOn() {
            return false;
        }
        public static bool BSecure() {
            return false;
        }
        public static CSteamID GetSteamID() {
            return new CSteamID(76561198000000000UL);
        }
        public static bool WasRestartRequested() {
            return false;
        }
        public static void SetMaxPlayerCount(int cPlayersMax) {
            return;
        }
        public static void SetBotPlayerCount(int cBotplayers) {
            return;
        }
        public static void SetServerName(string pszServerName) {
            return;
        }
        public static void SetMapName(string pszMapName) {
            return;
        }
        public static void SetPasswordProtected(bool bPasswordProtected) {
            return;
        }
        public static void SetSpectatorPort(ushort unSpectatorPort) {
            return;
        }
        public static void SetSpectatorServerName(string pszSpectatorServerName) {
            return;
        }
        public static void ClearAllKeyValues() {
            return;
        }
        public static void SetKeyValue(string pKey, string pValue) {
            return;
        }
        public static void SetGameTags(string pchGameTags) {
            return;
        }
        public static void SetGameData(string pchGameData) {
            return;
        }
        public static void SetRegion(string pszRegion) {
            return;
        }
        public static void SetAdvertiseServerActive(bool bActive) {
            return;
        }
        public static HAuthTicket GetAuthSessionTicket(byte[] pTicket, int cbMaxTicket, out uint pcbTicket, ref SteamNetworkingIdentity pSnid) {
            pcbTicket = default;
            return HAuthTicket.Invalid;
        }
        public static EBeginAuthSessionResult BeginAuthSession(byte[] pAuthTicket, int cbAuthTicket, CSteamID steamID) {
            return default;
        }
        public static void EndAuthSession(CSteamID steamID) {
            return;
        }
        public static void CancelAuthTicket(HAuthTicket hAuthTicket) {
            return;
        }
        public static EUserHasLicenseForAppResult UserHasLicenseForApp(CSteamID steamID, AppId_t appID) {
            return default;
        }
        public static bool RequestUserGroupStatus(CSteamID steamIDUser, CSteamID steamIDGroup) {
            return false;
        }
        public static void GetGameplayStats() {
            return;
        }
        public static SteamAPICall_t GetServerReputation() {
            return SteamAPICall_t.Invalid;
        }
        public static SteamIPAddress_t GetPublicIP() {
            return default;
        }
        public static bool HandleIncomingPacket(byte[] pData, int cbData, uint srcIP, ushort srcPort) {
            return false;
        }
        public static int GetNextOutgoingPacket(byte[] pOut, int cbMaxOut, out uint pNetAdr, out ushort pPort) {
            pNetAdr = default;
            pPort = default;
            return 0;
        }
        public static SteamAPICall_t AssociateWithClan(CSteamID steamIDClan) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t ComputeNewPlayerCompatibility(CSteamID steamIDNewPlayer) {
            return SteamAPICall_t.Invalid;
        }
        public static bool SendUserConnectAndAuthenticate_DEPRECATED(uint unIPClient, byte[] pvAuthBlob, uint cubAuthBlobSize, out CSteamID pSteamIDUser) {
            pSteamIDUser = default;
            return false;
        }
        public static CSteamID CreateUnauthenticatedUserConnection() {
            return new CSteamID(76561198000000000UL);
        }
        public static void SendUserDisconnect_DEPRECATED(CSteamID steamIDUser) {
            return;
        }
        public static bool BUpdateUserData(CSteamID steamIDUser, string pchPlayerName, uint uScore) {
            return false;
        }
    }

    public static class SteamGameServerClient {
        public static HSteamPipe CreateSteamPipe() {
            return default;
        }
        public static bool BReleaseSteamPipe(HSteamPipe hSteamPipe) {
            return false;
        }
        public static HSteamUser ConnectToGlobalUser(HSteamPipe hSteamPipe) {
            return default;
        }
        public static HSteamUser CreateLocalUser(out HSteamPipe phSteamPipe, EAccountType eAccountType) {
            phSteamPipe = default;
            return default;
        }
        public static void ReleaseUser(HSteamPipe hSteamPipe, HSteamUser hUser) {
            return;
        }
        public static IntPtr GetISteamUser(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameServer(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static void SetLocalIPBinding(ref SteamIPAddress_t unIP, ushort usPort) {
            return;
        }
        public static IntPtr GetISteamFriends(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUtils(HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMatchmaking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMatchmakingServers(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGenericInterface(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUserStats(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameServerStats(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamApps(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamNetworking(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamRemoteStorage(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamScreenshots(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamGameSearch(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static uint GetIPCCallCount() {
            return 0;
        }
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) {
            return;
        }
        public static bool BShutdownIfAllPipesClosed() {
            return false;
        }
        public static IntPtr GetISteamHTTP(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamController(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamUGC(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamAppList(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMusic(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamMusicRemote(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamHTMLSurface(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamInventory(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamVideo(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamParentalSettings(HSteamUser hSteamuser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamInput(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamParties(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
        public static IntPtr GetISteamRemotePlay(HSteamUser hSteamUser, HSteamPipe hSteamPipe, string pchVersion) {
            return IntPtr.Zero;
        }
    }

    public static class SteamGameServerHTTP {
        public static HTTPRequestHandle CreateHTTPRequest(EHTTPMethod eHTTPRequestMethod, string pchAbsoluteURL) {
            return default;
        }
        public static bool SetHTTPRequestContextValue(HTTPRequestHandle hRequest, ulong ulContextValue) {
            return false;
        }
        public static bool SetHTTPRequestNetworkActivityTimeout(HTTPRequestHandle hRequest, uint unTimeoutSeconds) {
            return false;
        }
        public static bool SetHTTPRequestHeaderValue(HTTPRequestHandle hRequest, string pchHeaderName, string pchHeaderValue) {
            return false;
        }
        public static bool SetHTTPRequestGetOrPostParameter(HTTPRequestHandle hRequest, string pchParamName, string pchParamValue) {
            return false;
        }
        public static bool SendHTTPRequest(HTTPRequestHandle hRequest, out SteamAPICall_t pCallHandle) {
            pCallHandle = default;
            return false;
        }
        public static bool SendHTTPRequestAndStreamResponse(HTTPRequestHandle hRequest, out SteamAPICall_t pCallHandle) {
            pCallHandle = default;
            return false;
        }
        public static bool DeferHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool PrioritizeHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool GetHTTPResponseHeaderSize(HTTPRequestHandle hRequest, string pchHeaderName, out uint unResponseHeaderSize) {
            unResponseHeaderSize = default;
            return false;
        }
        public static bool GetHTTPResponseHeaderValue(HTTPRequestHandle hRequest, string pchHeaderName, byte[] pHeaderValueBuffer, uint unBufferSize) {
            return false;
        }
        public static bool GetHTTPResponseBodySize(HTTPRequestHandle hRequest, out uint unBodySize) {
            unBodySize = default;
            return false;
        }
        public static bool GetHTTPResponseBodyData(HTTPRequestHandle hRequest, byte[] pBodyDataBuffer, uint unBufferSize) {
            return false;
        }
        public static bool GetHTTPStreamingResponseBodyData(HTTPRequestHandle hRequest, uint cOffset, byte[] pBodyDataBuffer, uint unBufferSize) {
            return false;
        }
        public static bool ReleaseHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool GetHTTPDownloadProgressPct(HTTPRequestHandle hRequest, out float pflPercentOut) {
            pflPercentOut = default;
            return false;
        }
        public static bool SetHTTPRequestRawPostBody(HTTPRequestHandle hRequest, string pchContentType, byte[] pubBody, uint unBodyLen) {
            return false;
        }
        public static HTTPCookieContainerHandle CreateCookieContainer(bool bAllowResponsesToModify) {
            return default;
        }
        public static bool ReleaseCookieContainer(HTTPCookieContainerHandle hCookieContainer) {
            return false;
        }
        public static bool SetCookie(HTTPCookieContainerHandle hCookieContainer, string pchHost, string pchUrl, string pchCookie) {
            return false;
        }
        public static bool SetHTTPRequestCookieContainer(HTTPRequestHandle hRequest, HTTPCookieContainerHandle hCookieContainer) {
            return false;
        }
        public static bool SetHTTPRequestUserAgentInfo(HTTPRequestHandle hRequest, string pchUserAgentInfo) {
            return false;
        }
        public static bool SetHTTPRequestRequiresVerifiedCertificate(HTTPRequestHandle hRequest, bool bRequireVerifiedCertificate) {
            return false;
        }
        public static bool SetHTTPRequestAbsoluteTimeoutMS(HTTPRequestHandle hRequest, uint unMilliseconds) {
            return false;
        }
        public static bool GetHTTPRequestWasTimedOut(HTTPRequestHandle hRequest, out bool pbWasTimedOut) {
            pbWasTimedOut = default;
            return false;
        }
    }

    public static class SteamGameServerInventory {
        public static EResult GetResultStatus(SteamInventoryResult_t resultHandle) {
            return default;
        }
        public static bool GetResultItems(SteamInventoryResult_t resultHandle, SteamItemDetails_t[] pOutItemsArray, ref uint punOutItemsArraySize) {
            return false;
        }
        public static bool GetResultItemProperty(SteamInventoryResult_t resultHandle, uint unItemIndex, string pchPropertyName, out string pchValueBuffer, ref uint punValueBufferSizeOut) {
            pchValueBuffer = default;
            return false;
        }
        public static uint GetResultTimestamp(SteamInventoryResult_t resultHandle) {
            return 0;
        }
        public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, CSteamID steamIDExpected) {
            return false;
        }
        public static void DestroyResult(SteamInventoryResult_t resultHandle) {
            return;
        }
        public static bool GetAllItems(out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool GetItemsByID(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t[] pInstanceIDs, uint unCountInstanceIDs) {
            pResultHandle = default;
            return false;
        }
        public static bool SerializeResult(SteamInventoryResult_t resultHandle, byte[] pOutBuffer, out uint punOutBufferSize) {
            punOutBufferSize = default;
            return false;
        }
        public static bool DeserializeResult(out SteamInventoryResult_t pOutResultHandle, byte[] pBuffer, uint unBufferSize, bool bRESERVED_MUST_BE_FALSE = false) {
            pOutResultHandle = default;
            return false;
        }
        public static bool GenerateItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayItemDefs, uint[] punArrayQuantity, uint unArrayLength) {
            pResultHandle = default;
            return false;
        }
        public static bool GrantPromoItems(out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool AddPromoItem(out SteamInventoryResult_t pResultHandle, SteamItemDef_t itemDef) {
            pResultHandle = default;
            return false;
        }
        public static bool AddPromoItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayItemDefs, uint unArrayLength) {
            pResultHandle = default;
            return false;
        }
        public static bool ConsumeItem(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t itemConsume, uint unQuantity) {
            pResultHandle = default;
            return false;
        }
        public static bool ExchangeItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayGenerate, uint[] punArrayGenerateQuantity, uint unArrayGenerateLength, SteamItemInstanceID_t[] pArrayDestroy, uint[] punArrayDestroyQuantity, uint unArrayDestroyLength) {
            pResultHandle = default;
            return false;
        }
        public static bool TransferItemQuantity(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t itemIdSource, uint unQuantity, SteamItemInstanceID_t itemIdDest) {
            pResultHandle = default;
            return false;
        }
        public static void SendItemDropHeartbeat() {
            return;
        }
        public static bool TriggerItemDrop(out SteamInventoryResult_t pResultHandle, SteamItemDef_t dropListDefinition) {
            pResultHandle = default;
            return false;
        }
        public static bool TradeItems(out SteamInventoryResult_t pResultHandle, CSteamID steamIDTradePartner, SteamItemInstanceID_t[] pArrayGive, uint[] pArrayGiveQuantity, uint nArrayGiveLength, SteamItemInstanceID_t[] pArrayGet, uint[] pArrayGetQuantity, uint nArrayGetLength) {
            pResultHandle = default;
            return false;
        }
        public static bool LoadItemDefinitions() {
            return false;
        }
        public static bool GetItemDefinitionIDs(SteamItemDef_t[] pItemDefIDs, ref uint punItemDefIDsArraySize) {
            return false;
        }
        public static bool GetItemDefinitionProperty(SteamItemDef_t iDefinition, string pchPropertyName, out string pchValueBuffer, ref uint punValueBufferSizeOut) {
            pchValueBuffer = default;
            return false;
        }
        public static SteamAPICall_t RequestEligiblePromoItemDefinitionsIDs(CSteamID steamID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetEligiblePromoItemDefinitionIDs(CSteamID steamID, SteamItemDef_t[] pItemDefIDs, ref uint punItemDefIDsArraySize) {
            return false;
        }
        public static SteamAPICall_t StartPurchase(SteamItemDef_t[] pArrayItemDefs, uint[] punArrayQuantity, uint unArrayLength) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RequestPrices() {
            return SteamAPICall_t.Invalid;
        }
        public static uint GetNumItemsWithPrices() {
            return 0;
        }
        public static bool GetItemsWithPrices(SteamItemDef_t[] pArrayItemDefs, ulong[] pCurrentPrices, ulong[] pBasePrices, uint unArrayLength) {
            return false;
        }
        public static bool GetItemPrice(SteamItemDef_t iDefinition, out ulong pCurrentPrice, out ulong pBasePrice) {
            pCurrentPrice = default;
            pBasePrice = default;
            return false;
        }
        public static SteamInventoryUpdateHandle_t StartUpdateProperties() {
            return SteamInventoryUpdateHandle_t.Invalid;
        }
        public static bool RemoveProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, string pchPropertyValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, bool bValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, long nValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, float flValue) {
            return false;
        }
        public static bool SubmitUpdateProperties(SteamInventoryUpdateHandle_t handle, out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool InspectItem(out SteamInventoryResult_t pResultHandle, string pchItemToken) {
            pResultHandle = default;
            return false;
        }
    }

    public static class SteamGameServerNetworking {
        public static bool SendP2PPacket(CSteamID steamIDRemote, byte[] pubData, uint cubData, EP2PSend eP2PSendType, int nChannel = 0) {
            return false;
        }
        public static bool IsP2PPacketAvailable(out uint pcubMsgSize, int nChannel = 0) {
            pcubMsgSize = default;
            return false;
        }
        public static bool ReadP2PPacket(byte[] pubDest, uint cubDest, out uint pcubMsgSize, out CSteamID psteamIDRemote, int nChannel = 0) {
            pcubMsgSize = default;
            psteamIDRemote = default;
            return false;
        }
        public static bool AcceptP2PSessionWithUser(CSteamID steamIDRemote) {
            return false;
        }
        public static bool CloseP2PSessionWithUser(CSteamID steamIDRemote) {
            return false;
        }
        public static bool CloseP2PChannelWithUser(CSteamID steamIDRemote, int nChannel) {
            return false;
        }
        public static bool GetP2PSessionState(CSteamID steamIDRemote, out P2PSessionState_t pConnectionState) {
            pConnectionState = default;
            return false;
        }
        public static bool AllowP2PPacketRelay(bool bAllow) {
            return false;
        }
        public static SNetListenSocket_t CreateListenSocket(int nVirtualP2PPort, SteamIPAddress_t nIP, ushort nPort, bool bAllowUseOfPacketRelay) {
            return default;
        }
        public static SNetSocket_t CreateP2PConnectionSocket(CSteamID steamIDTarget, int nVirtualPort, int nTimeoutSec, bool bAllowUseOfPacketRelay) {
            return default;
        }
        public static SNetSocket_t CreateConnectionSocket(SteamIPAddress_t nIP, ushort nPort, int nTimeoutSec) {
            return default;
        }
        public static bool DestroySocket(SNetSocket_t hSocket, bool bNotifyRemoteEnd) {
            return false;
        }
        public static bool DestroyListenSocket(SNetListenSocket_t hSocket, bool bNotifyRemoteEnd) {
            return false;
        }
        public static bool SendDataOnSocket(SNetSocket_t hSocket, byte[] pubData, uint cubData, bool bReliable) {
            return false;
        }
        public static bool IsDataAvailableOnSocket(SNetSocket_t hSocket, out uint pcubMsgSize) {
            pcubMsgSize = default;
            return false;
        }
        public static bool RetrieveDataFromSocket(SNetSocket_t hSocket, byte[] pubDest, uint cubDest, out uint pcubMsgSize) {
            pcubMsgSize = default;
            return false;
        }
        public static bool IsDataAvailable(SNetListenSocket_t hListenSocket, out uint pcubMsgSize, out SNetSocket_t phSocket) {
            pcubMsgSize = default;
            phSocket = default;
            return false;
        }
        public static bool RetrieveData(SNetListenSocket_t hListenSocket, byte[] pubDest, uint cubDest, out uint pcubMsgSize, out SNetSocket_t phSocket) {
            pcubMsgSize = default;
            phSocket = default;
            return false;
        }
        public static bool GetSocketInfo(SNetSocket_t hSocket, out CSteamID pSteamIDRemote, out int peSocketStatus, out SteamIPAddress_t punIPRemote, out ushort punPortRemote) {
            pSteamIDRemote = default;
            peSocketStatus = default;
            punIPRemote = default;
            punPortRemote = default;
            return false;
        }
        public static bool GetListenSocketInfo(SNetListenSocket_t hListenSocket, out SteamIPAddress_t pnIP, out ushort pnPort) {
            pnIP = default;
            pnPort = default;
            return false;
        }
        public static ESNetSocketConnectionType GetSocketConnectionType(SNetSocket_t hSocket) {
            return default;
        }
        public static int GetMaxPacketSize(SNetSocket_t hSocket) {
            return 0;
        }
    }

    public static class SteamGameServerNetworkingSockets {
        public static HSteamListenSocket CreateListenSocketIP(ref SteamNetworkingIPAddr localAddress, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static HSteamNetConnection ConnectByIPAddress(ref SteamNetworkingIPAddr address, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return NativeGameNetworkingSockets.ConnectByIPAddress(ref address, nOptions, pOptions);
        }
        public static HSteamListenSocket CreateListenSocketP2P(int nLocalVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static HSteamNetConnection ConnectP2P(ref SteamNetworkingIdentity identityRemote, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamNetConnection.Invalid;
        }
        public static EResult AcceptConnection(HSteamNetConnection hConn) {
            return default;
        }
        public static bool CloseConnection(HSteamNetConnection hPeer, int nReason, string pszDebug, bool bEnableLinger) {
            return NativeGameNetworkingSockets.CloseConnection(hPeer, nReason, pszDebug, bEnableLinger);
        }
        public static bool CloseListenSocket(HSteamListenSocket hSocket) {
            return false;
        }
        public static bool SetConnectionUserData(HSteamNetConnection hPeer, long nUserData) {
            return false;
        }
        public static long GetConnectionUserData(HSteamNetConnection hPeer) {
            return 0;
        }
        public static void SetConnectionName(HSteamNetConnection hPeer, string pszName) {
            return;
        }
        public static bool GetConnectionName(HSteamNetConnection hPeer, out string pszName, int nMaxLen) {
            pszName = default;
            return false;
        }
        public static EResult SendMessageToConnection(HSteamNetConnection hConn, IntPtr pData, uint cbData, int nSendFlags, out long pOutMessageNumber) {
            return NativeGameNetworkingSockets.SendMessageToConnection(hConn, pData, cbData, nSendFlags, out pOutMessageNumber);
        }
        public static void SendMessages(int nMessages, SteamNetworkingMessage_t[] pMessages, long[] pOutMessageNumberOrResult) {
            return;
        }
        public static EResult FlushMessagesOnConnection(HSteamNetConnection hConn) {
            return default;
        }
        public static int ReceiveMessagesOnConnection(HSteamNetConnection hConn, IntPtr[] ppOutMessages, int nMaxMessages) {
            return NativeGameNetworkingSockets.ReceiveMessagesOnConnection(hConn, ppOutMessages, nMaxMessages);
        }
        public static bool GetConnectionInfo(HSteamNetConnection hConn, out SteamNetConnectionInfo_t pInfo) {
            return NativeGameNetworkingSockets.GetConnectionInfo(hConn, out pInfo);
        }
        public static EResult GetConnectionRealTimeStatus(HSteamNetConnection hConn, ref SteamNetConnectionRealTimeStatus_t pStatus, int nLanes, ref SteamNetConnectionRealTimeLaneStatus_t pLanes) {
            return NativeGameNetworkingSockets.GetConnectionRealTimeStatus(hConn, ref pStatus, nLanes, ref pLanes);
        }
        public static int GetDetailedConnectionStatus(HSteamNetConnection hConn, out string pszBuf, int cbBuf) {
            pszBuf = default;
            return 0;
        }
        public static bool GetListenSocketAddress(HSteamListenSocket hSocket, out SteamNetworkingIPAddr address) {
            address = default;
            return false;
        }
        public static bool CreateSocketPair(out HSteamNetConnection pOutConnection1, out HSteamNetConnection pOutConnection2, bool bUseNetworkLoopback, ref SteamNetworkingIdentity pIdentity1, ref SteamNetworkingIdentity pIdentity2) {
            pOutConnection1 = default;
            pOutConnection2 = default;
            return false;
        }
        public static EResult ConfigureConnectionLanes(HSteamNetConnection hConn, int nNumLanes, out int pLanePriorities, out ushort pLaneWeights) {
            pLanePriorities = default;
            pLaneWeights = default;
            return default;
        }
        public static bool GetIdentity(out SteamNetworkingIdentity pIdentity) {
            pIdentity = default;
            return false;
        }
        public static ESteamNetworkingAvailability InitAuthentication() {
            return ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current;
        }
        public static ESteamNetworkingAvailability GetAuthenticationStatus(out SteamNetAuthenticationStatus_t pDetails) {
            pDetails = default;
            return default;
        }
        public static HSteamNetPollGroup CreatePollGroup() {
            return HSteamNetPollGroup.Invalid;
        }
        public static bool DestroyPollGroup(HSteamNetPollGroup hPollGroup) {
            return false;
        }
        public static bool SetConnectionPollGroup(HSteamNetConnection hConn, HSteamNetPollGroup hPollGroup) {
            return false;
        }
        public static int ReceiveMessagesOnPollGroup(HSteamNetPollGroup hPollGroup, IntPtr[] ppOutMessages, int nMaxMessages) {
            return 0;
        }
        public static bool ReceivedRelayAuthTicket(IntPtr pvTicket, int cbTicket, out SteamDatagramRelayAuthTicket pOutParsedTicket) {
            pOutParsedTicket = default;
            return false;
        }
        public static int FindRelayAuthTicketForServer(ref SteamNetworkingIdentity identityGameServer, int nRemoteVirtualPort, out SteamDatagramRelayAuthTicket pOutParsedTicket) {
            pOutParsedTicket = default;
            return 0;
        }
        public static HSteamNetConnection ConnectToHostedDedicatedServer(ref SteamNetworkingIdentity identityTarget, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamNetConnection.Invalid;
        }
        public static ushort GetHostedDedicatedServerPort() {
            return 0;
        }
        public static SteamNetworkingPOPID GetHostedDedicatedServerPOPID() {
            return default;
        }
        public static EResult GetHostedDedicatedServerAddress(out SteamDatagramHostedAddress pRouting) {
            pRouting = default;
            return default;
        }
        public static HSteamListenSocket CreateHostedDedicatedServerListenSocket(int nLocalVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static EResult GetGameCoordinatorServerLogin(IntPtr pLoginInfo, out int pcbSignedBlob, IntPtr pBlob) {
            pcbSignedBlob = default;
            return default;
        }
        public static HSteamNetConnection ConnectP2PCustomSignaling(out ISteamNetworkingConnectionSignaling pSignaling, ref SteamNetworkingIdentity pPeerIdentity, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            pSignaling = default;
            return HSteamNetConnection.Invalid;
        }
        public static bool ReceivedP2PCustomSignal(IntPtr pMsg, int cbMsg, out ISteamNetworkingSignalingRecvContext pContext) {
            pContext = default;
            return false;
        }
        public static bool GetCertificateRequest(out int pcbBlob, IntPtr pBlob, out SteamNetworkingErrMsg errMsg) {
            pcbBlob = default;
            errMsg = default;
            return false;
        }
        public static bool SetCertificate(IntPtr pCertificate, int cbCertificate, out SteamNetworkingErrMsg errMsg) {
            errMsg = default;
            return false;
        }
        public static void ResetIdentity(ref SteamNetworkingIdentity pIdentity) {
            return;
        }
        public static void RunCallbacks() {
            A2SPingQuery.RunCallbacks();
            NativeGameNetworkingSockets.RunCallbacks();
        }
        public static bool BeginAsyncRequestFakeIP(int nNumPorts) {
            return false;
        }
        public static void GetFakeIP(int idxFirstPort, out SteamNetworkingFakeIPResult_t pInfo) {
            pInfo = default;
            return;
        }
        public static HSteamListenSocket CreateListenSocketP2PFakeIP(int idxFakePort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static EResult GetRemoteFakeIPForConnection(HSteamNetConnection hConn, out SteamNetworkingIPAddr pOutAddr) {
            pOutAddr = default;
            return default;
        }
        public static IntPtr CreateFakeUDPPort(int idxFakeServerPort) {
            return IntPtr.Zero;
        }
    }

    public static class SteamGameServerNetworkingUtils {
        public static IntPtr AllocateMessage(int cbAllocateBuffer) {
            return IntPtr.Zero;
        }
        public static void InitRelayNetworkAccess() {
            return;
        }
        public static ESteamNetworkingAvailability GetRelayNetworkStatus(out SteamRelayNetworkStatus_t pDetails) {
            pDetails = default;
            return default;
        }
        public static float GetLocalPingLocation(out SteamNetworkPingLocation_t result) {
            result = default;
            return 0;
        }
        public static int EstimatePingTimeBetweenTwoLocations(ref SteamNetworkPingLocation_t location1, ref SteamNetworkPingLocation_t location2) {
            return 0;
        }
        public static int EstimatePingTimeFromLocalHost(ref SteamNetworkPingLocation_t remoteLocation) {
            return 0;
        }
        public static void ConvertPingLocationToString(ref SteamNetworkPingLocation_t location, out string pszBuf, int cchBufSize) {
            pszBuf = default;
            return;
        }
        public static bool ParsePingLocationString(string pszString, out SteamNetworkPingLocation_t result) {
            result = default;
            return false;
        }
        public static bool CheckPingDataUpToDate(float flMaxAgeSeconds) {
            return false;
        }
        public static int GetPingToDataCenter(SteamNetworkingPOPID popID, out SteamNetworkingPOPID pViaRelayPoP) {
            pViaRelayPoP = default;
            return 0;
        }
        public static int GetDirectPingToPOP(SteamNetworkingPOPID popID) {
            return 0;
        }
        public static int GetPOPCount() {
            return 0;
        }
        public static int GetPOPList(out SteamNetworkingPOPID list, int nListSz) {
            list = default;
            return 0;
        }
        public static SteamNetworkingMicroseconds GetLocalTimestamp() {
            return default;
        }
        public static void SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType eDetailLevel, FSteamNetworkingSocketsDebugOutput pfnFunc) {
            NativeGameNetworkingSockets.SetUserDebugOutputFunction(eDetailLevel, pfnFunc);
        }
        public static bool IsFakeIPv4(uint nIPv4) {
            return false;
        }
        public static ESteamNetworkingFakeIPType GetIPv4FakeIPType(uint nIPv4) {
            return default;
        }
        public static EResult GetRealIdentityForFakeIP(ref SteamNetworkingIPAddr fakeIP, out SteamNetworkingIdentity pOutRealIdentity) {
            pOutRealIdentity = default;
            return default;
        }
        public static bool SetConfigValue(ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, ESteamNetworkingConfigDataType eDataType, IntPtr pArg) {
            return false;
        }
        public static ESteamNetworkingGetConfigValueResult GetConfigValue(ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, out ESteamNetworkingConfigDataType pOutDataType, IntPtr pResult, ref ulong cbResult) {
            pOutDataType = default;
            return default;
        }
        public static string GetConfigValueInfo(ESteamNetworkingConfigValue eValue, out ESteamNetworkingConfigDataType pOutDataType, out ESteamNetworkingConfigScope pOutScope) {
            pOutDataType = default;
            pOutScope = default;
            return string.Empty;
        }
        public static ESteamNetworkingConfigValue IterateGenericEditableConfigValues(ESteamNetworkingConfigValue eCurrent, bool bEnumerateDevVars) {
            return default;
        }
        public static void SteamNetworkingIPAddr_ToString(ref SteamNetworkingIPAddr addr, out string buf, uint cbBuf, bool bWithPort) {
            buf = default;
            return;
        }
        public static bool SteamNetworkingIPAddr_ParseString(out SteamNetworkingIPAddr pAddr, string pszStr) {
            pAddr = default;
            return false;
        }
        public static ESteamNetworkingFakeIPType SteamNetworkingIPAddr_GetFakeIPType(ref SteamNetworkingIPAddr addr) {
            return default;
        }
        public static void SteamNetworkingIdentity_ToString(ref SteamNetworkingIdentity identity, out string buf, uint cbBuf) {
            buf = default;
            return;
        }
        public static bool SteamNetworkingIdentity_ParseString(out SteamNetworkingIdentity pIdentity, string pszStr) {
            pIdentity = default;
            return false;
        }
    }

    public static class SteamGameServerStats {
        public static SteamAPICall_t RequestUserStats(CSteamID steamIDUser) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out int pData) {
            pData = default;
            return false;
        }
        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out float pData) {
            pData = default;
            return false;
        }
        public static bool GetUserAchievement(CSteamID steamIDUser, string pchName, out bool pbAchieved) {
            pbAchieved = default;
            return false;
        }
        public static bool SetUserStat(CSteamID steamIDUser, string pchName, int nData) {
            return false;
        }
        public static bool SetUserStat(CSteamID steamIDUser, string pchName, float fData) {
            return false;
        }
        public static bool UpdateUserAvgRateStat(CSteamID steamIDUser, string pchName, float flCountThisSession, double dSessionLength) {
            return false;
        }
        public static bool SetUserAchievement(CSteamID steamIDUser, string pchName) {
            return false;
        }
        public static bool ClearUserAchievement(CSteamID steamIDUser, string pchName) {
            return false;
        }
        public static SteamAPICall_t StoreUserStats(CSteamID steamIDUser) {
            return SteamAPICall_t.Invalid;
        }
    }

    public static class SteamGameServerUGC {
        public static UGCQueryHandle_t CreateQueryUserUGCRequest(AccountID_t unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, AppId_t nCreatorAppID, AppId_t nConsumerAppID, uint unPage) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, AppId_t nCreatorAppID, AppId_t nConsumerAppID, uint unPage) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, AppId_t nCreatorAppID, AppId_t nConsumerAppID, string pchCursor = null) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryUGCDetailsRequest(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return UGCQueryHandle_t.Invalid;
        }
        public static SteamAPICall_t SendQueryUGCRequest(UGCQueryHandle_t handle) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetQueryUGCResult(UGCQueryHandle_t handle, uint index, out SteamUGCDetails_t pDetails) {
            pDetails = default;
            return false;
        }
        public static uint GetQueryUGCNumTags(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCTag(UGCQueryHandle_t handle, uint index, uint indexTag, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, uint index, uint indexTag, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, uint index, out string pchURL, uint cchURLSize) {
            pchURL = default;
            return false;
        }
        public static bool GetQueryUGCMetadata(UGCQueryHandle_t handle, uint index, out string pchMetadata, uint cchMetadatasize) {
            pchMetadata = default;
            return false;
        }
        public static bool GetQueryUGCChildren(UGCQueryHandle_t handle, uint index, PublishedFileId_t[] pvecPublishedFileID, uint cMaxEntries) {
            return false;
        }
        public static bool GetQueryUGCStatistic(UGCQueryHandle_t handle, uint index, EItemStatistic eStatType, out ulong pStatValue) {
            pStatValue = default;
            return false;
        }
        public static uint GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, uint index, uint previewIndex, out string pchURLOrVideoID, uint cchURLSize, out string pchOriginalFileName, uint cchOriginalFileNameSize, out EItemPreviewType pPreviewType) {
            pchURLOrVideoID = default;
            pchOriginalFileName = default;
            pPreviewType = default;
            return false;
        }
        public static uint GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex, out string pchKey, uint cchKeySize, out string pchValue, uint cchValueSize) {
            pchKey = default;
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, uint index, string pchKey, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static uint GetQueryUGCContentDescriptors(UGCQueryHandle_t handle, uint index, out EUGCContentDescriptorID pvecDescriptors, uint cMaxEntries) {
            pvecDescriptors = default;
            return 0;
        }
        public static bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) {
            return false;
        }
        public static bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) {
            return false;
        }
        public static bool AddRequiredTagGroup(UGCQueryHandle_t handle, System.Collections.Generic.IList<string> pTagGroups) {
            return false;
        }
        public static bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) {
            return false;
        }
        public static bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs) {
            return false;
        }
        public static bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags) {
            return false;
        }
        public static bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) {
            return false;
        }
        public static bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata) {
            return false;
        }
        public static bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren) {
            return false;
        }
        public static bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) {
            return false;
        }
        public static bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) {
            return false;
        }
        public static bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, uint unDays) {
            return false;
        }
        public static bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage) {
            return false;
        }
        public static bool SetAllowCachedResponse(UGCQueryHandle_t handle, uint unMaxAgeSeconds) {
            return false;
        }
        public static bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) {
            return false;
        }
        public static bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) {
            return false;
        }
        public static bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) {
            return false;
        }
        public static bool SetRankedByTrendDays(UGCQueryHandle_t handle, uint unDays) {
            return false;
        }
        public static bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, uint rtStart, uint rtEnd) {
            return false;
        }
        public static bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, uint rtStart, uint rtEnd) {
            return false;
        }
        public static bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue) {
            return false;
        }
        public static SteamAPICall_t RequestUGCDetails(PublishedFileId_t nPublishedFileID, uint unMaxAgeSeconds) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t CreateItem(AppId_t nConsumerAppId, EWorkshopFileType eFileType) {
            return SteamAPICall_t.Invalid;
        }
        public static UGCUpdateHandle_t StartItemUpdate(AppId_t nConsumerAppId, PublishedFileId_t nPublishedFileID) {
            return UGCUpdateHandle_t.Invalid;
        }
        public static bool SetItemTitle(UGCUpdateHandle_t handle, string pchTitle) {
            return false;
        }
        public static bool SetItemDescription(UGCUpdateHandle_t handle, string pchDescription) {
            return false;
        }
        public static bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage) {
            return false;
        }
        public static bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) {
            return false;
        }
        public static bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility) {
            return false;
        }
        public static bool SetItemTags(UGCUpdateHandle_t updateHandle, System.Collections.Generic.IList<string> pTags) {
            return false;
        }
        public static bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder) {
            return false;
        }
        public static bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile) {
            return false;
        }
        public static bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload) {
            return false;
        }
        public static bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle) {
            return false;
        }
        public static bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey) {
            return false;
        }
        public static bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue) {
            return false;
        }
        public static bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type) {
            return false;
        }
        public static bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID) {
            return false;
        }
        public static bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, uint index, string pszPreviewFile) {
            return false;
        }
        public static bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, uint index, string pszVideoID) {
            return false;
        }
        public static bool RemoveItemPreview(UGCUpdateHandle_t handle, uint index) {
            return false;
        }
        public static bool AddContentDescriptor(UGCUpdateHandle_t handle, EUGCContentDescriptorID descid) {
            return false;
        }
        public static bool RemoveContentDescriptor(UGCUpdateHandle_t handle, EUGCContentDescriptorID descid) {
            return false;
        }
        public static SteamAPICall_t SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote) {
            return SteamAPICall_t.Invalid;
        }
        public static EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, out ulong punBytesProcessed, out ulong punBytesTotal) {
            punBytesProcessed = default;
            punBytesTotal = default;
            return default;
        }
        public static SteamAPICall_t SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetUserItemVote(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddItemToFavorites(AppId_t nAppId, PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveItemFromFavorites(AppId_t nAppId, PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t SubscribeItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t UnsubscribeItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static uint GetNumSubscribedItems() {
            return 0;
        }
        public static uint GetSubscribedItems(PublishedFileId_t[] pvecPublishedFileID, uint cMaxEntries) {
            return 0;
        }
        public static uint GetItemState(PublishedFileId_t nPublishedFileID) {
            return 0;
        }
        public static bool GetItemInstallInfo(PublishedFileId_t nPublishedFileID, out ulong punSizeOnDisk, out string pchFolder, uint cchFolderSize, out uint punTimeStamp) {
            punSizeOnDisk = default;
            pchFolder = default;
            punTimeStamp = default;
            return false;
        }
        public static bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, out ulong punBytesDownloaded, out ulong punBytesTotal) {
            punBytesDownloaded = default;
            punBytesTotal = default;
            return false;
        }
        public static bool DownloadItem(PublishedFileId_t nPublishedFileID, bool bHighPriority) {
            return false;
        }
        public static bool BInitWorkshopForGameServer(DepotId_t unWorkshopDepotID, string pszFolder) {
            return false;
        }
        public static void SuspendDownloads(bool bSuspend) {
            return;
        }
        public static SteamAPICall_t StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t StopPlaytimeTrackingForAllItems() {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddAppDependency(PublishedFileId_t nPublishedFileID, AppId_t nAppID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveAppDependency(PublishedFileId_t nPublishedFileID, AppId_t nAppID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetAppDependencies(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t DeleteItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool ShowWorkshopEULA() {
            return false;
        }
        public static SteamAPICall_t GetWorkshopEULAStatus() {
            return SteamAPICall_t.Invalid;
        }
    }

    public static class SteamGameServerUtils {
        public static uint GetSecondsSinceAppActive() {
            return 0;
        }
        public static uint GetSecondsSinceComputerActive() {
            return 0;
        }
        public static EUniverse GetConnectedUniverse() {
            return default;
        }
        public static uint GetServerRealTime() {
            return (uint)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static string GetIPCountry() {
            return string.Empty;
        }
        public static bool GetImageSize(int iImage, out uint pnWidth, out uint pnHeight) {
            pnWidth = default;
            pnHeight = default;
            return false;
        }
        public static bool GetImageRGBA(int iImage, byte[] pubDest, int nDestBufferSize) {
            return false;
        }
        public static byte GetCurrentBatteryPower() {
            return 0;
        }
        public static AppId_t GetAppID() {
            return new AppId_t(304930);
        }
        public static void SetOverlayNotificationPosition(ENotificationPosition eNotificationPosition) {
            return;
        }
        public static bool IsAPICallCompleted(SteamAPICall_t hSteamAPICall, out bool pbFailed) {
            pbFailed = default;
            return false;
        }
        public static ESteamAPICallFailure GetAPICallFailureReason(SteamAPICall_t hSteamAPICall) {
            return default;
        }
        public static bool GetAPICallResult(SteamAPICall_t hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, out bool pbFailed) {
            pbFailed = default;
            return false;
        }
        public static uint GetIPCCallCount() {
            return 0;
        }
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) {
            return;
        }
        public static bool IsOverlayEnabled() {
            return false;
        }
        public static bool BOverlayNeedsPresent() {
            return false;
        }
        public static SteamAPICall_t CheckFileSignature(string szFileName) {
            return SteamAPICall_t.Invalid;
        }
        public static bool ShowGamepadTextInput(EGamepadTextInputMode eInputMode, EGamepadTextInputLineMode eLineInputMode, string pchDescription, uint unCharMax, string pchExistingText) {
            return false;
        }
        public static uint GetEnteredGamepadTextLength() {
            return 0;
        }
        public static bool GetEnteredGamepadTextInput(out string pchText, uint cchText) {
            pchText = default;
            return false;
        }
        public static string GetSteamUILanguage() {
            return string.Empty;
        }
        public static bool IsSteamRunningInVR() {
            return false;
        }
        public static void SetOverlayNotificationInset(int nHorizontalInset, int nVerticalInset) {
            return;
        }
        public static bool IsSteamInBigPictureMode() {
            return false;
        }
        public static void StartVRDashboard() {
            return;
        }
        public static bool IsVRHeadsetStreamingEnabled() {
            return false;
        }
        public static void SetVRHeadsetStreamingEnabled(bool bEnabled) {
            return;
        }
        public static bool IsSteamChinaLauncher() {
            return false;
        }
        public static bool InitFilterText(uint unFilterOptions = 0) {
            return false;
        }
        public static int FilterText(ETextFilteringContext eContext, CSteamID sourceSteamID, string pchInputMessage, out string pchOutFilteredText, uint nByteSizeOutFilteredText) {
            pchOutFilteredText = default;
            return 0;
        }
        public static ESteamIPv6ConnectivityState GetIPv6ConnectivityState(ESteamIPv6ConnectivityProtocol eProtocol) {
            return default;
        }
        public static bool IsSteamRunningOnSteamDeck() {
            return false;
        }
        public static bool ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode eKeyboardMode, int nTextFieldXPosition, int nTextFieldYPosition, int nTextFieldWidth, int nTextFieldHeight) {
            return false;
        }
        public static void SetGameLauncherMode(bool bLauncherMode) {
            return;
        }
        public static bool DismissFloatingGamepadTextInput() {
            return false;
        }
    }

    public static class SteamHTMLSurface {
        public static bool Init() {
            return false;
        }
        public static bool Shutdown() {
            return false;
        }
        public static SteamAPICall_t CreateBrowser(string pchUserAgent, string pchUserCSS) {
            return SteamAPICall_t.Invalid;
        }
        public static void RemoveBrowser(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void LoadURL(HHTMLBrowser unBrowserHandle, string pchURL, string pchPostData) {
            return;
        }
        public static void SetSize(HHTMLBrowser unBrowserHandle, uint unWidth, uint unHeight) {
            return;
        }
        public static void StopLoad(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void Reload(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void GoBack(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void GoForward(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void AddHeader(HHTMLBrowser unBrowserHandle, string pchKey, string pchValue) {
            return;
        }
        public static void ExecuteJavascript(HHTMLBrowser unBrowserHandle, string pchScript) {
            return;
        }
        public static void MouseUp(HHTMLBrowser unBrowserHandle, EHTMLMouseButton eMouseButton) {
            return;
        }
        public static void MouseDown(HHTMLBrowser unBrowserHandle, EHTMLMouseButton eMouseButton) {
            return;
        }
        public static void MouseDoubleClick(HHTMLBrowser unBrowserHandle, EHTMLMouseButton eMouseButton) {
            return;
        }
        public static void MouseMove(HHTMLBrowser unBrowserHandle, int x, int y) {
            return;
        }
        public static void MouseWheel(HHTMLBrowser unBrowserHandle, int nDelta) {
            return;
        }
        public static void KeyDown(HHTMLBrowser unBrowserHandle, uint nNativeKeyCode, EHTMLKeyModifiers eHTMLKeyModifiers, bool bIsSystemKey = false) {
            return;
        }
        public static void KeyUp(HHTMLBrowser unBrowserHandle, uint nNativeKeyCode, EHTMLKeyModifiers eHTMLKeyModifiers) {
            return;
        }
        public static void KeyChar(HHTMLBrowser unBrowserHandle, uint cUnicodeChar, EHTMLKeyModifiers eHTMLKeyModifiers) {
            return;
        }
        public static void SetHorizontalScroll(HHTMLBrowser unBrowserHandle, uint nAbsolutePixelScroll) {
            return;
        }
        public static void SetVerticalScroll(HHTMLBrowser unBrowserHandle, uint nAbsolutePixelScroll) {
            return;
        }
        public static void SetKeyFocus(HHTMLBrowser unBrowserHandle, bool bHasKeyFocus) {
            return;
        }
        public static void ViewSource(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void CopyToClipboard(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void PasteFromClipboard(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void Find(HHTMLBrowser unBrowserHandle, string pchSearchStr, bool bCurrentlyInFind, bool bReverse) {
            return;
        }
        public static void StopFind(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void GetLinkAtPosition(HHTMLBrowser unBrowserHandle, int x, int y) {
            return;
        }
        public static void SetCookie(string pchHostname, string pchKey, string pchValue, string pchPath = "/", uint nExpires = 0, bool bSecure = false, bool bHTTPOnly = false) {
            return;
        }
        public static void SetPageScaleFactor(HHTMLBrowser unBrowserHandle, float flZoom, int nPointX, int nPointY) {
            return;
        }
        public static void SetBackgroundMode(HHTMLBrowser unBrowserHandle, bool bBackgroundMode) {
            return;
        }
        public static void SetDPIScalingFactor(HHTMLBrowser unBrowserHandle, float flDPIScaling) {
            return;
        }
        public static void OpenDeveloperTools(HHTMLBrowser unBrowserHandle) {
            return;
        }
        public static void AllowStartRequest(HHTMLBrowser unBrowserHandle, bool bAllowed) {
            return;
        }
        public static void JSDialogResponse(HHTMLBrowser unBrowserHandle, bool bResult) {
            return;
        }
        public static void FileLoadDialogResponse(HHTMLBrowser unBrowserHandle, IntPtr pchSelectedFiles) {
            return;
        }
    }

    public static class SteamHTTP {
        public static HTTPRequestHandle CreateHTTPRequest(EHTTPMethod eHTTPRequestMethod, string pchAbsoluteURL) {
            return default;
        }
        public static bool SetHTTPRequestContextValue(HTTPRequestHandle hRequest, ulong ulContextValue) {
            return false;
        }
        public static bool SetHTTPRequestNetworkActivityTimeout(HTTPRequestHandle hRequest, uint unTimeoutSeconds) {
            return false;
        }
        public static bool SetHTTPRequestHeaderValue(HTTPRequestHandle hRequest, string pchHeaderName, string pchHeaderValue) {
            return false;
        }
        public static bool SetHTTPRequestGetOrPostParameter(HTTPRequestHandle hRequest, string pchParamName, string pchParamValue) {
            return false;
        }
        public static bool SendHTTPRequest(HTTPRequestHandle hRequest, out SteamAPICall_t pCallHandle) {
            pCallHandle = default;
            return false;
        }
        public static bool SendHTTPRequestAndStreamResponse(HTTPRequestHandle hRequest, out SteamAPICall_t pCallHandle) {
            pCallHandle = default;
            return false;
        }
        public static bool DeferHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool PrioritizeHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool GetHTTPResponseHeaderSize(HTTPRequestHandle hRequest, string pchHeaderName, out uint unResponseHeaderSize) {
            unResponseHeaderSize = default;
            return false;
        }
        public static bool GetHTTPResponseHeaderValue(HTTPRequestHandle hRequest, string pchHeaderName, byte[] pHeaderValueBuffer, uint unBufferSize) {
            return false;
        }
        public static bool GetHTTPResponseBodySize(HTTPRequestHandle hRequest, out uint unBodySize) {
            unBodySize = default;
            return false;
        }
        public static bool GetHTTPResponseBodyData(HTTPRequestHandle hRequest, byte[] pBodyDataBuffer, uint unBufferSize) {
            return false;
        }
        public static bool GetHTTPStreamingResponseBodyData(HTTPRequestHandle hRequest, uint cOffset, byte[] pBodyDataBuffer, uint unBufferSize) {
            return false;
        }
        public static bool ReleaseHTTPRequest(HTTPRequestHandle hRequest) {
            return false;
        }
        public static bool GetHTTPDownloadProgressPct(HTTPRequestHandle hRequest, out float pflPercentOut) {
            pflPercentOut = default;
            return false;
        }
        public static bool SetHTTPRequestRawPostBody(HTTPRequestHandle hRequest, string pchContentType, byte[] pubBody, uint unBodyLen) {
            return false;
        }
        public static HTTPCookieContainerHandle CreateCookieContainer(bool bAllowResponsesToModify) {
            return default;
        }
        public static bool ReleaseCookieContainer(HTTPCookieContainerHandle hCookieContainer) {
            return false;
        }
        public static bool SetCookie(HTTPCookieContainerHandle hCookieContainer, string pchHost, string pchUrl, string pchCookie) {
            return false;
        }
        public static bool SetHTTPRequestCookieContainer(HTTPRequestHandle hRequest, HTTPCookieContainerHandle hCookieContainer) {
            return false;
        }
        public static bool SetHTTPRequestUserAgentInfo(HTTPRequestHandle hRequest, string pchUserAgentInfo) {
            return false;
        }
        public static bool SetHTTPRequestRequiresVerifiedCertificate(HTTPRequestHandle hRequest, bool bRequireVerifiedCertificate) {
            return false;
        }
        public static bool SetHTTPRequestAbsoluteTimeoutMS(HTTPRequestHandle hRequest, uint unMilliseconds) {
            return false;
        }
        public static bool GetHTTPRequestWasTimedOut(HTTPRequestHandle hRequest, out bool pbWasTimedOut) {
            pbWasTimedOut = default;
            return false;
        }
    }

    public static class SteamInput {
        public static bool Init(bool bExplicitlyCallRunFrame) {
            return false;
        }
        public static bool Shutdown() {
            return false;
        }
        public static bool SetInputActionManifestFilePath(string pchInputActionManifestAbsolutePath) {
            return false;
        }
        public static void RunFrame(bool bReservedValue = true) {
            return;
        }
        public static bool BWaitForData(bool bWaitForever, uint unTimeout) {
            return false;
        }
        public static bool BNewDataAvailable() {
            return false;
        }
        public static int GetConnectedControllers(InputHandle_t[] handlesOut) {
            return 0;
        }
        public static void EnableDeviceCallbacks() {
            return;
        }
        public static void EnableActionEventCallbacks(SteamInputActionEventCallbackPointer pCallback) {
            return;
        }
        public static InputActionSetHandle_t GetActionSetHandle(string pszActionSetName) {
            return default;
        }
        public static void ActivateActionSet(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle) {
            return;
        }
        public static InputActionSetHandle_t GetCurrentActionSet(InputHandle_t inputHandle) {
            return default;
        }
        public static void ActivateActionSetLayer(InputHandle_t inputHandle, InputActionSetHandle_t actionSetLayerHandle) {
            return;
        }
        public static void DeactivateActionSetLayer(InputHandle_t inputHandle, InputActionSetHandle_t actionSetLayerHandle) {
            return;
        }
        public static void DeactivateAllActionSetLayers(InputHandle_t inputHandle) {
            return;
        }
        public static int GetActiveActionSetLayers(InputHandle_t inputHandle, InputActionSetHandle_t[] handlesOut) {
            return 0;
        }
        public static InputDigitalActionHandle_t GetDigitalActionHandle(string pszActionName) {
            return default;
        }
        public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t inputHandle, InputDigitalActionHandle_t digitalActionHandle) {
            return default;
        }
        public static int GetDigitalActionOrigins(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle, InputDigitalActionHandle_t digitalActionHandle, EInputActionOrigin[] originsOut) {
            return 0;
        }
        public static string GetStringForDigitalActionName(InputDigitalActionHandle_t eActionHandle) {
            return string.Empty;
        }
        public static InputAnalogActionHandle_t GetAnalogActionHandle(string pszActionName) {
            return default;
        }
        public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t inputHandle, InputAnalogActionHandle_t analogActionHandle) {
            return default;
        }
        public static int GetAnalogActionOrigins(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle, InputAnalogActionHandle_t analogActionHandle, EInputActionOrigin[] originsOut) {
            return 0;
        }
        public static string GetGlyphPNGForActionOrigin(EInputActionOrigin eOrigin, ESteamInputGlyphSize eSize, uint unFlags) {
            return string.Empty;
        }
        public static string GetGlyphSVGForActionOrigin(EInputActionOrigin eOrigin, uint unFlags) {
            return string.Empty;
        }
        public static string GetGlyphForActionOrigin_Legacy(EInputActionOrigin eOrigin) {
            return string.Empty;
        }
        public static string GetStringForActionOrigin(EInputActionOrigin eOrigin) {
            return string.Empty;
        }
        public static string GetStringForAnalogActionName(InputAnalogActionHandle_t eActionHandle) {
            return string.Empty;
        }
        public static void StopAnalogActionMomentum(InputHandle_t inputHandle, InputAnalogActionHandle_t eAction) {
            return;
        }
        public static InputMotionData_t GetMotionData(InputHandle_t inputHandle) {
            return default;
        }
        public static void TriggerVibration(InputHandle_t inputHandle, ushort usLeftSpeed, ushort usRightSpeed) {
            return;
        }
        public static void TriggerVibrationExtended(InputHandle_t inputHandle, ushort usLeftSpeed, ushort usRightSpeed, ushort usLeftTriggerSpeed, ushort usRightTriggerSpeed) {
            return;
        }
        public static void TriggerSimpleHapticEvent(InputHandle_t inputHandle, EControllerHapticLocation eHapticLocation, byte nIntensity, char nGainDB, byte nOtherIntensity, char nOtherGainDB) {
            return;
        }
        public static void SetLEDColor(InputHandle_t inputHandle, byte nColorR, byte nColorG, byte nColorB, uint nFlags) {
            return;
        }
        public static void Legacy_TriggerHapticPulse(InputHandle_t inputHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec) {
            return;
        }
        public static void Legacy_TriggerRepeatedHapticPulse(InputHandle_t inputHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec, ushort usOffMicroSec, ushort unRepeat, uint nFlags) {
            return;
        }
        public static bool ShowBindingPanel(InputHandle_t inputHandle) {
            return false;
        }
        public static ESteamInputType GetInputTypeForHandle(InputHandle_t inputHandle) {
            return default;
        }
        public static InputHandle_t GetControllerForGamepadIndex(int nIndex) {
            return default;
        }
        public static int GetGamepadIndexForController(InputHandle_t ulinputHandle) {
            return 0;
        }
        public static string GetStringForXboxOrigin(EXboxOrigin eOrigin) {
            return string.Empty;
        }
        public static string GetGlyphForXboxOrigin(EXboxOrigin eOrigin) {
            return string.Empty;
        }
        public static EInputActionOrigin GetActionOriginFromXboxOrigin(InputHandle_t inputHandle, EXboxOrigin eOrigin) {
            return default;
        }
        public static EInputActionOrigin TranslateActionOrigin(ESteamInputType eDestinationInputType, EInputActionOrigin eSourceOrigin) {
            return default;
        }
        public static bool GetDeviceBindingRevision(InputHandle_t inputHandle, out int pMajor, out int pMinor) {
            pMajor = default;
            pMinor = default;
            return false;
        }
        public static uint GetRemotePlaySessionID(InputHandle_t inputHandle) {
            return 0;
        }
        public static ushort GetSessionInputConfigurationSettings() {
            return 0;
        }
        public static void SetDualSenseTriggerEffect(InputHandle_t inputHandle, IntPtr pParam) {
            return;
        }
    }

    public static class SteamInventory {
        public static EResult GetResultStatus(SteamInventoryResult_t resultHandle) {
            return default;
        }
        public static bool GetResultItems(SteamInventoryResult_t resultHandle, SteamItemDetails_t[] pOutItemsArray, ref uint punOutItemsArraySize) {
            return false;
        }
        public static bool GetResultItemProperty(SteamInventoryResult_t resultHandle, uint unItemIndex, string pchPropertyName, out string pchValueBuffer, ref uint punValueBufferSizeOut) {
            pchValueBuffer = default;
            return false;
        }
        public static uint GetResultTimestamp(SteamInventoryResult_t resultHandle) {
            return 0;
        }
        public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, CSteamID steamIDExpected) {
            return false;
        }
        public static void DestroyResult(SteamInventoryResult_t resultHandle) {
            return;
        }
        public static bool GetAllItems(out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool GetItemsByID(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t[] pInstanceIDs, uint unCountInstanceIDs) {
            pResultHandle = default;
            return false;
        }
        public static bool SerializeResult(SteamInventoryResult_t resultHandle, byte[] pOutBuffer, out uint punOutBufferSize) {
            punOutBufferSize = default;
            return false;
        }
        public static bool DeserializeResult(out SteamInventoryResult_t pOutResultHandle, byte[] pBuffer, uint unBufferSize, bool bRESERVED_MUST_BE_FALSE = false) {
            pOutResultHandle = default;
            return false;
        }
        public static bool GenerateItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayItemDefs, uint[] punArrayQuantity, uint unArrayLength) {
            pResultHandle = default;
            return false;
        }
        public static bool GrantPromoItems(out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool AddPromoItem(out SteamInventoryResult_t pResultHandle, SteamItemDef_t itemDef) {
            pResultHandle = default;
            return false;
        }
        public static bool AddPromoItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayItemDefs, uint unArrayLength) {
            pResultHandle = default;
            return false;
        }
        public static bool ConsumeItem(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t itemConsume, uint unQuantity) {
            pResultHandle = default;
            return false;
        }
        public static bool ExchangeItems(out SteamInventoryResult_t pResultHandle, SteamItemDef_t[] pArrayGenerate, uint[] punArrayGenerateQuantity, uint unArrayGenerateLength, SteamItemInstanceID_t[] pArrayDestroy, uint[] punArrayDestroyQuantity, uint unArrayDestroyLength) {
            pResultHandle = default;
            return false;
        }
        public static bool TransferItemQuantity(out SteamInventoryResult_t pResultHandle, SteamItemInstanceID_t itemIdSource, uint unQuantity, SteamItemInstanceID_t itemIdDest) {
            pResultHandle = default;
            return false;
        }
        public static void SendItemDropHeartbeat() {
            return;
        }
        public static bool TriggerItemDrop(out SteamInventoryResult_t pResultHandle, SteamItemDef_t dropListDefinition) {
            pResultHandle = default;
            return false;
        }
        public static bool TradeItems(out SteamInventoryResult_t pResultHandle, CSteamID steamIDTradePartner, SteamItemInstanceID_t[] pArrayGive, uint[] pArrayGiveQuantity, uint nArrayGiveLength, SteamItemInstanceID_t[] pArrayGet, uint[] pArrayGetQuantity, uint nArrayGetLength) {
            pResultHandle = default;
            return false;
        }
        public static bool LoadItemDefinitions() {
            return false;
        }
        public static bool GetItemDefinitionIDs(SteamItemDef_t[] pItemDefIDs, ref uint punItemDefIDsArraySize) {
            return false;
        }
        public static bool GetItemDefinitionProperty(SteamItemDef_t iDefinition, string pchPropertyName, out string pchValueBuffer, ref uint punValueBufferSizeOut) {
            pchValueBuffer = default;
            return false;
        }
        public static SteamAPICall_t RequestEligiblePromoItemDefinitionsIDs(CSteamID steamID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetEligiblePromoItemDefinitionIDs(CSteamID steamID, SteamItemDef_t[] pItemDefIDs, ref uint punItemDefIDsArraySize) {
            return false;
        }
        public static SteamAPICall_t StartPurchase(SteamItemDef_t[] pArrayItemDefs, uint[] punArrayQuantity, uint unArrayLength) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RequestPrices() {
            return SteamAPICall_t.Invalid;
        }
        public static uint GetNumItemsWithPrices() {
            return 0;
        }
        public static bool GetItemsWithPrices(SteamItemDef_t[] pArrayItemDefs, ulong[] pCurrentPrices, ulong[] pBasePrices, uint unArrayLength) {
            return false;
        }
        public static bool GetItemPrice(SteamItemDef_t iDefinition, out ulong pCurrentPrice, out ulong pBasePrice) {
            pCurrentPrice = default;
            pBasePrice = default;
            return false;
        }
        public static SteamInventoryUpdateHandle_t StartUpdateProperties() {
            return SteamInventoryUpdateHandle_t.Invalid;
        }
        public static bool RemoveProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, string pchPropertyValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, bool bValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, long nValue) {
            return false;
        }
        public static bool SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t nItemID, string pchPropertyName, float flValue) {
            return false;
        }
        public static bool SubmitUpdateProperties(SteamInventoryUpdateHandle_t handle, out SteamInventoryResult_t pResultHandle) {
            pResultHandle = default;
            return false;
        }
        public static bool InspectItem(out SteamInventoryResult_t pResultHandle, string pchItemToken) {
            pResultHandle = default;
            return false;
        }
    }

    public static class SteamMatchmaking {
        public static int GetFavoriteGameCount() {
            return SteamFavoritesManager.GetCount();
        }
        public static bool GetFavoriteGame(int iGame, out AppId_t pnAppID, out uint pnIP, out ushort pnConnPort, out ushort pnQueryPort, out uint punFlags, out uint pRTime32LastPlayedOnServer) {
            return SteamFavoritesManager.GetFavoriteGame(iGame, out pnAppID, out pnIP, out pnConnPort, out pnQueryPort, out punFlags, out pRTime32LastPlayedOnServer);
        }
        public static int AddFavoriteGame(AppId_t nAppID, uint nIP, ushort nConnPort, ushort nQueryPort, uint unFlags, uint rTime32LastPlayedOnServer) {
            return SteamFavoritesManager.AddFavoriteGame(nAppID, nIP, nConnPort, nQueryPort, unFlags, rTime32LastPlayedOnServer);
        }
        public static bool RemoveFavoriteGame(AppId_t nAppID, uint nIP, ushort nConnPort, ushort nQueryPort, uint unFlags) {
            return SteamFavoritesManager.RemoveFavoriteGame(nAppID, nIP, nConnPort, nQueryPort, unFlags);
        }
        public static SteamAPICall_t RequestLobbyList() {
            return SteamAPICall_t.Invalid;
        }
        public static void AddRequestLobbyListStringFilter(string pchKeyToMatch, string pchValueToMatch, ELobbyComparison eComparisonType) {
            return;
        }
        public static void AddRequestLobbyListNumericalFilter(string pchKeyToMatch, int nValueToMatch, ELobbyComparison eComparisonType) {
            return;
        }
        public static void AddRequestLobbyListNearValueFilter(string pchKeyToMatch, int nValueToBeCloseTo) {
            return;
        }
        public static void AddRequestLobbyListFilterSlotsAvailable(int nSlotsAvailable) {
            return;
        }
        public static void AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter eLobbyDistanceFilter) {
            return;
        }
        public static void AddRequestLobbyListResultCountFilter(int cMaxResults) {
            return;
        }
        public static void AddRequestLobbyListCompatibleMembersFilter(CSteamID steamIDLobby) {
            return;
        }
        public static CSteamID GetLobbyByIndex(int iLobby) {
            return new CSteamID(76561198000000000UL);
        }
        public static SteamAPICall_t CreateLobby(ELobbyType eLobbyType, int cMaxMembers) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t JoinLobby(CSteamID steamIDLobby) {
            return SteamAPICall_t.Invalid;
        }
        public static void LeaveLobby(CSteamID steamIDLobby) {
            return;
        }
        public static bool InviteUserToLobby(CSteamID steamIDLobby, CSteamID steamIDInvitee) {
            return false;
        }
        public static int GetNumLobbyMembers(CSteamID steamIDLobby) {
            return 0;
        }
        public static CSteamID GetLobbyMemberByIndex(CSteamID steamIDLobby, int iMember) {
            return new CSteamID(76561198000000000UL);
        }
        public static string GetLobbyData(CSteamID steamIDLobby, string pchKey) {
            return string.Empty;
        }
        public static bool SetLobbyData(CSteamID steamIDLobby, string pchKey, string pchValue) {
            return false;
        }
        public static int GetLobbyDataCount(CSteamID steamIDLobby) {
            return 0;
        }
        public static bool GetLobbyDataByIndex(CSteamID steamIDLobby, int iLobbyData, out string pchKey, int cchKeyBufferSize, out string pchValue, int cchValueBufferSize) {
            pchKey = default;
            pchValue = default;
            return false;
        }
        public static bool DeleteLobbyData(CSteamID steamIDLobby, string pchKey) {
            return false;
        }
        public static string GetLobbyMemberData(CSteamID steamIDLobby, CSteamID steamIDUser, string pchKey) {
            return string.Empty;
        }
        public static void SetLobbyMemberData(CSteamID steamIDLobby, string pchKey, string pchValue) {
            return;
        }
        public static bool SendLobbyChatMsg(CSteamID steamIDLobby, byte[] pvMsgBody, int cubMsgBody) {
            return false;
        }
        public static int GetLobbyChatEntry(CSteamID steamIDLobby, int iChatID, out CSteamID pSteamIDUser, byte[] pvData, int cubData, out EChatEntryType peChatEntryType) {
            pSteamIDUser = default;
            peChatEntryType = default;
            return 0;
        }
        public static bool RequestLobbyData(CSteamID steamIDLobby) {
            return false;
        }
        public static void SetLobbyGameServer(CSteamID steamIDLobby, uint unGameServerIP, ushort unGameServerPort, CSteamID steamIDGameServer) {
            return;
        }
        public static bool GetLobbyGameServer(CSteamID steamIDLobby, out uint punGameServerIP, out ushort punGameServerPort, out CSteamID psteamIDGameServer) {
            punGameServerIP = default;
            punGameServerPort = default;
            psteamIDGameServer = default;
            return false;
        }
        public static bool SetLobbyMemberLimit(CSteamID steamIDLobby, int cMaxMembers) {
            return false;
        }
        public static int GetLobbyMemberLimit(CSteamID steamIDLobby) {
            return 0;
        }
        public static bool SetLobbyType(CSteamID steamIDLobby, ELobbyType eLobbyType) {
            return false;
        }
        public static bool SetLobbyJoinable(CSteamID steamIDLobby, bool bLobbyJoinable) {
            return false;
        }
        public static CSteamID GetLobbyOwner(CSteamID steamIDLobby) {
            return new CSteamID(76561198000000000UL);
        }
        public static bool SetLobbyOwner(CSteamID steamIDLobby, CSteamID steamIDNewOwner) {
            return false;
        }
        public static bool SetLinkedLobby(CSteamID steamIDLobby, CSteamID steamIDLobbyDependent) {
            return false;
        }
        public static void CheckForPSNGameBootInvite(uint iGameBootAttributes) {
            return;
        }
    }

    public static class SteamMatchmakingServers {
        public static HServerListRequest RequestInternetServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return SteamMasterServerQuery.RequestInternetServerList(iApp, ppchFilters, nFilters, pRequestServersResponse);
        }
        public static HServerListRequest RequestLANServerList(AppId_t iApp, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return SteamMasterServerQuery.RequestLANServerList(iApp, pRequestServersResponse);
        }
        public static HServerListRequest RequestFriendsServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return SteamMasterServerQuery.RequestInternetServerList(iApp, ppchFilters, nFilters, pRequestServersResponse);
        }
        public static HServerListRequest RequestFavoritesServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return SteamMasterServerQuery.RequestFavoritesServerList(iApp, ppchFilters, nFilters, pRequestServersResponse);
        }
        public static HServerListRequest RequestHistoryServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return SteamMasterServerQuery.RequestInternetServerList(iApp, ppchFilters, nFilters, pRequestServersResponse);
        }
        public static HServerListRequest RequestSpectatorServerList(AppId_t iApp, MatchMakingKeyValuePair_t[] ppchFilters, uint nFilters, ISteamMatchmakingServerListResponse pRequestServersResponse) {
            return default;
        }
        public static void ReleaseRequest(HServerListRequest hServerListRequest) {
            SteamMasterServerQuery.ReleaseRequest(hServerListRequest);
        }
        public static gameserveritem_t GetServerDetails(HServerListRequest hRequest, int iServer) {
            return SteamMasterServerQuery.GetServerDetails(hRequest, iServer);
        }
        public static void CancelQuery(HServerListRequest hRequest) {
            SteamMasterServerQuery.CancelQuery(hRequest);
        }
        public static void RefreshQuery(HServerListRequest hRequest) {
            return;
        }
        public static bool IsRefreshing(HServerListRequest hRequest) {
            return SteamMasterServerQuery.IsRefreshing(hRequest);
        }
        public static int GetServerCount(HServerListRequest hRequest) {
            return 0;
        }
        public static void RefreshServer(HServerListRequest hRequest, int iServer) {
            return;
        }
        public static HServerQuery PingServer(uint unIP, ushort usPort, ISteamMatchmakingPingResponse pRequestServersResponse) {
            return A2SPingQuery.PingServer(unIP, usPort, pRequestServersResponse);
        }
        public static HServerQuery PlayerDetails(uint unIP, ushort usPort, ISteamMatchmakingPlayersResponse pRequestServersResponse) {
            return default;
        }
        public static HServerQuery ServerRules(uint unIP, ushort usPort, ISteamMatchmakingRulesResponse pRequestServersResponse) {
            return default;
        }
        public static void CancelServerQuery(HServerQuery hServerQuery) {
            return;
        }
    }

    public static class SteamGameSearch {
        public static EGameSearchErrorCode_t AddGameSearchParams(string pchKeyToFind, string pchValuesToFind) {
            return default;
        }
        public static EGameSearchErrorCode_t SearchForGameWithLobby(CSteamID steamIDLobby, int nPlayerMin, int nPlayerMax) {
            return default;
        }
        public static EGameSearchErrorCode_t SearchForGameSolo(int nPlayerMin, int nPlayerMax) {
            return default;
        }
        public static EGameSearchErrorCode_t AcceptGame() {
            return default;
        }
        public static EGameSearchErrorCode_t DeclineGame() {
            return default;
        }
        public static EGameSearchErrorCode_t RetrieveConnectionDetails(CSteamID steamIDHost, out string pchConnectionDetails, int cubConnectionDetails) {
            pchConnectionDetails = default;
            return default;
        }
        public static EGameSearchErrorCode_t EndGameSearch() {
            return default;
        }
        public static EGameSearchErrorCode_t SetGameHostParams(string pchKey, string pchValue) {
            return default;
        }
        public static EGameSearchErrorCode_t SetConnectionDetails(string pchConnectionDetails, int cubConnectionDetails) {
            return default;
        }
        public static EGameSearchErrorCode_t RequestPlayersForGame(int nPlayerMin, int nPlayerMax, int nMaxTeamSize) {
            return default;
        }
        public static EGameSearchErrorCode_t HostConfirmGameStart(ulong ullUniqueGameID) {
            return default;
        }
        public static EGameSearchErrorCode_t CancelRequestPlayersForGame() {
            return default;
        }
        public static EGameSearchErrorCode_t SubmitPlayerResult(ulong ullUniqueGameID, CSteamID steamIDPlayer, EPlayerResult_t EPlayerResult) {
            return default;
        }
        public static EGameSearchErrorCode_t EndGame(ulong ullUniqueGameID) {
            return default;
        }
    }

    public static class SteamParties {
        public static uint GetNumActiveBeacons() {
            return 0;
        }
        public static PartyBeaconID_t GetBeaconByIndex(uint unIndex) {
            return default;
        }
        public static bool GetBeaconDetails(PartyBeaconID_t ulBeaconID, out CSteamID pSteamIDBeaconOwner, out SteamPartyBeaconLocation_t pLocation, out string pchMetadata, int cchMetadata) {
            pSteamIDBeaconOwner = default;
            pLocation = default;
            pchMetadata = default;
            return false;
        }
        public static SteamAPICall_t JoinParty(PartyBeaconID_t ulBeaconID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetNumAvailableBeaconLocations(out uint puNumLocations) {
            puNumLocations = default;
            return false;
        }
        public static bool GetAvailableBeaconLocations(SteamPartyBeaconLocation_t[] pLocationList, uint uMaxNumLocations) {
            return false;
        }
        public static SteamAPICall_t CreateBeacon(uint unOpenSlots, ref SteamPartyBeaconLocation_t pBeaconLocation, string pchConnectString, string pchMetadata) {
            return SteamAPICall_t.Invalid;
        }
        public static void OnReservationCompleted(PartyBeaconID_t ulBeacon, CSteamID steamIDUser) {
            return;
        }
        public static void CancelReservation(PartyBeaconID_t ulBeacon, CSteamID steamIDUser) {
            return;
        }
        public static SteamAPICall_t ChangeNumOpenSlots(PartyBeaconID_t ulBeacon, uint unOpenSlots) {
            return SteamAPICall_t.Invalid;
        }
        public static bool DestroyBeacon(PartyBeaconID_t ulBeacon) {
            return false;
        }
        public static bool GetBeaconLocationData(SteamPartyBeaconLocation_t BeaconLocation, ESteamPartyBeaconLocationData eData, out string pchDataStringOut, int cchDataStringOut) {
            pchDataStringOut = default;
            return false;
        }
    }

    public static class SteamMusic {
        public static bool BIsEnabled() {
            return false;
        }
        public static bool BIsPlaying() {
            return false;
        }
        public static AudioPlayback_Status GetPlaybackStatus() {
            return default;
        }
        public static void Play() {
            return;
        }
        public static void Pause() {
            return;
        }
        public static void PlayPrevious() {
            return;
        }
        public static void PlayNext() {
            return;
        }
        public static void SetVolume(float flVolume) {
            return;
        }
        public static float GetVolume() {
            return 0;
        }
    }

    public static class SteamMusicRemote {
        public static bool RegisterSteamMusicRemote(string pchName) {
            return false;
        }
        public static bool DeregisterSteamMusicRemote() {
            return false;
        }
        public static bool BIsCurrentMusicRemote() {
            return false;
        }
        public static bool BActivationSuccess(bool bValue) {
            return false;
        }
        public static bool SetDisplayName(string pchDisplayName) {
            return false;
        }
        public static bool SetPNGIcon_64x64(byte[] pvBuffer, uint cbBufferLength) {
            return false;
        }
        public static bool EnablePlayPrevious(bool bValue) {
            return false;
        }
        public static bool EnablePlayNext(bool bValue) {
            return false;
        }
        public static bool EnableShuffled(bool bValue) {
            return false;
        }
        public static bool EnableLooped(bool bValue) {
            return false;
        }
        public static bool EnableQueue(bool bValue) {
            return false;
        }
        public static bool EnablePlaylists(bool bValue) {
            return false;
        }
        public static bool UpdatePlaybackStatus(AudioPlayback_Status nStatus) {
            return false;
        }
        public static bool UpdateShuffled(bool bValue) {
            return false;
        }
        public static bool UpdateLooped(bool bValue) {
            return false;
        }
        public static bool UpdateVolume(float flValue) {
            return false;
        }
        public static bool CurrentEntryWillChange() {
            return false;
        }
        public static bool CurrentEntryIsAvailable(bool bAvailable) {
            return false;
        }
        public static bool UpdateCurrentEntryText(string pchText) {
            return false;
        }
        public static bool UpdateCurrentEntryElapsedSeconds(int nValue) {
            return false;
        }
        public static bool UpdateCurrentEntryCoverArt(byte[] pvBuffer, uint cbBufferLength) {
            return false;
        }
        public static bool CurrentEntryDidChange() {
            return false;
        }
        public static bool QueueWillChange() {
            return false;
        }
        public static bool ResetQueueEntries() {
            return false;
        }
        public static bool SetQueueEntry(int nID, int nPosition, string pchEntryText) {
            return false;
        }
        public static bool SetCurrentQueueEntry(int nID) {
            return false;
        }
        public static bool QueueDidChange() {
            return false;
        }
        public static bool PlaylistWillChange() {
            return false;
        }
        public static bool ResetPlaylistEntries() {
            return false;
        }
        public static bool SetPlaylistEntry(int nID, int nPosition, string pchEntryText) {
            return false;
        }
        public static bool SetCurrentPlaylistEntry(int nID) {
            return false;
        }
        public static bool PlaylistDidChange() {
            return false;
        }
    }

    public static class SteamNetworking {
        public static bool SendP2PPacket(CSteamID steamIDRemote, byte[] pubData, uint cubData, EP2PSend eP2PSendType, int nChannel = 0) {
            return false;
        }
        public static bool IsP2PPacketAvailable(out uint pcubMsgSize, int nChannel = 0) {
            pcubMsgSize = default;
            return false;
        }
        public static bool ReadP2PPacket(byte[] pubDest, uint cubDest, out uint pcubMsgSize, out CSteamID psteamIDRemote, int nChannel = 0) {
            pcubMsgSize = default;
            psteamIDRemote = default;
            return false;
        }
        public static bool AcceptP2PSessionWithUser(CSteamID steamIDRemote) {
            return false;
        }
        public static bool CloseP2PSessionWithUser(CSteamID steamIDRemote) {
            return false;
        }
        public static bool CloseP2PChannelWithUser(CSteamID steamIDRemote, int nChannel) {
            return false;
        }
        public static bool GetP2PSessionState(CSteamID steamIDRemote, out P2PSessionState_t pConnectionState) {
            pConnectionState = default;
            return false;
        }
        public static bool AllowP2PPacketRelay(bool bAllow) {
            return false;
        }
        public static SNetListenSocket_t CreateListenSocket(int nVirtualP2PPort, SteamIPAddress_t nIP, ushort nPort, bool bAllowUseOfPacketRelay) {
            return default;
        }
        public static SNetSocket_t CreateP2PConnectionSocket(CSteamID steamIDTarget, int nVirtualPort, int nTimeoutSec, bool bAllowUseOfPacketRelay) {
            return default;
        }
        public static SNetSocket_t CreateConnectionSocket(SteamIPAddress_t nIP, ushort nPort, int nTimeoutSec) {
            return default;
        }
        public static bool DestroySocket(SNetSocket_t hSocket, bool bNotifyRemoteEnd) {
            return false;
        }
        public static bool DestroyListenSocket(SNetListenSocket_t hSocket, bool bNotifyRemoteEnd) {
            return false;
        }
        public static bool SendDataOnSocket(SNetSocket_t hSocket, byte[] pubData, uint cubData, bool bReliable) {
            return false;
        }
        public static bool IsDataAvailableOnSocket(SNetSocket_t hSocket, out uint pcubMsgSize) {
            pcubMsgSize = default;
            return false;
        }
        public static bool RetrieveDataFromSocket(SNetSocket_t hSocket, byte[] pubDest, uint cubDest, out uint pcubMsgSize) {
            pcubMsgSize = default;
            return false;
        }
        public static bool IsDataAvailable(SNetListenSocket_t hListenSocket, out uint pcubMsgSize, out SNetSocket_t phSocket) {
            pcubMsgSize = default;
            phSocket = default;
            return false;
        }
        public static bool RetrieveData(SNetListenSocket_t hListenSocket, byte[] pubDest, uint cubDest, out uint pcubMsgSize, out SNetSocket_t phSocket) {
            pcubMsgSize = default;
            phSocket = default;
            return false;
        }
        public static bool GetSocketInfo(SNetSocket_t hSocket, out CSteamID pSteamIDRemote, out int peSocketStatus, out SteamIPAddress_t punIPRemote, out ushort punPortRemote) {
            pSteamIDRemote = default;
            peSocketStatus = default;
            punIPRemote = default;
            punPortRemote = default;
            return false;
        }
        public static bool GetListenSocketInfo(SNetListenSocket_t hListenSocket, out SteamIPAddress_t pnIP, out ushort pnPort) {
            pnIP = default;
            pnPort = default;
            return false;
        }
        public static ESNetSocketConnectionType GetSocketConnectionType(SNetSocket_t hSocket) {
            return default;
        }
        public static int GetMaxPacketSize(SNetSocket_t hSocket) {
            return 0;
        }
    }

    public static class SteamNetworkingMessages {
        public static EResult SendMessageToUser(ref SteamNetworkingIdentity identityRemote, IntPtr pubData, uint cubData, int nSendFlags, int nRemoteChannel) {
            return default;
        }
        public static int ReceiveMessagesOnChannel(int nLocalChannel, IntPtr[] ppOutMessages, int nMaxMessages) {
            return 0;
        }
        public static bool AcceptSessionWithUser(ref SteamNetworkingIdentity identityRemote) {
            return false;
        }
        public static bool CloseSessionWithUser(ref SteamNetworkingIdentity identityRemote) {
            return false;
        }
        public static bool CloseChannelWithUser(ref SteamNetworkingIdentity identityRemote, int nLocalChannel) {
            return false;
        }
        public static ESteamNetworkingConnectionState GetSessionConnectionInfo(ref SteamNetworkingIdentity identityRemote, out SteamNetConnectionInfo_t pConnectionInfo, out SteamNetConnectionRealTimeStatus_t pQuickStatus) {
            pConnectionInfo = default;
            pQuickStatus = default;
            return default;
        }
    }

    public static class SteamNetworkingSockets {
        public static HSteamListenSocket CreateListenSocketIP(ref SteamNetworkingIPAddr localAddress, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static HSteamNetConnection ConnectByIPAddress(ref SteamNetworkingIPAddr address, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return NativeGameNetworkingSockets.ConnectByIPAddress(ref address, nOptions, pOptions);
        }
        public static HSteamListenSocket CreateListenSocketP2P(int nLocalVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static HSteamNetConnection ConnectP2P(ref SteamNetworkingIdentity identityRemote, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamNetConnection.Invalid;
        }
        public static EResult AcceptConnection(HSteamNetConnection hConn) {
            return default;
        }
        public static bool CloseConnection(HSteamNetConnection hPeer, int nReason, string pszDebug, bool bEnableLinger) {
            return NativeGameNetworkingSockets.CloseConnection(hPeer, nReason, pszDebug, bEnableLinger);
        }
        public static bool CloseListenSocket(HSteamListenSocket hSocket) {
            return false;
        }
        public static bool SetConnectionUserData(HSteamNetConnection hPeer, long nUserData) {
            return false;
        }
        public static long GetConnectionUserData(HSteamNetConnection hPeer) {
            return 0;
        }
        public static void SetConnectionName(HSteamNetConnection hPeer, string pszName) {
            return;
        }
        public static bool GetConnectionName(HSteamNetConnection hPeer, out string pszName, int nMaxLen) {
            pszName = default;
            return false;
        }
        public static EResult SendMessageToConnection(HSteamNetConnection hConn, IntPtr pData, uint cbData, int nSendFlags, out long pOutMessageNumber) {
            return NativeGameNetworkingSockets.SendMessageToConnection(hConn, pData, cbData, nSendFlags, out pOutMessageNumber);
        }
        public static void SendMessages(int nMessages, SteamNetworkingMessage_t[] pMessages, long[] pOutMessageNumberOrResult) {
            return;
        }
        public static EResult FlushMessagesOnConnection(HSteamNetConnection hConn) {
            return default;
        }
        public static int ReceiveMessagesOnConnection(HSteamNetConnection hConn, IntPtr[] ppOutMessages, int nMaxMessages) {
            return NativeGameNetworkingSockets.ReceiveMessagesOnConnection(hConn, ppOutMessages, nMaxMessages);
        }
        public static bool GetConnectionInfo(HSteamNetConnection hConn, out SteamNetConnectionInfo_t pInfo) {
            return NativeGameNetworkingSockets.GetConnectionInfo(hConn, out pInfo);
        }
        public static EResult GetConnectionRealTimeStatus(HSteamNetConnection hConn, ref SteamNetConnectionRealTimeStatus_t pStatus, int nLanes, ref SteamNetConnectionRealTimeLaneStatus_t pLanes) {
            return NativeGameNetworkingSockets.GetConnectionRealTimeStatus(hConn, ref pStatus, nLanes, ref pLanes);
        }
        public static int GetDetailedConnectionStatus(HSteamNetConnection hConn, out string pszBuf, int cbBuf) {
            pszBuf = default;
            return 0;
        }
        public static bool GetListenSocketAddress(HSteamListenSocket hSocket, out SteamNetworkingIPAddr address) {
            address = default;
            return false;
        }
        public static bool CreateSocketPair(out HSteamNetConnection pOutConnection1, out HSteamNetConnection pOutConnection2, bool bUseNetworkLoopback, ref SteamNetworkingIdentity pIdentity1, ref SteamNetworkingIdentity pIdentity2) {
            pOutConnection1 = default;
            pOutConnection2 = default;
            return false;
        }
        public static EResult ConfigureConnectionLanes(HSteamNetConnection hConn, int nNumLanes, out int pLanePriorities, out ushort pLaneWeights) {
            pLanePriorities = default;
            pLaneWeights = default;
            return default;
        }
        public static bool GetIdentity(out SteamNetworkingIdentity pIdentity) {
            pIdentity = default;
            return false;
        }
        public static ESteamNetworkingAvailability InitAuthentication() {
            return ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current;
        }
        public static ESteamNetworkingAvailability GetAuthenticationStatus(out SteamNetAuthenticationStatus_t pDetails) {
            pDetails = default;
            return default;
        }
        public static HSteamNetPollGroup CreatePollGroup() {
            return HSteamNetPollGroup.Invalid;
        }
        public static bool DestroyPollGroup(HSteamNetPollGroup hPollGroup) {
            return false;
        }
        public static bool SetConnectionPollGroup(HSteamNetConnection hConn, HSteamNetPollGroup hPollGroup) {
            return false;
        }
        public static int ReceiveMessagesOnPollGroup(HSteamNetPollGroup hPollGroup, IntPtr[] ppOutMessages, int nMaxMessages) {
            return 0;
        }
        public static bool ReceivedRelayAuthTicket(IntPtr pvTicket, int cbTicket, out SteamDatagramRelayAuthTicket pOutParsedTicket) {
            pOutParsedTicket = default;
            return false;
        }
        public static int FindRelayAuthTicketForServer(ref SteamNetworkingIdentity identityGameServer, int nRemoteVirtualPort, out SteamDatagramRelayAuthTicket pOutParsedTicket) {
            pOutParsedTicket = default;
            return 0;
        }
        public static HSteamNetConnection ConnectToHostedDedicatedServer(ref SteamNetworkingIdentity identityTarget, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamNetConnection.Invalid;
        }
        public static ushort GetHostedDedicatedServerPort() {
            return 0;
        }
        public static SteamNetworkingPOPID GetHostedDedicatedServerPOPID() {
            return default;
        }
        public static EResult GetHostedDedicatedServerAddress(out SteamDatagramHostedAddress pRouting) {
            pRouting = default;
            return default;
        }
        public static HSteamListenSocket CreateHostedDedicatedServerListenSocket(int nLocalVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static EResult GetGameCoordinatorServerLogin(IntPtr pLoginInfo, out int pcbSignedBlob, IntPtr pBlob) {
            pcbSignedBlob = default;
            return default;
        }
        public static HSteamNetConnection ConnectP2PCustomSignaling(out ISteamNetworkingConnectionSignaling pSignaling, ref SteamNetworkingIdentity pPeerIdentity, int nRemoteVirtualPort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            pSignaling = default;
            return HSteamNetConnection.Invalid;
        }
        public static bool ReceivedP2PCustomSignal(IntPtr pMsg, int cbMsg, out ISteamNetworkingSignalingRecvContext pContext) {
            pContext = default;
            return false;
        }
        public static bool GetCertificateRequest(out int pcbBlob, IntPtr pBlob, out SteamNetworkingErrMsg errMsg) {
            pcbBlob = default;
            errMsg = default;
            return false;
        }
        public static bool SetCertificate(IntPtr pCertificate, int cbCertificate, out SteamNetworkingErrMsg errMsg) {
            errMsg = default;
            return false;
        }
        public static void ResetIdentity(ref SteamNetworkingIdentity pIdentity) {
            return;
        }
        public static void RunCallbacks() {
            A2SPingQuery.RunCallbacks();
            NativeGameNetworkingSockets.RunCallbacks();
        }
        public static bool BeginAsyncRequestFakeIP(int nNumPorts) {
            return false;
        }
        public static void GetFakeIP(int idxFirstPort, out SteamNetworkingFakeIPResult_t pInfo) {
            pInfo = default;
            return;
        }
        public static HSteamListenSocket CreateListenSocketP2PFakeIP(int idxFakePort, int nOptions, SteamNetworkingConfigValue_t[] pOptions) {
            return HSteamListenSocket.Invalid;
        }
        public static EResult GetRemoteFakeIPForConnection(HSteamNetConnection hConn, out SteamNetworkingIPAddr pOutAddr) {
            pOutAddr = default;
            return default;
        }
        public static IntPtr CreateFakeUDPPort(int idxFakeServerPort) {
            return IntPtr.Zero;
        }
    }

    public static class SteamNetworkingUtils {
        public static IntPtr AllocateMessage(int cbAllocateBuffer) {
            return IntPtr.Zero;
        }
        public static void InitRelayNetworkAccess() {
            return;
        }
        public static ESteamNetworkingAvailability GetRelayNetworkStatus(out SteamRelayNetworkStatus_t pDetails) {
            pDetails = default;
            return default;
        }
        public static float GetLocalPingLocation(out SteamNetworkPingLocation_t result) {
            result = default;
            return 0;
        }
        public static int EstimatePingTimeBetweenTwoLocations(ref SteamNetworkPingLocation_t location1, ref SteamNetworkPingLocation_t location2) {
            return 0;
        }
        public static int EstimatePingTimeFromLocalHost(ref SteamNetworkPingLocation_t remoteLocation) {
            return 0;
        }
        public static void ConvertPingLocationToString(ref SteamNetworkPingLocation_t location, out string pszBuf, int cchBufSize) {
            pszBuf = default;
            return;
        }
        public static bool ParsePingLocationString(string pszString, out SteamNetworkPingLocation_t result) {
            result = default;
            return false;
        }
        public static bool CheckPingDataUpToDate(float flMaxAgeSeconds) {
            return false;
        }
        public static int GetPingToDataCenter(SteamNetworkingPOPID popID, out SteamNetworkingPOPID pViaRelayPoP) {
            pViaRelayPoP = default;
            return 0;
        }
        public static int GetDirectPingToPOP(SteamNetworkingPOPID popID) {
            return 0;
        }
        public static int GetPOPCount() {
            return 0;
        }
        public static int GetPOPList(out SteamNetworkingPOPID list, int nListSz) {
            list = default;
            return 0;
        }
        public static SteamNetworkingMicroseconds GetLocalTimestamp() {
            return default;
        }
        public static void SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType eDetailLevel, FSteamNetworkingSocketsDebugOutput pfnFunc) {
            NativeGameNetworkingSockets.SetUserDebugOutputFunction(eDetailLevel, pfnFunc);
        }
        public static bool IsFakeIPv4(uint nIPv4) {
            return false;
        }
        public static ESteamNetworkingFakeIPType GetIPv4FakeIPType(uint nIPv4) {
            return default;
        }
        public static EResult GetRealIdentityForFakeIP(ref SteamNetworkingIPAddr fakeIP, out SteamNetworkingIdentity pOutRealIdentity) {
            pOutRealIdentity = default;
            return default;
        }
        public static bool SetConfigValue(ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, ESteamNetworkingConfigDataType eDataType, IntPtr pArg) {
            return false;
        }
        public static ESteamNetworkingGetConfigValueResult GetConfigValue(ESteamNetworkingConfigValue eValue, ESteamNetworkingConfigScope eScopeType, IntPtr scopeObj, out ESteamNetworkingConfigDataType pOutDataType, IntPtr pResult, ref ulong cbResult) {
            pOutDataType = default;
            return default;
        }
        public static string GetConfigValueInfo(ESteamNetworkingConfigValue eValue, out ESteamNetworkingConfigDataType pOutDataType, out ESteamNetworkingConfigScope pOutScope) {
            pOutDataType = default;
            pOutScope = default;
            return string.Empty;
        }
        public static ESteamNetworkingConfigValue IterateGenericEditableConfigValues(ESteamNetworkingConfigValue eCurrent, bool bEnumerateDevVars) {
            return default;
        }
        public static void SteamNetworkingIPAddr_ToString(ref SteamNetworkingIPAddr addr, out string buf, uint cbBuf, bool bWithPort) {
            buf = default;
            return;
        }
        public static bool SteamNetworkingIPAddr_ParseString(out SteamNetworkingIPAddr pAddr, string pszStr) {
            pAddr = default;
            return false;
        }
        public static ESteamNetworkingFakeIPType SteamNetworkingIPAddr_GetFakeIPType(ref SteamNetworkingIPAddr addr) {
            return default;
        }
        public static void SteamNetworkingIdentity_ToString(ref SteamNetworkingIdentity identity, out string buf, uint cbBuf) {
            buf = default;
            return;
        }
        public static bool SteamNetworkingIdentity_ParseString(out SteamNetworkingIdentity pIdentity, string pszStr) {
            pIdentity = default;
            return false;
        }
    }

    public static class SteamParentalSettings {
        public static bool BIsParentalLockEnabled() {
            return false;
        }
        public static bool BIsParentalLockLocked() {
            return false;
        }
        public static bool BIsAppBlocked(AppId_t nAppID) {
            return false;
        }
        public static bool BIsAppInBlockList(AppId_t nAppID) {
            return false;
        }
        public static bool BIsFeatureBlocked(EParentalFeature eFeature) {
            return false;
        }
        public static bool BIsFeatureInBlockList(EParentalFeature eFeature) {
            return false;
        }
    }

    public static class SteamRemotePlay {
        public static uint GetSessionCount() {
            return 0;
        }
        public static RemotePlaySessionID_t GetSessionID(int iSessionIndex) {
            return default;
        }
        public static CSteamID GetSessionSteamID(RemotePlaySessionID_t unSessionID) {
            return new CSteamID(76561198000000000UL);
        }
        public static string GetSessionClientName(RemotePlaySessionID_t unSessionID) {
            return string.Empty;
        }
        public static ESteamDeviceFormFactor GetSessionClientFormFactor(RemotePlaySessionID_t unSessionID) {
            return default;
        }
        public static bool BGetSessionClientResolution(RemotePlaySessionID_t unSessionID, out int pnResolutionX, out int pnResolutionY) {
            pnResolutionX = default;
            pnResolutionY = default;
            return false;
        }
        public static bool BSendRemotePlayTogetherInvite(CSteamID steamIDFriend) {
            return false;
        }
    }

    public static class SteamRemoteStorage {
        public static bool FileWrite(string pchFile, byte[] pvData, int cubData) {
            return false;
        }
        public static int FileRead(string pchFile, byte[] pvData, int cubDataToRead) {
            return 0;
        }
        public static SteamAPICall_t FileWriteAsync(string pchFile, byte[] pvData, uint cubData) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t FileReadAsync(string pchFile, uint nOffset, uint cubToRead) {
            return SteamAPICall_t.Invalid;
        }
        public static bool FileReadAsyncComplete(SteamAPICall_t hReadCall, byte[] pvBuffer, uint cubToRead) {
            return false;
        }
        public static bool FileForget(string pchFile) {
            return false;
        }
        public static bool FileDelete(string pchFile) {
            return false;
        }
        public static SteamAPICall_t FileShare(string pchFile) {
            return SteamAPICall_t.Invalid;
        }
        public static bool SetSyncPlatforms(string pchFile, ERemoteStoragePlatform eRemoteStoragePlatform) {
            return false;
        }
        public static UGCFileWriteStreamHandle_t FileWriteStreamOpen(string pchFile) {
            return default;
        }
        public static bool FileWriteStreamWriteChunk(UGCFileWriteStreamHandle_t writeHandle, byte[] pvData, int cubData) {
            return false;
        }
        public static bool FileWriteStreamClose(UGCFileWriteStreamHandle_t writeHandle) {
            return false;
        }
        public static bool FileWriteStreamCancel(UGCFileWriteStreamHandle_t writeHandle) {
            return false;
        }
        public static bool FileExists(string pchFile) {
            return false;
        }
        public static bool FilePersisted(string pchFile) {
            return false;
        }
        public static int GetFileSize(string pchFile) {
            return 0;
        }
        public static long GetFileTimestamp(string pchFile) {
            return 0;
        }
        public static ERemoteStoragePlatform GetSyncPlatforms(string pchFile) {
            return default;
        }
        public static int GetFileCount() {
            return 0;
        }
        public static string GetFileNameAndSize(int iFile, out int pnFileSizeInBytes) {
            pnFileSizeInBytes = default;
            return string.Empty;
        }
        public static bool GetQuota(out ulong pnTotalBytes, out ulong puAvailableBytes) {
            pnTotalBytes = default;
            puAvailableBytes = default;
            return false;
        }
        public static bool IsCloudEnabledForAccount() {
            return false;
        }
        public static bool IsCloudEnabledForApp() {
            return false;
        }
        public static void SetCloudEnabledForApp(bool bEnabled) {
            return;
        }
        public static SteamAPICall_t UGCDownload(UGCHandle_t hContent, uint unPriority) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetUGCDownloadProgress(UGCHandle_t hContent, out int pnBytesDownloaded, out int pnBytesExpected) {
            pnBytesDownloaded = default;
            pnBytesExpected = default;
            return false;
        }
        public static bool GetUGCDetails(UGCHandle_t hContent, out AppId_t pnAppID, out string ppchName, out int pnFileSizeInBytes, out CSteamID pSteamIDOwner) {
            pnAppID = default;
            ppchName = default;
            pnFileSizeInBytes = default;
            pSteamIDOwner = default;
            return false;
        }
        public static int UGCRead(UGCHandle_t hContent, byte[] pvData, int cubDataToRead, uint cOffset, EUGCReadAction eAction) {
            return 0;
        }
        public static int GetCachedUGCCount() {
            return 0;
        }
        public static UGCHandle_t GetCachedUGCHandle(int iCachedContent) {
            return default;
        }
        public static SteamAPICall_t PublishWorkshopFile(string pchFile, string pchPreviewFile, AppId_t nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, System.Collections.Generic.IList<string> pTags, EWorkshopFileType eWorkshopFileType) {
            return SteamAPICall_t.Invalid;
        }
        public static PublishedFileUpdateHandle_t CreatePublishedFileUpdateRequest(PublishedFileId_t unPublishedFileId) {
            return default;
        }
        public static bool UpdatePublishedFileFile(PublishedFileUpdateHandle_t updateHandle, string pchFile) {
            return false;
        }
        public static bool UpdatePublishedFilePreviewFile(PublishedFileUpdateHandle_t updateHandle, string pchPreviewFile) {
            return false;
        }
        public static bool UpdatePublishedFileTitle(PublishedFileUpdateHandle_t updateHandle, string pchTitle) {
            return false;
        }
        public static bool UpdatePublishedFileDescription(PublishedFileUpdateHandle_t updateHandle, string pchDescription) {
            return false;
        }
        public static bool UpdatePublishedFileVisibility(PublishedFileUpdateHandle_t updateHandle, ERemoteStoragePublishedFileVisibility eVisibility) {
            return false;
        }
        public static bool UpdatePublishedFileTags(PublishedFileUpdateHandle_t updateHandle, System.Collections.Generic.IList<string> pTags) {
            return false;
        }
        public static SteamAPICall_t CommitPublishedFileUpdate(PublishedFileUpdateHandle_t updateHandle) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetPublishedFileDetails(PublishedFileId_t unPublishedFileId, uint unMaxSecondsOld) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t DeletePublishedFile(PublishedFileId_t unPublishedFileId) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumerateUserPublishedFiles(uint unStartIndex) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t SubscribePublishedFile(PublishedFileId_t unPublishedFileId) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumerateUserSubscribedFiles(uint unStartIndex) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t UnsubscribePublishedFile(PublishedFileId_t unPublishedFileId) {
            return SteamAPICall_t.Invalid;
        }
        public static bool UpdatePublishedFileSetChangeDescription(PublishedFileUpdateHandle_t updateHandle, string pchChangeDescription) {
            return false;
        }
        public static SteamAPICall_t GetPublishedItemVoteDetails(PublishedFileId_t unPublishedFileId) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t UpdateUserPublishedItemVote(PublishedFileId_t unPublishedFileId, bool bVoteUp) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetUserPublishedItemVoteDetails(PublishedFileId_t unPublishedFileId) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumerateUserSharedWorkshopFiles(CSteamID steamId, uint unStartIndex, System.Collections.Generic.IList<string> pRequiredTags, System.Collections.Generic.IList<string> pExcludedTags) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t PublishVideo(EWorkshopVideoProvider eVideoProvider, string pchVideoAccount, string pchVideoIdentifier, string pchPreviewFile, AppId_t nConsumerAppId, string pchTitle, string pchDescription, ERemoteStoragePublishedFileVisibility eVisibility, System.Collections.Generic.IList<string> pTags) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t SetUserPublishedFileAction(PublishedFileId_t unPublishedFileId, EWorkshopFileAction eAction) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumeratePublishedFilesByUserAction(EWorkshopFileAction eAction, uint unStartIndex) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType eEnumerationType, uint unStartIndex, uint unCount, uint unDays, System.Collections.Generic.IList<string> pTags, System.Collections.Generic.IList<string> pUserTags) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t UGCDownloadToLocation(UGCHandle_t hContent, string pchLocation, uint unPriority) {
            return SteamAPICall_t.Invalid;
        }
        public static int GetLocalFileChangeCount() {
            return 0;
        }
        public static string GetLocalFileChange(int iFile, out ERemoteStorageLocalFileChange pEChangeType, out ERemoteStorageFilePathType pEFilePathType) {
            pEChangeType = default;
            pEFilePathType = default;
            return string.Empty;
        }
        public static bool BeginFileWriteBatch() {
            return false;
        }
        public static bool EndFileWriteBatch() {
            return false;
        }
    }

    public static class SteamScreenshots {
        public static ScreenshotHandle WriteScreenshot(byte[] pubRGB, uint cubRGB, int nWidth, int nHeight) {
            return default;
        }
        public static ScreenshotHandle AddScreenshotToLibrary(string pchFilename, string pchThumbnailFilename, int nWidth, int nHeight) {
            return default;
        }
        public static void TriggerScreenshot() {
            return;
        }
        public static void HookScreenshots(bool bHook) {
            return;
        }
        public static bool SetLocation(ScreenshotHandle hScreenshot, string pchLocation) {
            return false;
        }
        public static bool TagUser(ScreenshotHandle hScreenshot, CSteamID steamID) {
            return false;
        }
        public static bool TagPublishedFile(ScreenshotHandle hScreenshot, PublishedFileId_t unPublishedFileID) {
            return false;
        }
        public static bool IsScreenshotsHooked() {
            return false;
        }
        public static ScreenshotHandle AddVRScreenshotToLibrary(EVRScreenshotType eType, string pchFilename, string pchVRFilename) {
            return default;
        }
    }

    public static class SteamUGC {
        public static UGCQueryHandle_t CreateQueryUserUGCRequest(AccountID_t unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder, AppId_t nCreatorAppID, AppId_t nConsumerAppID, uint unPage) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, AppId_t nCreatorAppID, AppId_t nConsumerAppID, uint unPage) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryAllUGCRequest(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType, AppId_t nCreatorAppID, AppId_t nConsumerAppID, string pchCursor = null) {
            return UGCQueryHandle_t.Invalid;
        }
        public static UGCQueryHandle_t CreateQueryUGCDetailsRequest(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return UGCQueryHandle_t.Invalid;
        }
        public static SteamAPICall_t SendQueryUGCRequest(UGCQueryHandle_t handle) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetQueryUGCResult(UGCQueryHandle_t handle, uint index, out SteamUGCDetails_t pDetails) {
            pDetails = default;
            return false;
        }
        public static uint GetQueryUGCNumTags(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCTag(UGCQueryHandle_t handle, uint index, uint indexTag, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCTagDisplayName(UGCQueryHandle_t handle, uint index, uint indexTag, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCPreviewURL(UGCQueryHandle_t handle, uint index, out string pchURL, uint cchURLSize) {
            pchURL = default;
            return false;
        }
        public static bool GetQueryUGCMetadata(UGCQueryHandle_t handle, uint index, out string pchMetadata, uint cchMetadatasize) {
            pchMetadata = default;
            return false;
        }
        public static bool GetQueryUGCChildren(UGCQueryHandle_t handle, uint index, PublishedFileId_t[] pvecPublishedFileID, uint cMaxEntries) {
            return false;
        }
        public static bool GetQueryUGCStatistic(UGCQueryHandle_t handle, uint index, EItemStatistic eStatType, out ulong pStatValue) {
            pStatValue = default;
            return false;
        }
        public static uint GetQueryUGCNumAdditionalPreviews(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCAdditionalPreview(UGCQueryHandle_t handle, uint index, uint previewIndex, out string pchURLOrVideoID, uint cchURLSize, out string pchOriginalFileName, uint cchOriginalFileNameSize, out EItemPreviewType pPreviewType) {
            pchURLOrVideoID = default;
            pchOriginalFileName = default;
            pPreviewType = default;
            return false;
        }
        public static uint GetQueryUGCNumKeyValueTags(UGCQueryHandle_t handle, uint index) {
            return 0;
        }
        public static bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex, out string pchKey, uint cchKeySize, out string pchValue, uint cchValueSize) {
            pchKey = default;
            pchValue = default;
            return false;
        }
        public static bool GetQueryUGCKeyValueTag(UGCQueryHandle_t handle, uint index, string pchKey, out string pchValue, uint cchValueSize) {
            pchValue = default;
            return false;
        }
        public static uint GetQueryUGCContentDescriptors(UGCQueryHandle_t handle, uint index, out EUGCContentDescriptorID pvecDescriptors, uint cMaxEntries) {
            pvecDescriptors = default;
            return 0;
        }
        public static bool ReleaseQueryUGCRequest(UGCQueryHandle_t handle) {
            return false;
        }
        public static bool AddRequiredTag(UGCQueryHandle_t handle, string pTagName) {
            return false;
        }
        public static bool AddRequiredTagGroup(UGCQueryHandle_t handle, System.Collections.Generic.IList<string> pTagGroups) {
            return false;
        }
        public static bool AddExcludedTag(UGCQueryHandle_t handle, string pTagName) {
            return false;
        }
        public static bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool bReturnOnlyIDs) {
            return false;
        }
        public static bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool bReturnKeyValueTags) {
            return false;
        }
        public static bool SetReturnLongDescription(UGCQueryHandle_t handle, bool bReturnLongDescription) {
            return false;
        }
        public static bool SetReturnMetadata(UGCQueryHandle_t handle, bool bReturnMetadata) {
            return false;
        }
        public static bool SetReturnChildren(UGCQueryHandle_t handle, bool bReturnChildren) {
            return false;
        }
        public static bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool bReturnAdditionalPreviews) {
            return false;
        }
        public static bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool bReturnTotalOnly) {
            return false;
        }
        public static bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, uint unDays) {
            return false;
        }
        public static bool SetLanguage(UGCQueryHandle_t handle, string pchLanguage) {
            return false;
        }
        public static bool SetAllowCachedResponse(UGCQueryHandle_t handle, uint unMaxAgeSeconds) {
            return false;
        }
        public static bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string pMatchCloudFileName) {
            return false;
        }
        public static bool SetMatchAnyTag(UGCQueryHandle_t handle, bool bMatchAnyTag) {
            return false;
        }
        public static bool SetSearchText(UGCQueryHandle_t handle, string pSearchText) {
            return false;
        }
        public static bool SetRankedByTrendDays(UGCQueryHandle_t handle, uint unDays) {
            return false;
        }
        public static bool SetTimeCreatedDateRange(UGCQueryHandle_t handle, uint rtStart, uint rtEnd) {
            return false;
        }
        public static bool SetTimeUpdatedDateRange(UGCQueryHandle_t handle, uint rtStart, uint rtEnd) {
            return false;
        }
        public static bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string pKey, string pValue) {
            return false;
        }
        public static SteamAPICall_t RequestUGCDetails(PublishedFileId_t nPublishedFileID, uint unMaxAgeSeconds) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t CreateItem(AppId_t nConsumerAppId, EWorkshopFileType eFileType) {
            return SteamAPICall_t.Invalid;
        }
        public static UGCUpdateHandle_t StartItemUpdate(AppId_t nConsumerAppId, PublishedFileId_t nPublishedFileID) {
            return UGCUpdateHandle_t.Invalid;
        }
        public static bool SetItemTitle(UGCUpdateHandle_t handle, string pchTitle) {
            return false;
        }
        public static bool SetItemDescription(UGCUpdateHandle_t handle, string pchDescription) {
            return false;
        }
        public static bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string pchLanguage) {
            return false;
        }
        public static bool SetItemMetadata(UGCUpdateHandle_t handle, string pchMetaData) {
            return false;
        }
        public static bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility eVisibility) {
            return false;
        }
        public static bool SetItemTags(UGCUpdateHandle_t updateHandle, System.Collections.Generic.IList<string> pTags) {
            return false;
        }
        public static bool SetItemContent(UGCUpdateHandle_t handle, string pszContentFolder) {
            return false;
        }
        public static bool SetItemPreview(UGCUpdateHandle_t handle, string pszPreviewFile) {
            return false;
        }
        public static bool SetAllowLegacyUpload(UGCUpdateHandle_t handle, bool bAllowLegacyUpload) {
            return false;
        }
        public static bool RemoveAllItemKeyValueTags(UGCUpdateHandle_t handle) {
            return false;
        }
        public static bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string pchKey) {
            return false;
        }
        public static bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string pchKey, string pchValue) {
            return false;
        }
        public static bool AddItemPreviewFile(UGCUpdateHandle_t handle, string pszPreviewFile, EItemPreviewType type) {
            return false;
        }
        public static bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string pszVideoID) {
            return false;
        }
        public static bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, uint index, string pszPreviewFile) {
            return false;
        }
        public static bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, uint index, string pszVideoID) {
            return false;
        }
        public static bool RemoveItemPreview(UGCUpdateHandle_t handle, uint index) {
            return false;
        }
        public static bool AddContentDescriptor(UGCUpdateHandle_t handle, EUGCContentDescriptorID descid) {
            return false;
        }
        public static bool RemoveContentDescriptor(UGCUpdateHandle_t handle, EUGCContentDescriptorID descid) {
            return false;
        }
        public static SteamAPICall_t SubmitItemUpdate(UGCUpdateHandle_t handle, string pchChangeNote) {
            return SteamAPICall_t.Invalid;
        }
        public static EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, out ulong punBytesProcessed, out ulong punBytesTotal) {
            punBytesProcessed = default;
            punBytesTotal = default;
            return default;
        }
        public static SteamAPICall_t SetUserItemVote(PublishedFileId_t nPublishedFileID, bool bVoteUp) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetUserItemVote(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddItemToFavorites(AppId_t nAppId, PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveItemFromFavorites(AppId_t nAppId, PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t SubscribeItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t UnsubscribeItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static uint GetNumSubscribedItems() {
            return 0;
        }
        public static uint GetSubscribedItems(PublishedFileId_t[] pvecPublishedFileID, uint cMaxEntries) {
            return 0;
        }
        public static uint GetItemState(PublishedFileId_t nPublishedFileID) {
            return 0;
        }
        public static bool GetItemInstallInfo(PublishedFileId_t nPublishedFileID, out ulong punSizeOnDisk, out string pchFolder, uint cchFolderSize, out uint punTimeStamp) {
            punSizeOnDisk = default;
            pchFolder = default;
            punTimeStamp = default;
            return false;
        }
        public static bool GetItemDownloadInfo(PublishedFileId_t nPublishedFileID, out ulong punBytesDownloaded, out ulong punBytesTotal) {
            punBytesDownloaded = default;
            punBytesTotal = default;
            return false;
        }
        public static bool DownloadItem(PublishedFileId_t nPublishedFileID, bool bHighPriority) {
            return false;
        }
        public static bool BInitWorkshopForGameServer(DepotId_t unWorkshopDepotID, string pszFolder) {
            return false;
        }
        public static void SuspendDownloads(bool bSuspend) {
            return;
        }
        public static SteamAPICall_t StartPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t StopPlaytimeTracking(PublishedFileId_t[] pvecPublishedFileID, uint unNumPublishedFileIDs) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t StopPlaytimeTrackingForAllItems() {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveDependency(PublishedFileId_t nParentPublishedFileID, PublishedFileId_t nChildPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AddAppDependency(PublishedFileId_t nPublishedFileID, AppId_t nAppID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RemoveAppDependency(PublishedFileId_t nPublishedFileID, AppId_t nAppID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetAppDependencies(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t DeleteItem(PublishedFileId_t nPublishedFileID) {
            return SteamAPICall_t.Invalid;
        }
        public static bool ShowWorkshopEULA() {
            return false;
        }
        public static SteamAPICall_t GetWorkshopEULAStatus() {
            return SteamAPICall_t.Invalid;
        }
    }

    public static class SteamUser {
        public static HSteamUser GetHSteamUser() {
            return default;
        }
        public static bool BLoggedOn() {
            return false;
        }
        public static CSteamID GetSteamID() {
            return new CSteamID(76561198000000000UL);
        }
        public static int InitiateGameConnection_DEPRECATED(byte[] pAuthBlob, int cbMaxAuthBlob, CSteamID steamIDGameServer, uint unIPServer, ushort usPortServer, bool bSecure) {
            return 0;
        }
        public static void TerminateGameConnection_DEPRECATED(uint unIPServer, ushort usPortServer) {
            return;
        }
        public static void TrackAppUsageEvent(CGameID gameID, int eAppUsageEvent, string pchExtraInfo = "") {
            return;
        }
        public static bool GetUserDataFolder(out string pchBuffer, int cubBuffer) {
            pchBuffer = default;
            return false;
        }
        public static void StartVoiceRecording() {
            return;
        }
        public static void StopVoiceRecording() {
            return;
        }
        public static EVoiceResult GetAvailableVoice(out uint pcbCompressed) {
            pcbCompressed = default;
            return default;
        }
        public static EVoiceResult GetVoice(bool bWantCompressed, byte[] pDestBuffer, uint cbDestBufferSize, out uint nBytesWritten) {
            nBytesWritten = default;
            return default;
        }
        public static EVoiceResult DecompressVoice(byte[] pCompressed, uint cbCompressed, byte[] pDestBuffer, uint cbDestBufferSize, out uint nBytesWritten, uint nDesiredSampleRate) {
            nBytesWritten = default;
            return default;
        }
        public static uint GetVoiceOptimalSampleRate() {
            return 0;
        }
        public static HAuthTicket GetAuthSessionTicket(byte[] pTicket, int cbMaxTicket, out uint pcbTicket, ref SteamNetworkingIdentity pSteamNetworkingIdentity) {
            if (pTicket != null && cbMaxTicket >= 128) {
                using (var ms = new System.IO.MemoryStream(pTicket)) {
                    using (var writer = new System.IO.BinaryWriter(ms)) {
                        uint ticketLength = 128;
                        writer.Write(ticketLength);
                        writer.Write((uint)1); // Version
                        ulong steamId = GetSteamID().m_SteamID;
                        if (steamId == 0) steamId = 76561198000000000UL;
                        writer.Write(steamId);
                        writer.Write((uint)304930); // Unturned AppID
                        writer.Write((uint)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        for (int i = 0; i < 104; i++) {
                            writer.Write((byte)0xAA);
                        }
                        pcbTicket = ticketLength;
                        return new HAuthTicket(1);
                    }
                }
            }
            pcbTicket = default;
            return HAuthTicket.Invalid;
        }
        public static HAuthTicket GetAuthTicketForWebApi(string pchIdentity) {
            return HAuthTicket.Invalid;
        }
        public static EBeginAuthSessionResult BeginAuthSession(byte[] pAuthTicket, int cbAuthTicket, CSteamID steamID) {
            return default;
        }
        public static void EndAuthSession(CSteamID steamID) {
            return;
        }
        public static void CancelAuthTicket(HAuthTicket hAuthTicket) {
            return;
        }
        public static EUserHasLicenseForAppResult UserHasLicenseForApp(CSteamID steamID, AppId_t appID) {
            return default;
        }
        public static bool BIsBehindNAT() {
            return false;
        }
        public static void AdvertiseGame(CSteamID steamIDGameServer, uint unIPServer, ushort usPortServer) {
            return;
        }
        public static SteamAPICall_t RequestEncryptedAppTicket(byte[] pDataToInclude, int cbDataToInclude) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetEncryptedAppTicket(byte[] pTicket, int cbMaxTicket, out uint pcbTicket) {
            pcbTicket = default;
            return false;
        }
        public static int GetGameBadgeLevel(int nSeries, bool bFoil) {
            return 0;
        }
        public static int GetPlayerSteamLevel() {
            return 0;
        }
        public static SteamAPICall_t RequestStoreAuthURL(string pchRedirectURL) {
            return SteamAPICall_t.Invalid;
        }
        public static bool BIsPhoneVerified() {
            return false;
        }
        public static bool BIsTwoFactorEnabled() {
            return false;
        }
        public static bool BIsPhoneIdentifying() {
            return false;
        }
        public static bool BIsPhoneRequiringVerification() {
            return false;
        }
        public static SteamAPICall_t GetMarketEligibility() {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetDurationControl() {
            return SteamAPICall_t.Invalid;
        }
        public static bool BSetDurationControlOnlineState(EDurationControlOnlineState eNewState) {
            return false;
        }
    }

    public static class SteamUserStats {
        public static bool RequestCurrentStats() {
            return false;
        }
        public static bool GetStat(string pchName, out int pData) {
            pData = default;
            return false;
        }
        public static bool GetStat(string pchName, out float pData) {
            pData = default;
            return false;
        }
        public static bool SetStat(string pchName, int nData) {
            return false;
        }
        public static bool SetStat(string pchName, float fData) {
            return false;
        }
        public static bool UpdateAvgRateStat(string pchName, float flCountThisSession, double dSessionLength) {
            return false;
        }
        public static bool GetAchievement(string pchName, out bool pbAchieved) {
            pbAchieved = default;
            return false;
        }
        public static bool SetAchievement(string pchName) {
            return false;
        }
        public static bool ClearAchievement(string pchName) {
            return false;
        }
        public static bool GetAchievementAndUnlockTime(string pchName, out bool pbAchieved, out uint punUnlockTime) {
            pbAchieved = default;
            punUnlockTime = default;
            return false;
        }
        public static bool StoreStats() {
            return false;
        }
        public static int GetAchievementIcon(string pchName) {
            return 0;
        }
        public static string GetAchievementDisplayAttribute(string pchName, string pchKey) {
            return string.Empty;
        }
        public static bool IndicateAchievementProgress(string pchName, uint nCurProgress, uint nMaxProgress) {
            return false;
        }
        public static uint GetNumAchievements() {
            return 0;
        }
        public static string GetAchievementName(uint iAchievement) {
            return string.Empty;
        }
        public static SteamAPICall_t RequestUserStats(CSteamID steamIDUser) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out int pData) {
            pData = default;
            return false;
        }
        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out float pData) {
            pData = default;
            return false;
        }
        public static bool GetUserAchievement(CSteamID steamIDUser, string pchName, out bool pbAchieved) {
            pbAchieved = default;
            return false;
        }
        public static bool GetUserAchievementAndUnlockTime(CSteamID steamIDUser, string pchName, out bool pbAchieved, out uint punUnlockTime) {
            pbAchieved = default;
            punUnlockTime = default;
            return false;
        }
        public static bool ResetAllStats(bool bAchievementsToo) {
            return false;
        }
        public static SteamAPICall_t FindOrCreateLeaderboard(string pchLeaderboardName, ELeaderboardSortMethod eLeaderboardSortMethod, ELeaderboardDisplayType eLeaderboardDisplayType) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t FindLeaderboard(string pchLeaderboardName) {
            return SteamAPICall_t.Invalid;
        }
        public static string GetLeaderboardName(SteamLeaderboard_t hSteamLeaderboard) {
            return string.Empty;
        }
        public static int GetLeaderboardEntryCount(SteamLeaderboard_t hSteamLeaderboard) {
            return 0;
        }
        public static ELeaderboardSortMethod GetLeaderboardSortMethod(SteamLeaderboard_t hSteamLeaderboard) {
            return default;
        }
        public static ELeaderboardDisplayType GetLeaderboardDisplayType(SteamLeaderboard_t hSteamLeaderboard) {
            return default;
        }
        public static SteamAPICall_t DownloadLeaderboardEntries(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardDataRequest eLeaderboardDataRequest, int nRangeStart, int nRangeEnd) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t DownloadLeaderboardEntriesForUsers(SteamLeaderboard_t hSteamLeaderboard, CSteamID[] prgUsers, int cUsers) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetDownloadedLeaderboardEntry(SteamLeaderboardEntries_t hSteamLeaderboardEntries, int index, out LeaderboardEntry_t pLeaderboardEntry, int[] pDetails, int cDetailsMax) {
            pLeaderboardEntry = default;
            return false;
        }
        public static SteamAPICall_t UploadLeaderboardScore(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod, int nScore, int[] pScoreDetails, int cScoreDetailsCount) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t AttachLeaderboardUGC(SteamLeaderboard_t hSteamLeaderboard, UGCHandle_t hUGC) {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t GetNumberOfCurrentPlayers() {
            return SteamAPICall_t.Invalid;
        }
        public static SteamAPICall_t RequestGlobalAchievementPercentages() {
            return SteamAPICall_t.Invalid;
        }
        public static int GetMostAchievedAchievementInfo(out string pchName, uint unNameBufLen, out float pflPercent, out bool pbAchieved) {
            pchName = default;
            pflPercent = default;
            pbAchieved = default;
            return 0;
        }
        public static int GetNextMostAchievedAchievementInfo(int iIteratorPrevious, out string pchName, uint unNameBufLen, out float pflPercent, out bool pbAchieved) {
            pchName = default;
            pflPercent = default;
            pbAchieved = default;
            return 0;
        }
        public static bool GetAchievementAchievedPercent(string pchName, out float pflPercent) {
            pflPercent = default;
            return false;
        }
        public static SteamAPICall_t RequestGlobalStats(int nHistoryDays) {
            return SteamAPICall_t.Invalid;
        }
        public static bool GetGlobalStat(string pchStatName, out long pData) {
            pData = default;
            return false;
        }
        public static bool GetGlobalStat(string pchStatName, out double pData) {
            pData = default;
            return false;
        }
        public static int GetGlobalStatHistory(string pchStatName, long[] pData, uint cubData) {
            return 0;
        }
        public static int GetGlobalStatHistory(string pchStatName, double[] pData, uint cubData) {
            return 0;
        }
        public static bool GetAchievementProgressLimits(string pchName, out int pnMinProgress, out int pnMaxProgress) {
            pnMinProgress = default;
            pnMaxProgress = default;
            return false;
        }
        public static bool GetAchievementProgressLimits(string pchName, out float pfMinProgress, out float pfMaxProgress) {
            pfMinProgress = default;
            pfMaxProgress = default;
            return false;
        }
    }

    public static class SteamUtils {
        public static uint GetSecondsSinceAppActive() {
            return 0;
        }
        public static uint GetSecondsSinceComputerActive() {
            return 0;
        }
        public static EUniverse GetConnectedUniverse() {
            return default;
        }
        public static uint GetServerRealTime() {
            return (uint)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static string GetIPCountry() {
            return string.Empty;
        }
        public static bool GetImageSize(int iImage, out uint pnWidth, out uint pnHeight) {
            pnWidth = default;
            pnHeight = default;
            return false;
        }
        public static bool GetImageRGBA(int iImage, byte[] pubDest, int nDestBufferSize) {
            return false;
        }
        public static byte GetCurrentBatteryPower() {
            return 0;
        }
        public static AppId_t GetAppID() {
            return new AppId_t(304930);
        }
        public static void SetOverlayNotificationPosition(ENotificationPosition eNotificationPosition) {
            return;
        }
        public static bool IsAPICallCompleted(SteamAPICall_t hSteamAPICall, out bool pbFailed) {
            pbFailed = default;
            return false;
        }
        public static ESteamAPICallFailure GetAPICallFailureReason(SteamAPICall_t hSteamAPICall) {
            return default;
        }
        public static bool GetAPICallResult(SteamAPICall_t hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, out bool pbFailed) {
            pbFailed = default;
            return false;
        }
        public static uint GetIPCCallCount() {
            return 0;
        }
        public static void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction) {
            return;
        }
        public static bool IsOverlayEnabled() {
            return false;
        }
        public static bool BOverlayNeedsPresent() {
            return false;
        }
        public static SteamAPICall_t CheckFileSignature(string szFileName) {
            return SteamAPICall_t.Invalid;
        }
        public static bool ShowGamepadTextInput(EGamepadTextInputMode eInputMode, EGamepadTextInputLineMode eLineInputMode, string pchDescription, uint unCharMax, string pchExistingText) {
            return false;
        }
        public static uint GetEnteredGamepadTextLength() {
            return 0;
        }
        public static bool GetEnteredGamepadTextInput(out string pchText, uint cchText) {
            pchText = default;
            return false;
        }
        public static string GetSteamUILanguage() {
            return "english";
        }
        public static bool IsSteamRunningInVR() {
            return false;
        }
        public static void SetOverlayNotificationInset(int nHorizontalInset, int nVerticalInset) {
            return;
        }
        public static bool IsSteamInBigPictureMode() {
            return false;
        }
        public static void StartVRDashboard() {
            return;
        }
        public static bool IsVRHeadsetStreamingEnabled() {
            return false;
        }
        public static void SetVRHeadsetStreamingEnabled(bool bEnabled) {
            return;
        }
        public static bool IsSteamChinaLauncher() {
            return false;
        }
        public static bool InitFilterText(uint unFilterOptions = 0) {
            return false;
        }
        public static int FilterText(ETextFilteringContext eContext, CSteamID sourceSteamID, string pchInputMessage, out string pchOutFilteredText, uint nByteSizeOutFilteredText) {
            pchOutFilteredText = default;
            return 0;
        }
        public static ESteamIPv6ConnectivityState GetIPv6ConnectivityState(ESteamIPv6ConnectivityProtocol eProtocol) {
            return default;
        }
        public static bool IsSteamRunningOnSteamDeck() {
            return false;
        }
        public static bool ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode eKeyboardMode, int nTextFieldXPosition, int nTextFieldYPosition, int nTextFieldWidth, int nTextFieldHeight) {
            return false;
        }
        public static void SetGameLauncherMode(bool bLauncherMode) {
            return;
        }
        public static bool DismissFloatingGamepadTextInput() {
            return false;
        }
    }

    public static class SteamVideo {
        public static void GetVideoURL(AppId_t unVideoAppID) {
            return;
        }
        public static bool IsBroadcasting(out int pnNumViewers) {
            pnNumViewers = default;
            return false;
        }
        public static void GetOPFSettings(AppId_t unVideoAppID) {
            return;
        }
        public static bool GetOPFStringForApp(AppId_t unVideoAppID, out string pchBuffer, ref int pnBufferSize) {
            pchBuffer = default;
            return false;
        }
    }

    public static class SteamAPI {
        public static bool Init() {
            return true;
        }
        public static void Shutdown() {
            return;
        }
        public static bool RestartAppIfNecessary(AppId_t unOwnAppID) {
            return false;
        }
        public static void ReleaseCurrentThreadMemory() {
            return;
        }
        public static void RunCallbacks() {
            A2SPingQuery.RunCallbacks();
            NativeGameNetworkingSockets.RunCallbacks();
        }
        public static bool IsSteamRunning() {
            return false;
        }
        public static HSteamPipe GetHSteamPipe() {
            return default;
        }
        public static HSteamUser GetHSteamUser() {
            return default;
        }
    }

    public static class GameServer {
        public static bool Init(uint unIP, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
            return false;
        }
        public static void Shutdown() {
            return;
        }
        public static void RunCallbacks() {
            A2SPingQuery.RunCallbacks();
            NativeGameNetworkingSockets.RunCallbacks();
        }
        public static void ReleaseCurrentThreadMemory() {
            return;
        }
        public static bool BSecure() {
            return false;
        }
        public static CSteamID GetSteamID() {
            return new CSteamID(76561198000000000UL);
        }
        public static HSteamPipe GetHSteamPipe() {
            return default;
        }
        public static HSteamUser GetHSteamUser() {
            return default;
        }
    }

}