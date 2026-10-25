package com.smartlydressedgames.unturned;

import android.graphics.Bitmap;
import android.graphics.Color;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.google.zxing.BarcodeFormat;
import com.google.zxing.common.BitMatrix;
import com.google.zxing.qrcode.QRCodeWriter;

import in.dragonbra.javasteam.enums.EResult;
import in.dragonbra.javasteam.steam.authentication.AuthPollResult;
import in.dragonbra.javasteam.steam.authentication.AuthSessionDetails;
import in.dragonbra.javasteam.steam.authentication.QrAuthSession;
import in.dragonbra.javasteam.steam.authentication.SteamAuthentication;
import in.dragonbra.javasteam.steam.handlers.steamauthticket.SteamAuthTicket;
import in.dragonbra.javasteam.steam.handlers.steamauthticket.TicketInfo;
import in.dragonbra.javasteam.steam.handlers.steamuser.LogOnDetails;
import in.dragonbra.javasteam.steam.handlers.steamuser.SteamUser;
import in.dragonbra.javasteam.steam.handlers.steamuser.callback.LoggedOffCallback;
import in.dragonbra.javasteam.steam.handlers.steamuser.callback.LoggedOnCallback;
import in.dragonbra.javasteam.steam.steamclient.SteamClient;
import in.dragonbra.javasteam.steam.steamclient.callbackmgr.CallbackManager;
import in.dragonbra.javasteam.steam.steamclient.callbacks.ConnectedCallback;
import in.dragonbra.javasteam.steam.steamclient.callbacks.DisconnectedCallback;
import in.dragonbra.javasteam.steam.steamclient.configuration.SteamConfiguration;
import in.dragonbra.javasteam.types.SteamID;

import java.io.ByteArrayOutputStream;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;

public class SteamBridge {
    private static final String TAG = "UnturnedSteamBridge";

    public interface SteamLoginCallback {
        void onQrReady(String url, byte[] pngBytes);
        void onLoginSuccess(long steamId, String accountName, String refreshToken);
        void onLoginFailed(String error);
    }

    private static SteamClient sClient;
    private static CallbackManager sCallbackMgr;
    private static SteamConfiguration sConfiguration;
    private static SteamAuthentication sAuthentication;
    private static QrAuthSession sCurrentQrSession;
    private static SteamLoginCallback sCurrentCallback;

    private static volatile boolean sIsConnected = false;
    private static volatile boolean sIsLoggedOn = false;
    private static volatile long sSteamId = 0;
    private static volatile String sAccountName = "";
    private static volatile String sRefreshToken = "";

    private static final Handler sMainHandler = new Handler(Looper.getMainLooper());
    private static final AtomicBoolean sWorkerStarted = new AtomicBoolean(false);

    public static synchronized void init() {
        if (sClient != null) return;
        try {
            sConfiguration = SteamConfiguration.createDefault();
            sClient = new SteamClient(sConfiguration);
            sCallbackMgr = new CallbackManager(sClient);

            sCallbackMgr.subscribe(ConnectedCallback.class, cb -> {
                Log.i(TAG, "SteamClient Connected to CM.");
                sIsConnected = true;
            });

            sCallbackMgr.subscribe(DisconnectedCallback.class, cb -> {
                Log.i(TAG, "SteamClient Disconnected (userInitiated=" + cb.isUserInitiated() + ")");
                sIsConnected = false;
                sIsLoggedOn = false;
            });

            sCallbackMgr.subscribe(LoggedOnCallback.class, cb -> {
                if (cb.getResult() == EResult.OK) {
                    sIsLoggedOn = true;
                    if (cb.getClientSteamID() != null) {
                        sSteamId = cb.getClientSteamID().convertToUInt64();
                    }
                    Log.i(TAG, "SteamClient LoggedOn successfully! SteamID: " + sSteamId);
                } else {
                    sIsLoggedOn = false;
                    Log.w(TAG, "SteamClient LoggedOn failed: " + cb.getResult());
                }
            });

            sCallbackMgr.subscribe(LoggedOffCallback.class, cb -> {
                Log.i(TAG, "SteamClient LoggedOff: " + cb.getResult());
                sIsLoggedOn = false;
            });

            if (sWorkerStarted.compareAndSet(false, true)) {
                Thread callbackThread = new Thread(() -> {
                    while (true) {
                        try {
                            if (sCallbackMgr != null) {
                                sCallbackMgr.runWaitCallbacks(500);
                            } else {
                                Thread.sleep(500);
                            }
                        } catch (Exception ex) {
                            // continue loop
                        }
                    }
                }, "JavaSteamCallbackThread");
                callbackThread.setDaemon(true);
                callbackThread.start();
            }

            Log.i(TAG, "SteamBridge initialized successfully.");
        } catch (Exception ex) {
            Log.e(TAG, "Error initializing SteamBridge: " + ex.getMessage(), ex);
        }
    }

