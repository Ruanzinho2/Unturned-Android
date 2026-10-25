using System;
using System.Collections;
using UnityEngine;
using Steamworks;

namespace SDG.Unturned
{
    public class MenuSteamLoginUI
    {
        private static SleekFullscreenBox container;
        public static bool active { get; private set; }

        private static ISleekBox backgroundBox;
        private static ISleekBox headerBox;
        private static ISleekLabel titleLabel;
        private static ISleekLabel subtitleLabel;

        private static ISleekBox centerContainer;

        // Left Card (Steam Guard QR)
        private static ISleekBox qrCardBox;
        private static ISleekLabel qrCardHeaderLabel;
        private static ISleekBox qrFrameBox;
        private static ISleekImage qrImage;
        private static ISleekLabel qrLoadingLabel;
        private static ISleekLabel qrInstructionsLabel;
        private static ISleekLabel qrStatusLabel;
        private static ISleekButton refreshQrButton;
        private static Texture2D currentQrTexture;

        // Right Card (Credentials & Actions)
        private static ISleekBox credentialsCardBox;
        private static ISleekLabel credentialsCardHeaderLabel;
        private static ISleekLabel usernameHeaderLabel;
        private static ISleekField usernameField;
        private static ISleekLabel passwordHeaderLabel;
        private static ISleekField passwordField;
        private static ISleekButton loginButton;
        private static ISleekLabel credentialsStatusLabel;
        private static ISleekButton savedAccountButton;
        private static ISleekButton offlineButton;
        private static ISleekButton exitButton;

        // Auto-login view
        private static ISleekBox autoLoginBox;
        private static ISleekLabel autoLoginTitleLabel;
        private static ISleekLabel autoLoginStatusLabel;
        private static ISleekButton autoLoginCancelButton;

        private static ISleekLabel footerLabel;

        private static Coroutine pendingTransitionCoroutine = null;

        public static void open()
        {
            if (active) return;
            active = true;

            showMainLoginView();
            container.AnimateIntoView();

            requestNewQrCode();
        }

        public static void openAutoLogin(string accountName, string refreshToken)
        {
            if (active) return;
            active = true;

            showAutoLoginView(accountName);
            container.AnimateIntoView();

            SteamBridge.LoginWithToken(
                accountName,
                refreshToken,
                onQrReady: null,
                onLoginSuccess: (steamId, name) =>
                {
                    autoLoginStatusLabel.Text = $"Conectado com sucesso! Bem-vindo de volta, {name}!";
                    autoLoginStatusLabel.TextColor = new SleekColor(new Color32(92, 191, 58, 255));
                    UnturnedLog.info($"[MenuSteamLoginUI] Auto-login succeeded for {name} ({steamId})");

                    if (MenuUI.instance != null)
                    {
                        if (pendingTransitionCoroutine != null)
                        {
                            MenuUI.instance.StopCoroutine(pendingTransitionCoroutine);
                        }
                        pendingTransitionCoroutine = MenuUI.instance.StartCoroutine(DelayedTransitionToMenu());
                    }
                    else
                    {
                        close();
                        MenuDashboardUI.open();
                        MenuTitleUI.open();
                        MenuDashboardUI.refreshProfile();
                    }
                },
                onLoginFailed: (error) =>
                {
                    UnturnedLog.warn($"[MenuSteamLoginUI] Auto-login failed: {error}. Falling back to main login view.");
                    showMainLoginView();
                    qrStatusLabel.Text = "Sessão anterior expirada. Por favor, escaneie o código QR:";
                    requestNewQrCode();
                }
            );
        }

        public static void close()
        {
            if (!active) return;
            active = false;

            if (pendingTransitionCoroutine != null && MenuUI.instance != null)
            {
                MenuUI.instance.StopCoroutine(pendingTransitionCoroutine);
                pendingTransitionCoroutine = null;
            }

            container.AnimateOutOfView(0, 1);

            if (currentQrTexture != null)
            {
                UnityEngine.Object.Destroy(currentQrTexture);
                currentQrTexture = null;
            }
        }

