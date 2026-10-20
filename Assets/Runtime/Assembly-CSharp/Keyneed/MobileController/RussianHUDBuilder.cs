using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDG.Unturned;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Faithful runtime recreation of the Russian Unturned mobile HUD hierarchy and buttons.
	/// Automatically instantiates and initializes the complete mobile controls scheme.
	/// </summary>
	public class RussianHUDBuilder : MonoBehaviour
	{
		private static RussianHUDBuilder _instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void InitializeOnSceneLoad()
		{
			// Instantiate whenever Game scene is loaded
			string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			if (sceneName == "Game" || sceneName.Contains("Level") || sceneName.Contains("Map"))
			{
				EnsureHUDExists();
			}
		}

		public static void EnsureHUDExists()
		{
			if (_instance != null && _instance.gameObject != null) return;
			if (GameObject.Find("MobileHUDCanvas") != null) return;

			GameObject rootGo = new GameObject("MobileHUDCanvas");
			DontDestroyOnLoad(rootGo);
			_instance = rootGo.AddComponent<RussianHUDBuilder>();
			_instance.BuildRussianHUD();
		}

		private void LateUpdate()
		{
			InputBroker.ClearFrameDeltas();
		}

		private void BuildRussianHUD()
		{
			// 1. Root Canvas
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 3000;

			CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.matchWidthOrHeight = 0.5f;

			gameObject.AddComponent<GraphicRaycaster>();
			ControllerManager controllerMgr = gameObject.AddComponent<ControllerManager>();
			SaveManager saveMgr = gameObject.AddComponent<SaveManager>();
			LimitFrameRate frameLimiter = gameObject.AddComponent<LimitFrameRate>();

			Sprite circleSprite = CreateCircleSprite(128);
			Sprite roundedSquareSprite = CreateRoundedRectSprite(128, 128, 24);

			// 2. Joystick Touch Pad
			GameObject joyZoneGo = CreateUIElement("Joystick Touch Pad", transform,
				new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(480f, 265f), new Vector2(960f, 530f), new Vector2(0.5f, 0.5f));
			Image joyZoneImg = joyZoneGo.AddComponent<Image>();
			joyZoneImg.color = Color.clear; // Transparent hit area
			joyZoneImg.raycastTarget = true;

			// Joystick Background
			GameObject joyBgGo = CreateUIElement("Joystick Background", joyZoneGo.transform,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250f, 250f), new Vector2(0.5f, 0.5f));
			Image joyBgImg = joyBgGo.AddComponent<Image>();
			joyBgImg.sprite = circleSprite;
			joyBgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.45f);

			// Joystick Handle
			GameObject joyHandleGo = CreateUIElement("Handle", joyBgGo.transform,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f), new Vector2(0.5f, 0.5f));
			Image joyHandleImg = joyHandleGo.AddComponent<Image>();
			joyHandleImg.sprite = circleSprite;
			joyHandleImg.color = new Color(1f, 1f, 1f, 0.7f);

			FloatingJoyStick floatingJoy = joyZoneGo.AddComponent<FloatingJoyStick>();
			floatingJoy.joystickBackground = joyBgGo.GetComponent<RectTransform>();
			floatingJoy.joyStick = joyHandleGo.GetComponent<RectTransform>();
			floatingJoy.joystickMovementRange = 75f;
			floatingJoy.normalColor = new Color(1f, 1f, 1f, 0.45f);
			floatingJoy.pressedColor = new Color(0.2f, 0.8f, 1f, 0.8f);

			// 3. LookTouchPad_8tdg5543ffre
			GameObject lookZoneGo = CreateUIElement("LookTouchPad_8tdg5543ffre", transform,
				new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1548.4f, 502.2f), new Vector2(1032.3f, 1004.4f), new Vector2(0.5f, 0.5f));
			Image lookImg = lookZoneGo.AddComponent<Image>();
			lookImg.color = Color.clear;
			lookImg.raycastTarget = true;
			UltimateTouchpad touchpad = lookZoneGo.AddComponent<UltimateTouchpad>();
			touchpad.touchpadName = "LookTouchPad_8tdg5543ffre";
			lookZoneGo.AddComponent<UltimateTouchpadScreenSizeUpdater>();

			// 4. SliderSizeRegulator_048gr3gerbr5535g
			CreateSizeSlider(transform, controllerMgr);

			// 5. Build all 38 buttons with exact Russian layout
			BuildAllButtons(transform, circleSprite, roundedSquareSprite);
		}

		private struct ButtonSpec
		{
			public string name;
			public Vector2 anchorMin;
			public Vector2 anchorMax;
			public Vector2 anchoredPos;
			public Vector2 sizeDelta;
			public KeyCode keyCode;
			public bool lookSimulate;
			public ButtonPattern pattern;
			public string label;

			public ButtonSpec(string n, float aminX, float aminY, float amaxX, float amaxY, float posX, float posY, float sizeW, float sizeH, KeyCode key, bool look, ButtonPattern pat, string lbl)
			{
				name = n;
				anchorMin = new Vector2(aminX, aminY);
				anchorMax = new Vector2(amaxX, amaxY);
				anchoredPos = new Vector2(posX, posY);
				sizeDelta = new Vector2(sizeW, sizeH);
				keyCode = key;
				lookSimulate = look;
				pattern = pat;
				label = lbl;
			}
		}

		private void BuildAllButtons(Transform parent, Sprite circleSprite, Sprite squareSprite)
		{
			ButtonSpec[] specs = new ButtonSpec[]
			{
				// Primary Actions (Bottom Right)
				new ButtonSpec("Fire", 1f, 0f, 1f, 0f, -286.0f, 176.0f, 200.0f, 200.0f, KeyCode.Mouse0, true, ButtonPattern.None, "FIRE"),
				new ButtonSpec("Aim", 1f, 0f, 1f, 0f, -107.8f, 304.5f, 150.0f, 150.0f, KeyCode.Mouse1, true, ButtonPattern.None, "AIM"),
				new ButtonSpec("Fire1", 0f, 1f, 0f, 1f, 246.0f, -284.0f, 125.0f, 125.0f, KeyCode.Mouse0, true, ButtonPattern.None, "FIRE"),
				new ButtonSpec("Run", 1f, 0f, 1f, 0f, -243.9f, 498.3f, 165.8f, 166.0f, KeyCode.LeftShift, true, ButtonPattern.None, "RUN"),
				new ButtonSpec("Jump", 1f, 0f, 1f, 0f, -61.7f, 454.7f, 100.0f, 100.0f, KeyCode.Space, false, ButtonPattern.None, "JUMP"),
				new ButtonSpec("Reload", 1f, 0f, 1f, 0f, -92.1f, 587.0f, 125.0f, 125.0f, KeyCode.R, false, ButtonPattern.None, "RELOAD"),
				new ButtonSpec("Interact", 1f, 0f, 1f, 0f, -338.5f, 352.8f, 125.0f, 125.0f, KeyCode.F, false, ButtonPattern.None, "USE"),
				new ButtonSpec("Crouch", 1f, 0f, 1f, 0f, -57.2f, 60.0f, 75.0f, 75.0f, KeyCode.X, false, ButtonPattern.None, "CROUCH"),
				new ButtonSpec("Prone", 1f, 0f, 1f, 0f, -156.0f, 60.0f, 75.0f, 75.0f, KeyCode.Z, false, ButtonPattern.None, "PRONE"),

				// Weapons & Utility (Top / Right)
				new ButtonSpec("Firemode", 1f, 1f, 1f, 1f, -331.1f, -73.0f, 100.0f, 100.0f, KeyCode.V, false, ButtonPattern.None, "SEMI/AUTO"),
				new ButtonSpec("Attachment", 1f, 1f, 1f, 1f, -450.0f, -73.0f, 100.0f, 100.0f, KeyCode.T, false, ButtonPattern.None, "ATTACH"),
				new ButtonSpec("Light", 0f, 1f, 0f, 1f, 53.0f, -161.3f, 75.0f, 75.0f, KeyCode.N, false, ButtonPattern.None, "LIGHT"),
				new ButtonSpec("Voice", 1f, 1f, 1f, 1f, -212.1f, -165.2f, 100.0f, 100.0f, KeyCode.LeftAlt, true, ButtonPattern.None, "VOICE"),
				new ButtonSpec("Q", 1f, 1f, 1f, 1f, -438.8f, -186.0f, 100.0f, 100.0f, KeyCode.Q, true, ButtonPattern.None, "LEAN L"),
				new ButtonSpec("E", 1f, 1f, 1f, 1f, -220.0f, -186.0f, 100.0f, 100.0f, KeyCode.E, true, ButtonPattern.None, "LEAN R"),
				new ButtonSpec("H", 1f, 1f, 1f, 1f, -74.0f, -275.2f, 75.0f, 75.0f, KeyCode.H, true, ButtonPattern.None, "HORN"),

				// UI Navigation (Top / Left)
				new ButtonSpec("Inventory", 0f, 1f, 0f, 1f, 61.0f, -64.0f, 100.0f, 100.0f, KeyCode.Tab, false, ButtonPattern.None, "INVENTORY"),
				new ButtonSpec("Menu", 1f, 1f, 1f, 1f, -87.5f, -73.0f, 100.0f, 100.0f, KeyCode.Escape, false, ButtonPattern.None, "MENU"),
				new ButtonSpec("HUD", 0f, 1f, 0f, 1f, 0.0f, 0.0f, 75.0f, 75.0f, KeyCode.Home, false, ButtonPattern.None, "HUD"),
				new ButtonSpec("Chat", 1f, 1f, 1f, 1f, -86.0f, -261.3f, 75.0f, 75.0f, KeyCode.Return, false, ButtonPattern.None, "CHAT"),
				new ButtonSpec("Global Chat", 1f, 1f, 1f, 1f, -74.0f, -173.0f, 75.0f, 75.0f, KeyCode.J, false, ButtonPattern.None, "GLOBAL"),

				// Hotbar 1-5 (Bottom Center / Right)
				new ButtonSpec("1", 1f, 0f, 1f, 0f, -812.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha1, false, ButtonPattern.None, "1"),
				new ButtonSpec("2", 1f, 0f, 1f, 0f, -712.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha2, false, ButtonPattern.None, "2"),
				new ButtonSpec("3", 1f, 0f, 1f, 0f, -612.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha3, false, ButtonPattern.None, "3"),
				new ButtonSpec("4", 1f, 0f, 1f, 0f, -512.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha4, false, ButtonPattern.None, "4"),
				new ButtonSpec("5", 1f, 0f, 1f, 0f, -412.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha5, false, ButtonPattern.None, "5"),

				// Utility Quick Slots (Top Left)
				new ButtonSpec("C", 0f, 1f, 0f, 1f, 165.0f, -62.5f, 75.0f, 75.0f, KeyCode.C, false, ButtonPattern.None, "C"),
				new ButtonSpec("B", 0f, 1f, 0f, 1f, 256.0f, -62.5f, 75.0f, 75.0f, KeyCode.B, false, ButtonPattern.None, "B"),
				new ButtonSpec("N", 0f, 1f, 0f, 1f, 347.0f, -62.5f, 75.0f, 75.0f, KeyCode.N, false, ButtonPattern.None, "N"),

				// Vehicle Controls (Pattern = Vehicle)
				new ButtonSpec("ExitCar", 1f, 1f, 1f, 1f, -211.0f, -73.0f, 100.0f, 100.0f, KeyCode.F, false, ButtonPattern.Vehicle, "EXIT CAR"),
				new ButtonSpec("W", 1f, 0f, 1f, 0f, -523.0f, 451.7f, 169.2f, 169.2f, KeyCode.W, true, ButtonPattern.Vehicle, "W"),
				new ButtonSpec("A", 0f, 0f, 0f, 0f, 300.2f, 256.0f, 169.2f, 169.2f, KeyCode.A, false, ButtonPattern.Vehicle, "A"),
				new ButtonSpec("S", 1f, 0f, 1f, 0f, -523.0f, 229.0f, 169.2f, 169.2f, KeyCode.S, true, ButtonPattern.Vehicle, "S"),
				new ButtonSpec("D", 0f, 0f, 0f, 0f, 670.5f, 256.0f, 169.2f, 169.2f, KeyCode.D, false, ButtonPattern.Vehicle, "D"),
				new ButtonSpec("f1", 0f, 1f, 0f, 1f, 850.0f, -61.0f, 75.0f, 75.0f, KeyCode.F1, false, ButtonPattern.Vehicle, "F1"),
				new ButtonSpec("f2", 0f, 1f, 0f, 1f, 950.0f, -61.0f, 75.0f, 75.0f, KeyCode.F2, false, ButtonPattern.Vehicle, "F2"),
				new ButtonSpec("f3", 0f, 1f, 0f, 1f, 1050.0f, -61.0f, 75.0f, 75.0f, KeyCode.F3, false, ButtonPattern.Vehicle, "F3"),
				new ButtonSpec("f4", 0f, 1f, 0f, 1f, 1150.0f, -61.0f, 75.0f, 75.0f, KeyCode.F4, false, ButtonPattern.Vehicle, "F4"),
				new ButtonSpec("f5", 0f, 1f, 0f, 1f, 1250.0f, -61.0f, 75.0f, 75.0f, KeyCode.F5, false, ButtonPattern.Vehicle, "F5"),
				new ButtonSpec("f6", 0f, 1f, 0f, 1f, 1350.0f, -61.0f, 75.0f, 75.0f, KeyCode.F6, false, ButtonPattern.Vehicle, "F6")
			};

			foreach (var spec in specs)
			{
				CreateRussianButton(parent, spec, circleSprite);
			}
		}

		private void CreateRussianButton(Transform parent, ButtonSpec spec, Sprite bgSprite)
		{
			GameObject btnGo = CreateUIElement(spec.name, parent, spec.anchorMin, spec.anchorMax, spec.anchoredPos, spec.sizeDelta, new Vector2(0.5f, 0.5f));

			Image img = btnGo.AddComponent<Image>();
			img.sprite = bgSprite;
			img.color = new Color(0f, 0f, 0f, 0.45f);

			SpecialButton sb = btnGo.AddComponent<SpecialButton>();
			sb.keySimulate = spec.keyCode;
			sb.lookSimulate = spec.lookSimulate;

			CanvasGroup cg = btnGo.AddComponent<CanvasGroup>();
			sb.m_CanvasGroup = cg;

			Patterns pat = btnGo.AddComponent<Patterns>();
			pat.m_ButtonPattern = spec.pattern;
			pat.m_CanvasGroup = cg;
			pat.m_SpecialButton = sb;
			sb.m_Patterns = pat;

			MobileUIButton mb = btnGo.AddComponent<MobileUIButton>();
			mb.rectTransform = btnGo.GetComponent<RectTransform>();

			// Button Label / Icon
			GameObject textGo = new GameObject("Label");
			textGo.transform.SetParent(btnGo.transform, false);
			RectTransform textRt = textGo.AddComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.anchoredPosition = Vector2.zero;
			textRt.sizeDelta = Vector2.zero;

			Text txt = textGo.AddComponent<Text>();
			txt.text = spec.label;
			txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			txt.fontSize = Mathf.Clamp((int)(spec.sizeDelta.y * 0.25f), 12, 28);
			txt.alignment = TextAnchor.MiddleCenter;
			txt.color = Color.white;
			txt.raycastTarget = false;
		}

		private void CreateSizeSlider(Transform parent, ControllerManager mgr)
		{
			GameObject sliderGo = CreateUIElement("SliderSizeRegulator_048gr3gerbr5535g", parent,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(160f, 20f), new Vector2(0.5f, 0.5f));

			Slider slider = sliderGo.AddComponent<Slider>();
			slider.minValue = 40f;
			slider.maxValue = 250f;
			slider.value = 100f;

			// Background
			GameObject bgGo = CreateUIElement("Background", sliderGo.transform,
				new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
			Image bgImg = bgGo.AddComponent<Image>();
			bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

			// Fill Area
			GameObject fillAreaGo = CreateUIElement("Fill Area", sliderGo.transform,
				new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(-5f, 0f), new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
			GameObject fillGo = CreateUIElement("Fill", fillAreaGo.transform,
				new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(10f, 0f), new Vector2(0.5f, 0.5f));
			Image fillImg = fillGo.AddComponent<Image>();
			fillImg.color = new Color(0.2f, 0.8f, 1f, 0.9f);
			slider.fillRect = fillGo.GetComponent<RectTransform>();

			// Handle Slide Area
			GameObject handleAreaGo = CreateUIElement("Handle Slide Area", sliderGo.transform,
				Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
			GameObject handleGo = CreateUIElement("Handle", handleAreaGo.transform,
				new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(20f, 0f), new Vector2(0.5f, 0.5f));
			Image handleImg = handleGo.AddComponent<Image>();
			handleImg.color = Color.white;
			slider.handleRect = handleGo.GetComponent<RectTransform>();
			slider.targetGraphic = handleImg;

			mgr.resizeSlider = slider;
			sliderGo.SetActive(false);
		}

		private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Vector2 pivot)
		{
			GameObject go = new GameObject(name);
			go.transform.SetParent(parent, false);
			RectTransform rt = go.AddComponent<RectTransform>();
			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			rt.pivot = pivot;
			return go;
		}

		private static Sprite CreateCircleSprite(int size)
		{
			Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			Color[] colors = new Color[size * size];
			float center = size * 0.5f;
			float radius = center - 2f;
			float radiusSq = radius * radius;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dx = x - center;
					float dy = y - center;
					float distSq = dx * dx + dy * dy;
					if (distSq <= radiusSq)
					{
						float edge = Mathf.Clamp01((radius - Mathf.Sqrt(distSq)));
						colors[y * size + x] = new Color(1f, 1f, 1f, edge);
					}
					else
					{
						colors[y * size + x] = Color.clear;
					}
				}
			}
			tex.SetPixels(colors);
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
		}

		private static Sprite CreateRoundedRectSprite(int w, int h, int cornerRadius)
		{
			Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
			Color[] colors = new Color[w * h];
			for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
			tex.SetPixels(colors);
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
		}
	}
}
