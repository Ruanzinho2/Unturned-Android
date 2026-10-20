using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tank & Healer Studio Ultimate Touchpad.
/// Used for smooth touch look control in Unturned Android port.
/// </summary>
public class UltimateTouchpad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	public static Dictionary<string, UltimateTouchpad> UltimateTouchpads = new Dictionary<string, UltimateTouchpad>();

	public string touchpadName = "LookTouchPad_8tdg5543ffre";

	public RectTransform baseTrans;
	private int _inputId = -1;
	public bool inputActive = false;
	public Rect touchpadRect;
	public Vector2 previousPosition = Vector2.zero;
	public Vector2 currentPosition = Vector2.zero;

	public float horizontalValue = 0f;
	public float verticalValue = 0f;

	public float screenWidth = 1920f;
	public float screenHeight = 1080f;

	public Canvas ParentCanvas { get; set; }
	public RectTransform canvasRectTrans;

	public float touchSizeWidth = 50f;
	public float touchSizeHeight = 100f;
	public float touchSizePositionX = 100f;
	public float touchSizePositionY = 0f;
	public float screenWidthReference = 1280f;

	public float xSensitivity = 0.1f;
	public float ySensitivity = 0.05f;
	public float _xSensitivity = 128f;
	public float _ySensitivity = 64f;

	public float xGravity = 0.5f;
	public float yGravity = 0.5f;

	public bool displayBase = false;
	public Image baseImage;
	public Color baseColor = new Color(1f, 1f, 1f, 0.1f);
	public Color invisibleBaseColor = Color.clear;

	public float alphaEnabled = 1f;
	public float alphaDisabled = 0f;

	public float enabledDuration = 0.2f;
	public float disabledDuration = 0.5f;
	public float enabledSpeed = 5f;
	public float disabledSpeed = 2f;
	public float inactiveTime = 0f;

	public bool displayInput = false;
	public Image inputImage;
	public Color inputColor = Color.white;
	public Color invisibleInputColor = Color.clear;

	public float imageSize = 100f;
	public float tapCountDuration = 0.5f;
	public int targetTapCount = 2;
	public bool tapCountAchieved = false;
	public float currentTapTime = 0f;
	public int tapCount = 0;
	public bool useTouchInput = true;

	private Vector2 accumulatedDelta = Vector2.zero;

	private void Awake()
	{
		if (baseTrans == null) baseTrans = GetComponent<RectTransform>();
		if (ParentCanvas == null) ParentCanvas = GetComponentInParent<Canvas>();
		if (ParentCanvas != null) canvasRectTrans = ParentCanvas.GetComponent<RectTransform>();

		if (!string.IsNullOrEmpty(touchpadName))
		{
			if (UltimateTouchpads.ContainsKey(touchpadName))
			{
				UltimateTouchpads[touchpadName] = this;
			}
			else
			{
				UltimateTouchpads.Add(touchpadName, this);
			}
		}
	}

	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(touchpadName))
		{
			UltimateTouchpads[touchpadName] = this;
		}
	}

	private void OnDisable()
	{
		if (!string.IsNullOrEmpty(touchpadName) && UltimateTouchpads.ContainsKey(touchpadName))
		{
			UltimateTouchpads.Remove(touchpadName);
		}
		ResetTouchpad();
	}

	private void Start()
	{
		_xSensitivity = xSensitivity * screenWidthReference;
		_ySensitivity = ySensitivity * screenWidthReference;
		UpdateTouchpadPositioning();
	}

	public void OnPointerDown(PointerEventData touchInfo)
	{
		if (inputActive) return;

		inputActive = true;
		_inputId = touchInfo.pointerId;
		previousPosition = touchInfo.position;
		currentPosition = touchInfo.position;

		tapCount++;
		currentTapTime = tapCountDuration;
		if (tapCount >= targetTapCount)
		{
			tapCountAchieved = true;
		}
	}

	public void OnDrag(PointerEventData touchInfo)
	{
		if (!inputActive || touchInfo.pointerId != _inputId) return;

		currentPosition = touchInfo.position;
	}

	public void OnPointerUp(PointerEventData touchInfo)
	{
		if (touchInfo.pointerId == _inputId)
		{
			ResetTouchpad();
		}
	}

	/// <summary>
	/// Injects external delta from SpecialButtons that have lookSimulate enabled (e.g. Fire, Aim).
	/// </summary>
	public void FeedExternalLookDelta(Vector2 delta)
	{
		currentPosition += delta;
	}

	private void Update()
	{
		UpdateTouchPad();

		if (currentTapTime > 0f)
		{
			currentTapTime -= Time.unscaledDeltaTime;
			if (currentTapTime <= 0f)
			{
				tapCount = 0;
				tapCountAchieved = false;
			}
		}
	}

	public void UpdateTouchPad()
	{
		float referenceWidth = (screenWidth > 0f) ? screenWidth : (Screen.width > 0 ? (float)Screen.width : 1920f);
		float referenceHeight = (screenHeight > 0f) ? screenHeight : (Screen.height > 0 ? (float)Screen.height : 1080f);

		Vector2 diff = currentPosition - previousPosition;
		if (ParentCanvas != null && ParentCanvas.scaleFactor > 0f)
		{
			diff /= ParentCanvas.scaleFactor;
		}

		float targetH = (diff.x / referenceWidth) * _xSensitivity;
		float targetV = (diff.y / referenceHeight) * _ySensitivity;

		// Authentic Russian APK exponential smoothing
		horizontalValue = Mathf.Lerp(horizontalValue, targetH, xGravity);
		verticalValue = Mathf.Lerp(verticalValue, targetV, yGravity);

		previousPosition = currentPosition;
	}

	public void ResetTouchpad()
	{
		inputActive = false;
		_inputId = -1;
		horizontalValue = 0f;
		verticalValue = 0f;
		previousPosition = currentPosition;
	}

	public void UpdateTouchpadPositioning()
	{
		if (baseTrans != null)
		{
			if (ParentCanvas == null) ParentCanvas = GetComponentInParent<Canvas>();
			float scale = (ParentCanvas != null && ParentCanvas.scaleFactor > 0f) ? ParentCanvas.scaleFactor : 1f;
			Vector2 size = baseTrans.sizeDelta * scale;
			Vector2 pos = (Vector2)baseTrans.position - (size * 0.5f);
			touchpadRect = new Rect(pos.x, pos.y, size.x, size.y);
		}
	}

	public float GetHorizontalAxis() => horizontalValue;
	public float GetVerticalAxis() => verticalValue;
	public bool GetTouchpadState() => inputActive;
	public int GetTapCount() => tapCount;

	public void DisableTouchpad() => enabled = false;
	public void EnableTouchpad() => enabled = true;

	public static UltimateTouchpad GetUltimateTouchpad(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			return touchpad;
		}
		return null;
	}

	public static float GetHorizontalAxis(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			return touchpad.GetHorizontalAxis();
		}
		return 0f;
	}

	public static float GetVerticalAxis(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			return touchpad.GetVerticalAxis();
		}
		return 0f;
	}

	public static bool GetTouchpadState(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			return touchpad.GetTouchpadState();
		}
		return false;
	}

	public static int GetTapCount(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			return touchpad.GetTapCount();
		}
		return 0;
	}

	public static void DisableTouchpad(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			touchpad.DisableTouchpad();
		}
	}

	public static void EnableTouchpad(string name)
	{
		if (UltimateTouchpads.TryGetValue(name, out UltimateTouchpad touchpad))
		{
			touchpad.EnableTouchpad();
		}
	}
}
