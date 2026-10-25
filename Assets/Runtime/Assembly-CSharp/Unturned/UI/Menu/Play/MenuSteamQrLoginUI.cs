using System;
using UnityEngine;
using Steamworks;

namespace SDG.Unturned
{
    public class MenuSteamQrLoginUI
    {
        private static SleekFullscreenBox container;
        public static bool isActive { get; private set; }

        private static ISleekBox modalBox;
        private static ISleekLabel titleLabel;
        private static ISleekLabel instructionsLabel;
        private static ISleekImage qrImage;
        private static ISleekLabel statusLabel;
        private static ISleekButton closeButton;
        private static Texture2D currentQrTexture;

        public static void open()
        {
            if (isActive) return;
            isActive = true;

            statusLabel.Text = "Conectando aos servidores da Steam...";
            qrImage.IsVisible = false;

            container.AnimateIntoView();

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
                        statusLabel.Text = "Aponte a câmera do Steam Guard para o QR Code:";
                    }
                },
                onLoginSuccess: (steamId, accountName) =>
                {
                    statusLabel.Text = $"Conectado com sucesso! Bem-vindo, {accountName}!";
                    MenuPlayConnectUI.UpdateConfiguredFields(steamId, accountName);
                    close();
                },
                onLoginFailed: (error) =>
                {
                    statusLabel.Text = $"Erro: {error}";
                }
            );
        }

        public static void close()
        {
            if (!isActive) return;
            isActive = false;

            container.AnimateOutOfView(0, 1);

            if (currentQrTexture != null)
            {
                UnityEngine.Object.Destroy(currentQrTexture);
                currentQrTexture = null;
            }
        }

        private static void onClickedCloseButton(ISleekElement button)
        {
            close();
        }

        public MenuSteamQrLoginUI()
        {
            container = new SleekFullscreenBox();
            container.PositionOffset_X = 10;
            container.PositionOffset_Y = 10;
            container.PositionScale_Y = 1;
            container.SizeOffset_X = -20;
            container.SizeOffset_Y = -20;
            container.SizeScale_X = 1;
            container.SizeScale_Y = 1;
            MenuUI.container.AddChild(container);

            modalBox = Glazier.Get().CreateBox();
            modalBox.PositionScale_X = 0.5f;
            modalBox.PositionScale_Y = 0.5f;
            modalBox.PositionOffset_X = -210;
            modalBox.PositionOffset_Y = -230;
            modalBox.SizeOffset_X = 420;
            modalBox.SizeOffset_Y = 460;
            container.AddChild(modalBox);

            titleLabel = Glazier.Get().CreateLabel();
            titleLabel.PositionOffset_Y = 15;
            titleLabel.SizeOffset_Y = 30;
            titleLabel.SizeScale_X = 1;
            titleLabel.Text = "Login com Steam (Steam Guard QR)";
            titleLabel.FontSize = ESleekFontSize.Large;
            modalBox.AddChild(titleLabel);

            instructionsLabel = Glazier.Get().CreateLabel();
            instructionsLabel.PositionOffset_Y = 45;
            instructionsLabel.SizeOffset_Y = 30;
            instructionsLabel.SizeScale_X = 1;
            instructionsLabel.Text = "Abra o Steam Mobile no celular > Steam Guard > Escanear QR";
            instructionsLabel.FontSize = ESleekFontSize.Small;
            instructionsLabel.TextColor = Color.gray;
            modalBox.AddChild(instructionsLabel);

            qrImage = Glazier.Get().CreateImage();
            qrImage.PositionScale_X = 0.5f;
            qrImage.PositionOffset_X = -128;
            qrImage.PositionOffset_Y = 80;
            qrImage.SizeOffset_X = 256;
            qrImage.SizeOffset_Y = 256;
            qrImage.IsVisible = false;
            modalBox.AddChild(qrImage);

            statusLabel = Glazier.Get().CreateLabel();
            statusLabel.PositionOffset_Y = 345;
            statusLabel.SizeOffset_Y = 30;
            statusLabel.SizeScale_X = 1;
            statusLabel.Text = "Gerando QR Code...";
            statusLabel.FontSize = ESleekFontSize.Small;
            modalBox.AddChild(statusLabel);

            closeButton = Glazier.Get().CreateButton();
            closeButton.PositionScale_X = 0.5f;
            closeButton.PositionOffset_X = -80;
            closeButton.PositionOffset_Y = 390;
            closeButton.SizeOffset_X = 160;
            closeButton.SizeOffset_Y = 45;
            closeButton.Text = "Fechar";
            closeButton.FontSize = ESleekFontSize.Medium;
            closeButton.OnClicked += onClickedCloseButton;
            modalBox.AddChild(closeButton);
        }
    }
}