    public static void startQrLogin(final SteamLoginCallback callback) {
        init();
        sCurrentCallback = callback;

        new Thread(() -> {
            try {
                if (!sClient.isConnected()) {
                    Log.i(TAG, "Connecting to Steam network...");
                    sClient.connect();
                    long start = System.currentTimeMillis();
                    while (!sClient.isConnected() && (System.currentTimeMillis() - start < 10000)) {
                        Thread.sleep(100);
                    }
                    if (!sClient.isConnected()) {
                        notifyFailed("Falha ao conectar aos servidores da Steam (timeout).");
                        return;
                    }
                }

                sAuthentication = new SteamAuthentication(sClient);
                AuthSessionDetails details = new AuthSessionDetails();
                details.deviceFriendlyName = "Unturned Android";

                CompletableFuture<QrAuthSession> future = sAuthentication.beginAuthSessionViaQR(details);
                sCurrentQrSession = future.get(15, TimeUnit.SECONDS);

                String challengeUrl = sCurrentQrSession.getChallengeUrl();
                Log.i(TAG, "QR Auth Session started. URL: " + challengeUrl);

                byte[] pngBytes = generateQrPng(challengeUrl, 384, 384);
                notifyQrReady(challengeUrl, pngBytes);

                sCurrentQrSession.setChallengeUrlChanged(session -> {
                    try {
                        String newUrl = session.getChallengeUrl();
                        Log.i(TAG, "QR Challenge URL refreshed: " + newUrl);
                        byte[] newPng = generateQrPng(newUrl, 384, 384);
                        notifyQrReady(newUrl, newPng);
                    } catch (Exception e) {
                        Log.w(TAG, "Error handling refreshed QR: " + e.getMessage());
                    }
                });

                Log.i(TAG, "Waiting for user to scan QR in Steam Guard...");
                AuthPollResult pollResult = sCurrentQrSession.pollingWaitForResult().get();

                if (pollResult != null) {
                    sAccountName = pollResult.getAccountName();
                    sRefreshToken = pollResult.getRefreshToken();
                    Log.i(TAG, "Steam QR Login approved for account: " + sAccountName);

                    // Log on to SteamClient with the new refresh token
                    LogOnDetails logOnDetails = new LogOnDetails();
                    logOnDetails.setUsername(sAccountName);
                    logOnDetails.setAccessToken(sRefreshToken);
                    sClient.getHandler(SteamUser.class).logOn(logOnDetails);

                    long logOnStart = System.currentTimeMillis();
                    while (!sIsLoggedOn && (System.currentTimeMillis() - logOnStart < 10000)) {
                        Thread.sleep(100);
                    }

                    notifySuccess(sSteamId, sAccountName, sRefreshToken);
                } else {
                    notifyFailed("Sessão de login expirada ou rejeitada.");
                }

            } catch (Exception ex) {
                Log.e(TAG, "QR Login error: " + ex.getMessage(), ex);
                notifyFailed("Erro durante login: " + ex.getMessage());
            }
        }, "JavaSteamQrThread").start();
    }

