using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Main manager and procedural UI builder for mobile touch controls.
	/// Automatically creates the virtual HUD on startup, synchronizes visibility
	/// with gameplay and menus, and provides seamless concurrent support for touch,
	/// physical keyboard, and physical mouse.
	/// </summary>
	public class MobileController : MonoBehaviour
	{
		public static MobileController Instance { get; private set; }

		[Header("HUD Canvas & Hierarchy")]
		public Canvas mobileCanvas;
		public GameObject gameplayRoot;
		public GameObject hudToggleRoot;
		public Text hudToggleText;

		[Header("Configuration")]
		public bool controlsEnabled = true;

		private Sprite circleSprite;
		private Sprite ringSprite;
		private Font uiFont;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void AutoInitialize()
		{
			if (Instance != null) return;

			GameObject controllerGo = new GameObject("[MobileController]");
			DontDestroyOnLoad(controllerGo);
			controllerGo.AddComponent<MobileController>();
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			controlsEnabled = PlayerPrefs.GetInt("MobileControls_Enabled", 1) == 1;

			EnsureEventSystem();
			CreateProceduralResources();
			BuildMobileHUD();
		}

		private void EnsureEventSystem()
		{
			if (EventSystem.current == null)
			{
				GameObject eventSystemGo = new GameObject("EventSystem");
				DontDestroyOnLoad(eventSystemGo);
				eventSystemGo.AddComponent<EventSystem>();
				eventSystemGo.AddComponent<StandaloneInputModule>();
			}
		}

		private void CreateProceduralResources()
		{
			circleSprite = CreateCircleSprite(128);
			ringSprite = CreateRingSprite(128, 0.68f);

			uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
			if (uiFont == null)
			{
				Font[] existingFonts = Resources.FindObjectsOfTypeAll<Font>();
				if (existingFonts != null && existingFonts.Length > 0)
				{
					uiFont = existingFonts[0];
				}
				else
				{
					uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Arial", "Roboto", "DejaVu Sans", "sans-serif" }, 16);
				}
			}
		}

		private void BuildMobileHUD()
		{
			if (mobileCanvas != null) return;

			// 1. Root Canvas
			GameObject canvasGo = CreateUIObject("MobileCanvas", transform);
			mobileCanvas = canvasGo.AddComponent<Canvas>();
			mobileCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			mobileCanvas.sortingOrder = UnturnedCanvasSortOrders.Glazier - 1; // Order 14: underneath Glazier (15)

			CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			scaler.matchWidthOrHeight = 0.5f;

			canvasGo.AddComponent<GraphicRaycaster>();

			// 2. Gameplay Root (Contains all active gameplay controls)
			gameplayRoot = CreateUIObject("GameplayRoot", canvasGo.transform);
			RectTransform gameplayRt = gameplayRoot.GetComponent<RectTransform>();
			gameplayRt.anchorMin = Vector2.zero;
			gameplayRt.anchorMax = Vector2.one;
			gameplayRt.offsetMin = Vector2.zero;
			gameplayRt.offsetMax = Vector2.zero;

			// 3. TouchPad (Right side camera look: anchor X 0.35 to 1.0, Y 0 to 1.0)
			// Sits at the back of the gameplay hierarchy so buttons take precedence for touches
			GameObject touchPadGo = CreateUIObject("TouchPad", gameplayRoot.transform);
			RectTransform touchPadRt = touchPadGo.GetComponent<RectTransform>();
			touchPadRt.anchorMin = new Vector2(0.35f, 0f);
			touchPadRt.anchorMax = new Vector2(1.0f, 1.0f);
			touchPadRt.offsetMin = Vector2.zero;
			touchPadRt.offsetMax = Vector2.zero;
			Image touchPadImg = touchPadGo.AddComponent<Image>();
			touchPadImg.color = Color.clear;
			touchPadImg.raycastTarget = true;
			touchPadGo.AddComponent<SpecialTouchPad>();

			// 4. Movement Joystick (Left side dynamic touch zone: anchor X 0 to 0.45, Y 0 to 0.65)
			GameObject joystickZoneGo = CreateUIObject("JoystickTouchZone", gameplayRoot.transform);
			RectTransform joystickZoneRt = joystickZoneGo.GetComponent<RectTransform>();
			joystickZoneRt.anchorMin = Vector2.zero;
			joystickZoneRt.anchorMax = new Vector2(0.45f, 0.65f);
			joystickZoneRt.offsetMin = Vector2.zero;
			joystickZoneRt.offsetMax = Vector2.zero;
			Image joystickZoneImg = joystickZoneGo.AddComponent<Image>();
			joystickZoneImg.color = Color.clear;
			joystickZoneImg.raycastTarget = true;

			GameObject joystickBgGo = CreateUIObject("JoystickBackground", joystickZoneGo.transform);
			RectTransform joystickBgRt = joystickBgGo.GetComponent<RectTransform>();
			joystickBgRt.anchorMin = Vector2.zero;
			joystickBgRt.anchorMax = Vector2.zero;
			joystickBgRt.anchoredPosition = new Vector2(210f, 210f);
			joystickBgRt.sizeDelta = new Vector2(180f, 180f);
			Image joystickBgImg = joystickBgGo.AddComponent<Image>();
			joystickBgImg.sprite = ringSprite;
			joystickBgImg.color = new Color(1f, 1f, 1f, 0.35f);
			joystickBgImg.raycastTarget = false;

			GameObject joystickHandleGo = CreateUIObject("JoystickHandle", joystickBgGo.transform);
			RectTransform joystickHandleRt = joystickHandleGo.GetComponent<RectTransform>();
			joystickHandleRt.anchorMin = new Vector2(0.5f, 0.5f);
			joystickHandleRt.anchorMax = new Vector2(0.5f, 0.5f);
			joystickHandleRt.anchoredPosition = Vector2.zero;
			joystickHandleRt.sizeDelta = new Vector2(75f, 75f);
			Image joystickHandleImg = joystickHandleGo.AddComponent<Image>();
			joystickHandleImg.sprite = circleSprite;
			joystickHandleImg.color = new Color(1f, 1f, 1f, 0.80f);
			joystickHandleImg.raycastTarget = false;

			FloatingJoyStick joyStick = joystickZoneGo.AddComponent<FloatingJoyStick>();
			joyStick.joystickBackground = joystickBgRt;
			joyStick.joyStickHandle = joystickHandleRt;
			joyStick.joystickMovementRange = 80f;
			joyStick.isFloating = true;
			joyStick.autoSprintEnabled = true;

			// 5. Left Action Buttons (Claw / 3-Finger support)
			CreateSpecialButton("LeftFireButton", gameplayRoot.transform,
				new Vector2(0f, 0f), new Vector2(100f, 440f), new Vector2(85f, 85f),
				EButtonAction.Primary, "FIRE", new Color(0.85f, 0.25f, 0.25f, 0.70f), Color.white);

			CreateSpecialButton("SprintButton", gameplayRoot.transform,
				new Vector2(0f, 0f), new Vector2(205f, 440f), new Vector2(80f, 80f),
				EButtonAction.Sprint, "RUN", new Color(0.30f, 0.60f, 0.75f, 0.65f), Color.white);

			// 6. Right Action Buttons (Aim, Fire, Movement, Interaction)
			CreateSpecialButton("PrimaryFireButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-155f, 230f), new Vector2(125f, 125f),
				EButtonAction.Primary, "FIRE", new Color(0.85f, 0.25f, 0.25f, 0.75f), Color.white);

			CreateSpecialButton("SecondaryAimButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-285f, 290f), new Vector2(100f, 100f),
				EButtonAction.Secondary, "AIM", new Color(0.20f, 0.55f, 0.85f, 0.70f), Color.white);

			CreateSpecialButton("InteractButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-380f, 190f), new Vector2(95f, 95f),
				EButtonAction.Interact, "USE", new Color(0.90f, 0.70f, 0.15f, 0.70f), Color.white);

			CreateSpecialButton("ReloadButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-145f, 380f), new Vector2(80f, 80f),
				EButtonAction.Reload, "RELOAD", new Color(0.25f, 0.65f, 0.35f, 0.65f), Color.white);

			CreateSpecialButton("JumpButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-95f, 90f), new Vector2(95f, 95f),
				EButtonAction.Jump, "JUMP", new Color(0.25f, 0.25f, 0.25f, 0.65f), Color.white);

			CreateSpecialButton("CrouchButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-205f, 90f), new Vector2(85f, 85f),
				EButtonAction.Crouch, "CROUCH", new Color(0.25f, 0.25f, 0.25f, 0.65f), Color.white);

			CreateSpecialButton("ProneButton", gameplayRoot.transform,
				new Vector2(1f, 0f), new Vector2(-305f, 90f), new Vector2(80f, 80f),
				EButtonAction.Prone, "PRONE", new Color(0.25f, 0.25f, 0.25f, 0.65f), Color.white);

			// 7. Top Bar Utilities (Pause, Inventory, Camera Perspective, Tactical)
			CreateSpecialButton("PauseButton", gameplayRoot.transform,
				new Vector2(1f, 1f), new Vector2(-65f, -50f), new Vector2(80f, 55f),
				EButtonAction.Pause, "PAUSE", new Color(0.20f, 0.20f, 0.20f, 0.65f), Color.white);

			CreateSpecialButton("InventoryButton", gameplayRoot.transform,
				new Vector2(1f, 1f), new Vector2(-155f, -50f), new Vector2(80f, 55f),
				EButtonAction.Inventory, "BAG", new Color(0.20f, 0.20f, 0.20f, 0.65f), Color.white);

			CreateSpecialButton("PerspectiveButton", gameplayRoot.transform,
				new Vector2(1f, 1f), new Vector2(-245f, -50f), new Vector2(75f, 55f),
				EButtonAction.Perspective, "PERS", new Color(0.20f, 0.20f, 0.20f, 0.65f), Color.white);

			CreateSpecialButton("TacticalButton", gameplayRoot.transform,
				new Vector2(1f, 1f), new Vector2(-330f, -50f), new Vector2(75f, 55f),
				EButtonAction.Tactical, "TAC", new Color(0.20f, 0.20f, 0.20f, 0.65f), Color.white);

			// 8. HUD Toggle Root (Discreet button at top-left to hide/show virtual controls when using mouse/keyboard)
			hudToggleRoot = CreateUIObject("HUDToggleRoot", canvasGo.transform);
			RectTransform hudToggleRt = hudToggleRoot.GetComponent<RectTransform>();
			hudToggleRt.anchorMin = new Vector2(0f, 1f);
			hudToggleRt.anchorMax = new Vector2(0f, 1f);
			hudToggleRt.anchoredPosition = new Vector2(65f, -35f);
			hudToggleRt.sizeDelta = new Vector2(75f, 40f);

			Image toggleBg = hudToggleRoot.AddComponent<Image>();
			toggleBg.sprite = circleSprite;
			toggleBg.color = new Color(0.15f, 0.15f, 0.15f, 0.55f);
			toggleBg.raycastTarget = true;

			Button toggleBtn = hudToggleRoot.AddComponent<Button>();
			toggleBtn.onClick.AddListener(ToggleControls);

			GameObject toggleTextGo = CreateUIObject("Text", hudToggleRoot.transform);
			RectTransform toggleTextRt = toggleTextGo.GetComponent<RectTransform>();
			toggleTextRt.anchorMin = Vector2.zero;
			toggleTextRt.anchorMax = Vector2.one;
			toggleTextRt.offsetMin = Vector2.zero;
			toggleTextRt.offsetMax = Vector2.zero;

			hudToggleText = toggleTextGo.AddComponent<Text>();
			hudToggleText.text = controlsEnabled ? "HUD" : "OFF";
			hudToggleText.font = uiFont;
			hudToggleText.fontSize = 14;
			hudToggleText.alignment = TextAnchor.MiddleCenter;
			hudToggleText.color = new Color(1f, 1f, 1f, 0.85f);
			hudToggleText.raycastTarget = false;

			// Initially start hidden until player spawns
			gameplayRoot.SetActive(false);
			hudToggleRoot.SetActive(false);
		}

		private GameObject CreateSpecialButton(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Vector2 size,
			EButtonAction action, string label, Color bgColor, Color textColor)
		{
			GameObject btnGo = CreateUIObject(name, parent);
			RectTransform rt = btnGo.GetComponent<RectTransform>();
			rt.anchorMin = anchor;
			rt.anchorMax = anchor;
			rt.anchoredPosition = anchoredPos;
			rt.sizeDelta = size;

			CanvasGroup cg = btnGo.AddComponent<CanvasGroup>();
			Image img = btnGo.AddComponent<Image>();
			img.sprite = circleSprite;
			img.color = bgColor;
			img.raycastTarget = true;

			SpecialButton btn = btnGo.AddComponent<SpecialButton>();
			btn.action = action;
			btn.m_CanvasGroup = cg;
			btn.idleAlpha = bgColor.a;
			btn.pressedAlpha = 1.0f;

			GameObject textGo = CreateUIObject("Text", btnGo.transform);
			RectTransform textRt = textGo.GetComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			Text txt = textGo.AddComponent<Text>();
			txt.text = label;
			txt.font = uiFont;
			txt.fontSize = Mathf.Clamp(Mathf.RoundToInt(size.y * 0.26f), 12, 22);
			txt.fontStyle = FontStyle.Bold;
			txt.alignment = TextAnchor.MiddleCenter;
			txt.color = textColor;
			txt.raycastTarget = false;

			return btnGo;
		}

		private GameObject CreateUIObject(string name, Transform parent)
		{
			GameObject go = new GameObject(name);
			go.transform.SetParent(parent, false);
			go.layer = 5; // Layer 5 is UI
			go.AddComponent<RectTransform>();
			return go;
		}

		public void ToggleControls()
		{
			controlsEnabled = !controlsEnabled;
			PlayerPrefs.SetInt("MobileControls_Enabled", controlsEnabled ? 1 : 0);
			PlayerPrefs.Save();

			if (hudToggleText != null)
			{
				hudToggleText.text = controlsEnabled ? "HUD" : "OFF";
			}
		}

		private void Update()
		{
			bool hasAlivePlayer = Player.LocalPlayer != null &&
			                      Player.LocalPlayer.life != null &&
			                      Player.LocalPlayer.life.IsAlive;

			bool isModalOrMenuOpen = PlayerUI.window == null || PlayerUI.window.showCursor;
			bool shouldShowGameplayControls = hasAlivePlayer && !isModalOrMenuOpen && controlsEnabled;

			if (gameplayRoot != null && gameplayRoot.activeSelf != shouldShowGameplayControls)
			{
				gameplayRoot.SetActive(shouldShowGameplayControls);
			}

			if (hudToggleRoot != null && hudToggleRoot.activeSelf != hasAlivePlayer)
			{
				hudToggleRoot.SetActive(hasAlivePlayer);
			}
		}

		private void LateUpdate()
		{
			// Clean up frame-specific key transitions
			InputBroker.ClearFrameDeltas();
		}

		public void SetControlsVisible(bool visible)
		{
			controlsEnabled = visible;
			if (gameplayRoot != null)
			{
				gameplayRoot.SetActive(visible);
			}
		}

		private static Sprite CreateCircleSprite(int size)
		{
			Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Bilinear;
			Color[] pixels = new Color[size * size];
			float center = size * 0.5f;
			float radius = center - 1.5f;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
					float alpha = Mathf.Clamp01(radius + 0.5f - dist);
					pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
				}
			}

			tex.SetPixels(pixels);
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
		}

		private static Sprite CreateRingSprite(int size, float innerRadiusRatio)
		{
			Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Bilinear;
			Color[] pixels = new Color[size * size];
			float center = size * 0.5f;
			float outerRadius = center - 1.5f;
			float innerRadius = outerRadius * innerRadiusRatio;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
					float outerAlpha = Mathf.Clamp01(outerRadius + 0.5f - dist);
					float innerAlpha = Mathf.Clamp01(dist - (innerRadius - 0.5f));
					float alpha = Mathf.Min(outerAlpha, innerAlpha);
					pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
				}
			}

			tex.SetPixels(pixels);
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
		}
	}
}