        public static void CheckStartupLogin()
        {
            SteamBridge.EnsureInitialized();

            string savedAccount = PlayerPrefs.GetString("SteamAccountName", "");
            string savedToken = PlayerPrefs.GetString("SteamRefreshToken", "");

            if (!string.IsNullOrEmpty(savedAccount) && !string.IsNullOrEmpty(savedToken))
            {
                UnturnedLog.info($"[MenuSteamLoginUI] Saved Steam session found for '{savedAccount}'. Initiating auto-login...");
                MenuDashboardUI.close();
                MenuTitleUI.close();
                openAutoLogin(savedAccount, savedToken);
            }
            else
            {
                UnturnedLog.info("[MenuSteamLoginUI] No saved Steam session found. Showing DroidDeck login screen...");
                MenuDashboardUI.close();
                MenuTitleUI.close();
                open();
            }
        }

        private static void showAutoLoginView(string accountName)
        {
            centerContainer.IsVisible = false;
            autoLoginBox.IsVisible = true;

            autoLoginTitleLabel.Text = $"INICIANDO SESSÃO NO STEAM";
            autoLoginStatusLabel.Text = $"Conectando como {accountName}...\nAguarde a validação dos servidores Steam.";
            autoLoginStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
        }

        private static void showMainLoginView()
        {
            autoLoginBox.IsVisible = false;
            centerContainer.IsVisible = true;

            string savedAccount = PlayerPrefs.GetString("SteamAccountName", "");
            if (!string.IsNullOrEmpty(savedAccount))
            {
                savedAccountButton.Text = $"Entrar como {savedAccount}";
                savedAccountButton.IsVisible = true;
            }
            else
            {
                savedAccountButton.IsVisible = false;
            }
        }

        private static void requestNewQrCode()
        {
            qrLoadingLabel.IsVisible = true;
            qrImage.IsVisible = false;
            qrStatusLabel.Text = "Conectando à rede Steam...";
            qrStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));