    public static void loginWithToken(final String accountName, final String refreshToken, final SteamLoginCallback callback) {
        init();
        sCurrentCallback = callback;

        new Thread(() -> {
            try {
                if (!sClient.isConnected()) {
                    sClient.connect();
                    long start = System.currentTimeMillis();
                    while (!sClient.isConnected() && (System.currentTimeMillis() - start < 10000)) {
                        Thread.sleep(100);
                    }
                    if (!sClient.isConnected()) {
                        notifyFailed("Timeout ao conectar com a Steam.");
                        return;
                    }
                }

                sAccountName = accountName;
                sRefreshToken = refreshToken;

                LogOnDetails details = new LogOnDetails();
                details.setUsername(accountName);
                details.setAccessToken(refreshToken);
                sClient.getHandler(SteamUser.class).logOn(details);

                long start = System.currentTimeMillis();
                while (!sIsLoggedOn && (System.currentTimeMillis() - start < 10000)) {
                    Thread.sleep(100);
                }

                if (sIsLoggedOn) {
                    notifySuccess(sSteamId, sAccountName, sRefreshToken);
                } else {
                    notifyFailed("Token de login expirado. Por favor, escaneie o QR Code novamente.");
                }
            } catch (Exception ex) {
                notifyFailed("Falha no login com token: " + ex.getMessage());
            }
        }, "JavaSteamTokenLoginThread").start();
    }

    public static byte[] getAuthSessionTicket(int appId) {
        if (!sIsLoggedOn || sClient == null) {
            Log.w(TAG, "Cannot getAuthSessionTicket: SteamClient not logged on!");
            return null;
        }

        try {
            SteamAuthTicket authTicketHandler = sClient.getHandler(SteamAuthTicket.class);
            if (authTicketHandler != null) {
                TicketInfo info = authTicketHandler.getAuthSessionTicket(appId).get(10, TimeUnit.SECONDS);
                if (info != null && info.getTicket() != null) {
                    Log.i(TAG, "Successfully generated AuthSessionTicket for AppID " + appId + " (size: " + info.getTicket().length + " bytes)");
                    return info.getTicket();
                }
            }
        } catch (Exception ex) {
            Log.e(TAG, "Error generating AuthSessionTicket: " + ex.getMessage(), ex);
        }
        return null;
    }

    public static boolean isLoggedOn() {
        return sIsLoggedOn;
    }

    public static long getSteamId() {
        return sSteamId;
    }

    public static String getAccountName() {
        return sAccountName;
    }

    public static void logOff() {
        if (sClient != null && sIsLoggedOn) {
            try {
                sClient.getHandler(SteamUser.class).logOff();
            } catch (Exception ignored) {}
        }
        sIsLoggedOn = false;
        sSteamId = 0;
        sAccountName = "";
        sRefreshToken = "";
    }

    private static byte[] generateQrPng(String text, int width, int height) throws Exception {
        QRCodeWriter writer = new QRCodeWriter();
        BitMatrix matrix = writer.encode(text, BarcodeFormat.QR_CODE, width, height);

        int matrixWidth = matrix.getWidth();
        int matrixHeight = matrix.getHeight();
        Bitmap bitmap = Bitmap.createBitmap(matrixWidth, matrixHeight, Bitmap.Config.ARGB_8888);
        for (int x = 0; x < matrixWidth; x++) {
            for (int y = 0; y < matrixHeight; y++) {
                bitmap.setPixel(x, y, matrix.get(x, y) ? Color.BLACK : Color.WHITE);
            }
        }

        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, baos);
        return baos.toByteArray();
    }

    private static void notifyQrReady(String url, byte[] pngBytes) {
        if (sCurrentCallback != null) {
            sMainHandler.post(() -> sCurrentCallback.onQrReady(url, pngBytes));
        }
    }

    private static void notifySuccess(long steamId, String accountName, String refreshToken) {
        if (sCurrentCallback != null) {
            sMainHandler.post(() -> sCurrentCallback.onLoginSuccess(steamId, accountName, refreshToken));
        }
    }

    private static void notifyFailed(String error) {
        if (sCurrentCallback != null) {
            sMainHandler.post(() -> sCurrentCallback.onLoginFailed(error));
        }
    }
}