            SteamBridge.StartQrLogin(
                onQrReady: (url, texture) =>
                {
                    if (texture != null)
                    {
                        if (currentQrTexture != null)
                        {
                            UnityEngine.Object.Destroy(currentQrTexture);
                        }
                        currentQrTexture = texture;
                        qrImage.Texture = currentQrTexture;
                        qrImage.IsVisible = true;
                        qrLoadingLabel.IsVisible = false;
                        qrStatusLabel.Text = "Aguardando confirmação no app Steam...";
                        qrStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
                    }
                },
                onLoginSuccess: (steamId, accountName) =>
                {
                    qrStatusLabel.Text = $"Sessão autorizada! Bem-vindo, {accountName}!";
                    qrStatusLabel.TextColor = new SleekColor(new Color32(92, 191, 58, 255));
                    UnturnedLog.info($"[MenuSteamLoginUI] QR Login successful: {accountName} ({steamId})");

                    if (MenuUI.instance != null)
                    {
                        if (pendingTransitionCoroutine != null)
                        {
                            MenuUI.instance.StopCoroutine(pendingTransitionCoroutine);
                        }
                        pendingTransitionCoroutine = MenuUI.instance.StartCoroutine(DelayedTransitionToMenu());
                    }
                    else
                    {
                        close();
                        MenuDashboardUI.open();
                        MenuTitleUI.open();
                        MenuDashboardUI.refreshProfile();
                    }
                },
                onLoginFailed: (error) =>
                {
                    qrStatusLabel.Text = $"Erro: {error}";
                    qrStatusLabel.TextColor = new SleekColor(new Color32(255, 76, 76, 255));
                }
            );
        }

        private static IEnumerator DelayedTransitionToMenu()
        {
            yield return new WaitForSeconds(0.7f);
            close();
            MenuDashboardUI.open();
            MenuTitleUI.open();
            MenuDashboardUI.refreshProfile();
            pendingTransitionCoroutine = null;
        }

        private static void onClickedRefreshQr(ISleekElement button)
        {
            requestNewQrCode();
        }

        private static void onClickedLoginButton(ISleekElement button)
        {
            string username = usernameField.Text?.Trim();
            string password = passwordField.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                credentialsStatusLabel.Text = "Preencha o nome de usuário e a senha.";
                credentialsStatusLabel.TextColor = new SleekColor(new Color32(255, 76, 76, 255));
                return;
            }

            credentialsStatusLabel.Text = "Autenticando na Steam...";
            credentialsStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));

            SteamBridge.LoginWithCredentials(
                username,
                password,
                onLoginSuccess: (steamId, accountName) =>
                {
                    credentialsStatusLabel.Text = $"Conectado com sucesso como {accountName}!";
                    credentialsStatusLabel.TextColor = new SleekColor(new Color32(92, 191, 58, 255));

                    if (MenuUI.instance != null)
                    {
                        if (pendingTransitionCoroutine != null)
                        {
                            MenuUI.instance.StopCoroutine(pendingTransitionCoroutine);
                        }
                        pendingTransitionCoroutine = MenuUI.instance.StartCoroutine(DelayedTransitionToMenu());
                    }
                    else
                    {
                        close();
                        MenuDashboardUI.open();
                        MenuTitleUI.open();
                        MenuDashboardUI.refreshProfile();
                    }
                },
                onLoginFailed: (error) =>
                {
                    credentialsStatusLabel.Text = error;
                    credentialsStatusLabel.TextColor = new SleekColor(new Color32(255, 76, 76, 255));
                }
            );
        }

        private static void onClickedSavedAccount(ISleekElement button)
        {
            string savedAccount = PlayerPrefs.GetString("SteamAccountName", "");
            string savedToken = PlayerPrefs.GetString("SteamRefreshToken", "");
            if (!string.IsNullOrEmpty(savedAccount) && !string.IsNullOrEmpty(savedToken))
            {
                openAutoLogin(savedAccount, savedToken);
            }
        }

        private static void onClickedOffline(ISleekElement button)
        {
            close();
            MenuDashboardUI.open();
            MenuTitleUI.open();
            MenuDashboardUI.refreshProfile();
        }

        private static void onClickedExit(ISleekElement button)
        {
            Application.Quit();
        }

        private static void onClickedCancelAutoLogin(ISleekElement button)
        {
            showMainLoginView();
            requestNewQrCode();
        }

        public MenuSteamLoginUI()
        {
            container = new SleekFullscreenBox();
            container.PositionOffset_X = 0;
            container.PositionOffset_Y = 0;
            container.SizeScale_X = 1;
            container.SizeScale_Y = 1;
            MenuUI.container.AddChild(container);
            active = false;

            // Full-screen deep slate background (SteamOS/DroidDeck aesthetic)
            backgroundBox = Glazier.Get().CreateBox();
            backgroundBox.SizeScale_X = 1;
            backgroundBox.SizeScale_Y = 1;
            backgroundBox.BackgroundColor = new SleekColor(new Color32(14, 20, 27, 255));
            container.AddChild(backgroundBox);

            // Top Header
            headerBox = Glazier.Get().CreateBox();
            headerBox.PositionScale_X = 0.5f;
            headerBox.PositionOffset_X = -450;
            headerBox.PositionOffset_Y = 20;
            headerBox.SizeOffset_X = 900;
            headerBox.SizeOffset_Y = 65;
            headerBox.BackgroundColor = new SleekColor(new Color32(23, 31, 44, 200));
            container.AddChild(headerBox);

            titleLabel = Glazier.Get().CreateLabel();
            titleLabel.PositionOffset_Y = 6;
            titleLabel.SizeScale_X = 1;
            titleLabel.SizeOffset_Y = 30;
            titleLabel.Text = "INICIAR SESSÃO NO STEAM";
            titleLabel.FontSize = ESleekFontSize.Large;
            titleLabel.TextColor = new SleekColor(new Color32(229, 229, 229, 255));
            titleLabel.TextAlignment = TextAnchor.MiddleCenter;
            headerBox.AddChild(titleLabel);

            subtitleLabel = Glazier.Get().CreateLabel();
            subtitleLabel.PositionOffset_Y = 36;
            subtitleLabel.SizeScale_X = 1;
            subtitleLabel.SizeOffset_Y = 22;
            subtitleLabel.Text = "Use o aplicativo móvel do Steam para ler o código QR ou entre com suas credenciais";
            subtitleLabel.FontSize = ESleekFontSize.Small;
            subtitleLabel.TextColor = new SleekColor(new Color32(143, 152, 160, 255));
            subtitleLabel.TextAlignment = TextAnchor.MiddleCenter;
            headerBox.AddChild(subtitleLabel);

            // Center Two-Column Container (Width 880, Height 460)
            centerContainer = Glazier.Get().CreateBox();
            centerContainer.PositionScale_X = 0.5f;
            centerContainer.PositionScale_Y = 0.5f;
            centerContainer.PositionOffset_X = -440;
            centerContainer.PositionOffset_Y = -215;
            centerContainer.SizeOffset_X = 880;
            centerContainer.SizeOffset_Y = 460;
            centerContainer.BackgroundColor = new SleekColor(new Color32(0, 0, 0, 0)); // transparent
            container.AddChild(centerContainer);

            // ----------------------------------------------------
            // LEFT CARD: STEAM GUARD QR (Width 425, Height 460)
            // ----------------------------------------------------
            qrCardBox = Glazier.Get().CreateBox();
            qrCardBox.PositionOffset_X = 0;
            qrCardBox.PositionOffset_Y = 0;
            qrCardBox.SizeOffset_X = 425;
            qrCardBox.SizeOffset_Y = 460;
            qrCardBox.BackgroundColor = new SleekColor(new Color32(23, 31, 44, 255));
            centerContainer.AddChild(qrCardBox);

            qrCardHeaderLabel = Glazier.Get().CreateLabel();
            qrCardHeaderLabel.PositionOffset_Y = 12;
            qrCardHeaderLabel.SizeScale_X = 1;
            qrCardHeaderLabel.SizeOffset_Y = 25;
            qrCardHeaderLabel.Text = "CÓDIGO QR DO STEAM GUARD";
            qrCardHeaderLabel.FontSize = ESleekFontSize.Medium;
            qrCardHeaderLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
            qrCardHeaderLabel.TextAlignment = TextAnchor.MiddleCenter;
            qrCardBox.AddChild(qrCardHeaderLabel);

            // High-contrast white QR frame
            qrFrameBox = Glazier.Get().CreateBox();
            qrFrameBox.PositionScale_X = 0.5f;
            qrFrameBox.PositionOffset_X = -120;
            qrFrameBox.PositionOffset_Y = 45;
            qrFrameBox.SizeOffset_X = 240;
            qrFrameBox.SizeOffset_Y = 240;
            qrFrameBox.BackgroundColor = new SleekColor(Color.white);
            qrCardBox.AddChild(qrFrameBox);

            qrLoadingLabel = Glazier.Get().CreateLabel();
            qrLoadingLabel.SizeScale_X = 1;
            qrLoadingLabel.SizeScale_Y = 1;
            qrLoadingLabel.Text = "Gerando QR Code...";
            qrLoadingLabel.FontSize = ESleekFontSize.Small;
            qrLoadingLabel.TextColor = new SleekColor(Color.black);
            qrLoadingLabel.TextAlignment = TextAnchor.MiddleCenter;
            qrFrameBox.AddChild(qrLoadingLabel);

            qrImage = Glazier.Get().CreateImage();
            qrImage.PositionScale_X = 0.5f;
            qrImage.PositionScale_Y = 0.5f;
            qrImage.PositionOffset_X = -110;
            qrImage.PositionOffset_Y = -110;
            qrImage.SizeOffset_X = 220;
            qrImage.SizeOffset_Y = 220;
            qrImage.IsVisible = false;
            qrFrameBox.AddChild(qrImage);

            qrInstructionsLabel = Glazier.Get().CreateLabel();
            qrInstructionsLabel.PositionOffset_X = 20;
            qrInstructionsLabel.PositionOffset_Y = 295;
            qrInstructionsLabel.SizeOffset_X = 385;
            qrInstructionsLabel.SizeOffset_Y = 65;
            qrInstructionsLabel.Text = "1. Abra o aplicativo móvel do Steam no celular\n2. Vá no Steam Guard > Toque em \"Escanear código QR\"\n3. Aponte a câmera para o código acima para autorizar";
            qrInstructionsLabel.FontSize = ESleekFontSize.Small;
            qrInstructionsLabel.TextColor = new SleekColor(new Color32(198, 212, 223, 255));
            qrInstructionsLabel.TextAlignment = TextAnchor.UpperLeft;
            qrCardBox.AddChild(qrInstructionsLabel);

            qrStatusLabel = Glazier.Get().CreateLabel();
            qrStatusLabel.PositionOffset_X = 20;
            qrStatusLabel.PositionOffset_Y = 365;
            qrStatusLabel.SizeOffset_X = 385;
            qrStatusLabel.SizeOffset_Y = 25;
            qrStatusLabel.Text = "Aguardando leitura no Steam Guard...";
            qrStatusLabel.FontSize = ESleekFontSize.Small;
            qrStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
            qrStatusLabel.TextAlignment = TextAnchor.MiddleCenter;
            qrCardBox.AddChild(qrStatusLabel);

            refreshQrButton = Glazier.Get().CreateButton();
            refreshQrButton.PositionOffset_X = 20;
            refreshQrButton.PositionOffset_Y = 400;
            refreshQrButton.SizeOffset_X = 385;
            refreshQrButton.SizeOffset_Y = 44;
            refreshQrButton.Text = "Atualizar Código QR";
            refreshQrButton.FontSize = ESleekFontSize.Medium;
            refreshQrButton.BackgroundColor = new SleekColor(new Color32(33, 44, 61, 255));
            refreshQrButton.TextColor = new SleekColor(new Color32(220, 222, 223, 255));
            refreshQrButton.OnClicked += onClickedRefreshQr;
            qrCardBox.AddChild(refreshQrButton);

            // ----------------------------------------------------
            // RIGHT CARD: CREDENTIALS & OPTIONS (Width 425, Height 460)
            // ----------------------------------------------------
            credentialsCardBox = Glazier.Get().CreateBox();
            credentialsCardBox.PositionOffset_X = 455;
            credentialsCardBox.PositionOffset_Y = 0;
            credentialsCardBox.SizeOffset_X = 425;
            credentialsCardBox.SizeOffset_Y = 460;
            credentialsCardBox.BackgroundColor = new SleekColor(new Color32(23, 31, 44, 255));
            centerContainer.AddChild(credentialsCardBox);

            credentialsCardHeaderLabel = Glazier.Get().CreateLabel();
            credentialsCardHeaderLabel.PositionOffset_Y = 12;
            credentialsCardHeaderLabel.SizeScale_X = 1;
            credentialsCardHeaderLabel.SizeOffset_Y = 25;
            credentialsCardHeaderLabel.Text = "ENTRAR COM CREDENCIAIS";
            credentialsCardHeaderLabel.FontSize = ESleekFontSize.Medium;
            credentialsCardHeaderLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
            credentialsCardHeaderLabel.TextAlignment = TextAnchor.MiddleCenter;
            credentialsCardBox.AddChild(credentialsCardHeaderLabel);

            usernameHeaderLabel = Glazier.Get().CreateLabel();
            usernameHeaderLabel.PositionOffset_X = 20;
            usernameHeaderLabel.PositionOffset_Y = 42;
            usernameHeaderLabel.SizeOffset_X = 385;
            usernameHeaderLabel.SizeOffset_Y = 20;
            usernameHeaderLabel.Text = "Nome da conta Steam:";
            usernameHeaderLabel.FontSize = ESleekFontSize.Small;
            usernameHeaderLabel.TextColor = new SleekColor(new Color32(143, 152, 160, 255));
            usernameHeaderLabel.TextAlignment = TextAnchor.MiddleLeft;
            credentialsCardBox.AddChild(usernameHeaderLabel);

            usernameField = Glazier.Get().CreateStringField();
            usernameField.PositionOffset_X = 20;
            usernameField.PositionOffset_Y = 64;
            usernameField.SizeOffset_X = 385;
            usernameField.SizeOffset_Y = 38;
            usernameField.PlaceholderText = "Nome de usuário da Steam";
            usernameField.BackgroundColor = new SleekColor(new Color32(16, 24, 34, 255));
            usernameField.TextColor = new SleekColor(Color.white);
            credentialsCardBox.AddChild(usernameField);

            passwordHeaderLabel = Glazier.Get().CreateLabel();
            passwordHeaderLabel.PositionOffset_X = 20;
            passwordHeaderLabel.PositionOffset_Y = 108;
            passwordHeaderLabel.SizeOffset_X = 385;
            passwordHeaderLabel.SizeOffset_Y = 20;
            passwordHeaderLabel.Text = "Senha da conta:";
            passwordHeaderLabel.FontSize = ESleekFontSize.Small;
            passwordHeaderLabel.TextColor = new SleekColor(new Color32(143, 152, 160, 255));
            passwordHeaderLabel.TextAlignment = TextAnchor.MiddleLeft;
            credentialsCardBox.AddChild(passwordHeaderLabel);

            passwordField = Glazier.Get().CreateStringField();
            passwordField.PositionOffset_X = 20;
            passwordField.PositionOffset_Y = 130;
            passwordField.SizeOffset_X = 385;
            passwordField.SizeOffset_Y = 38;
            passwordField.IsPasswordField = true;
            passwordField.PlaceholderText = "Senha";
            passwordField.BackgroundColor = new SleekColor(new Color32(16, 24, 34, 255));
            passwordField.TextColor = new SleekColor(Color.white);
            credentialsCardBox.AddChild(passwordField);

            loginButton = Glazier.Get().CreateButton();
            loginButton.PositionOffset_X = 20;
            loginButton.PositionOffset_Y = 178;
            loginButton.SizeOffset_X = 385;
            loginButton.SizeOffset_Y = 44;
            loginButton.Text = "INICIAR SESSÃO";
            loginButton.FontSize = ESleekFontSize.Medium;
            loginButton.BackgroundColor = new SleekColor(new Color32(26, 159, 255, 255)); // Steam Accent Blue
            loginButton.TextColor = new SleekColor(Color.white);
            loginButton.OnClicked += onClickedLoginButton;
            credentialsCardBox.AddChild(loginButton);

            credentialsStatusLabel = Glazier.Get().CreateLabel();
            credentialsStatusLabel.PositionOffset_X = 20;
            credentialsStatusLabel.PositionOffset_Y = 226;
            credentialsStatusLabel.SizeOffset_X = 385;
            credentialsStatusLabel.SizeOffset_Y = 34;
            credentialsStatusLabel.Text = "";
            credentialsStatusLabel.FontSize = ESleekFontSize.Small;
            credentialsStatusLabel.TextColor = new SleekColor(new Color32(143, 152, 160, 255));
            credentialsStatusLabel.TextAlignment = TextAnchor.MiddleCenter;
            credentialsCardBox.AddChild(credentialsStatusLabel);

            savedAccountButton = Glazier.Get().CreateButton();
            savedAccountButton.PositionOffset_X = 20;
            savedAccountButton.PositionOffset_Y = 265;
            savedAccountButton.SizeOffset_X = 385;
            savedAccountButton.SizeOffset_Y = 40;
            savedAccountButton.Text = "Entrar com a conta salva";
            savedAccountButton.FontSize = ESleekFontSize.Small;
            savedAccountButton.BackgroundColor = new SleekColor(new Color32(34, 51, 72, 255));
            savedAccountButton.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
            savedAccountButton.OnClicked += onClickedSavedAccount;
            savedAccountButton.IsVisible = false;
            credentialsCardBox.AddChild(savedAccountButton);

            offlineButton = Glazier.Get().CreateButton();
            offlineButton.PositionOffset_X = 20;
            offlineButton.PositionOffset_Y = 350;
            offlineButton.SizeOffset_X = 385;
            offlineButton.SizeOffset_Y = 44;
            offlineButton.Text = "JOGAR OFFLINE (MODO CONVIDADO)";
            offlineButton.FontSize = ESleekFontSize.Small;
            offlineButton.BackgroundColor = new SleekColor(new Color32(33, 44, 61, 255));
            offlineButton.TextColor = new SleekColor(new Color32(220, 222, 223, 255));
            offlineButton.OnClicked += onClickedOffline;
            credentialsCardBox.AddChild(offlineButton);

            exitButton = Glazier.Get().CreateButton();
            exitButton.PositionOffset_X = 20;
            exitButton.PositionOffset_Y = 400;
            exitButton.SizeOffset_X = 385;
            exitButton.SizeOffset_Y = 44;
            exitButton.Text = "SAIR DO JOGO";
            exitButton.FontSize = ESleekFontSize.Small;
            exitButton.BackgroundColor = new SleekColor(new Color32(25, 33, 44, 255));
            exitButton.TextColor = new SleekColor(new Color32(180, 180, 180, 255));
            exitButton.OnClicked += onClickedExit;
            credentialsCardBox.AddChild(exitButton);

            // ----------------------------------------------------
            // AUTO-LOGIN OVERLAY CARD (Width 520, Height 260)
            // ----------------------------------------------------
            autoLoginBox = Glazier.Get().CreateBox();
            autoLoginBox.PositionScale_X = 0.5f;
            autoLoginBox.PositionScale_Y = 0.5f;
            autoLoginBox.PositionOffset_X = -260;
            autoLoginBox.PositionOffset_Y = -130;
            autoLoginBox.SizeOffset_X = 520;
            autoLoginBox.SizeOffset_Y = 260;
            autoLoginBox.BackgroundColor = new SleekColor(new Color32(23, 31, 44, 255));
            autoLoginBox.IsVisible = false;
            container.AddChild(autoLoginBox);

            autoLoginTitleLabel = Glazier.Get().CreateLabel();
            autoLoginTitleLabel.PositionOffset_Y = 25;
            autoLoginTitleLabel.SizeScale_X = 1;
            autoLoginTitleLabel.SizeOffset_Y = 30;
            autoLoginTitleLabel.Text = "INICIANDO SESSÃO NO STEAM";
            autoLoginTitleLabel.FontSize = ESleekFontSize.Large;
            autoLoginTitleLabel.TextColor = new SleekColor(new Color32(229, 229, 229, 255));
            autoLoginTitleLabel.TextAlignment = TextAnchor.MiddleCenter;
            autoLoginBox.AddChild(autoLoginTitleLabel);

            autoLoginStatusLabel = Glazier.Get().CreateLabel();
            autoLoginStatusLabel.PositionOffset_X = 30;
            autoLoginStatusLabel.PositionOffset_Y = 70;
            autoLoginStatusLabel.SizeOffset_X = 460;
            autoLoginStatusLabel.SizeOffset_Y = 80;
            autoLoginStatusLabel.Text = "Conectando ao Steam...";
            autoLoginStatusLabel.FontSize = ESleekFontSize.Medium;
            autoLoginStatusLabel.TextColor = new SleekColor(new Color32(102, 192, 244, 255));
            autoLoginStatusLabel.TextAlignment = TextAnchor.MiddleCenter;
            autoLoginBox.AddChild(autoLoginStatusLabel);

            autoLoginCancelButton = Glazier.Get().CreateButton();
            autoLoginCancelButton.PositionScale_X = 0.5f;
            autoLoginCancelButton.PositionOffset_X = -130;
            autoLoginCancelButton.PositionOffset_Y = 175;
            autoLoginCancelButton.SizeOffset_X = 260;
            autoLoginCancelButton.SizeOffset_Y = 44;
            autoLoginCancelButton.Text = "Entrar com outra conta";
            autoLoginCancelButton.FontSize = ESleekFontSize.Medium;
            autoLoginCancelButton.BackgroundColor = new SleekColor(new Color32(33, 44, 61, 255));
            autoLoginCancelButton.TextColor = new SleekColor(new Color32(220, 222, 223, 255));
            autoLoginCancelButton.OnClicked += onClickedCancelAutoLogin;
            autoLoginBox.AddChild(autoLoginCancelButton);

            // Bottom Footer
            footerLabel = Glazier.Get().CreateLabel();
            footerLabel.PositionScale_X = 0.5f;
            footerLabel.PositionScale_Y = 1f;
            footerLabel.PositionOffset_X = -350;
            footerLabel.PositionOffset_Y = -35;
            footerLabel.SizeOffset_X = 700;
            footerLabel.SizeOffset_Y = 25;
            footerLabel.Text = "Unturned Android Native • SteamOS & DroidDeck Integration";
            footerLabel.FontSize = ESleekFontSize.Small;
            footerLabel.TextColor = new SleekColor(new Color32(79, 90, 101, 255));
            footerLabel.TextAlignment = TextAnchor.MiddleCenter;
            container.AddChild(footerLabel);
        }
    }
}
