using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tank & Healer Studio Ultimate Touchpad.
/// Faithful reproduction of the Russian Unturned APK IL2CPP 0x1268800 - 0x126A800.
/// </summary>
public class UltimateTouchpad : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	public enum DisplayType { Touchpad, Inactive }
	public enum ScaleReference { ScreenHeight, ScreenWidth }
	public enum TapCountOption { None, Single, Double }

	public static Dictionary<string, UltimateTouchpad> UltimateTouchpads = new Dictionary<string, UltimateTouchpad>();

	public string touchpadName = "LookTouchPad_8tdg5543ffre";

	public RectTransform baseTrans;
	private int _inputId = -10;
	public bool inputActive = false;
	public Rect touchpadRect;
	public Vector2 previousPosition = Vector2.zero;
	public Vector2 currentPosition = Vector2.zero;

	public float horizontalValue = 0f;
	public float verticalValue = 0f;

	public float screenWidth = 1920f;
	public float screenHeight = 1080f;

	public Canvas ParentCanvas { get; private set; }
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

	public bool displayBase = true;
	public DisplayType displayType = DisplayType.Touchpad;
	public Image baseImage;
	public Color baseColor = new Color(1f, 1f, 1f, 0f);
	private Color invisibleBaseColor = Color.clear;

	public float alphaEnabled = 1f;
	public float alphaDisabled = 0f;
	public float enabledDuration = 1f;
	public float disabledDuration = 1f;
	private float enabledSpeed = 1f;
	private float disabledSpeed = 1f;
	public float inactiveTime = 5f;

	public bool displayInput = false;
	public Image inputImage;
	public Color inputColor = Color.white;
	private Color invisibleInputColor = Color.clear;

	public ScaleReference scaleReference = ScaleReference.ScreenHeight;
	public float imageSize = 0.5f;

	public TapCountOption tapCountOption = TapCountOption.None;
	public float tapCountDuration = 0.5f;
	public int targetTapCount = 2;
	private bool tapCountAchieved = false;
	private float currentTapTime = 0f;
	private int tapCount = 0;

	public bool useTouchInput = false;

	private void Awake()
	{
		if (baseTrans == null) baseTrans = GetComponent<RectTransform>();
		ParentCanvas = GetComponentInParent<Canvas>();
		if (ParentCanvas != null) canvasRectTrans = ParentCanvas.GetComponent<RectTransform>();

		if (!string.IsNullOrEmpty(touchpadName))
		{
			UltimateTouchpads[touchpadName] = this;
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

		if (canvasRectTrans != null)
		{
			screenWidth = canvasRectTrans.sizeDelta.x;
			screenHeight = canvasRectTrans.sizeDelta.y;
		}
		else
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
		}

		UpdateTouchpadPositioning();
	}

	public void OnPointerDown(PointerEventData touchInfo)
	{
		if (useTouchInput) return;
		ProcessOnInputDown(touchInfo.position, touchInfo.pointerId);
	}

	public void OnDrag(PointerEventData touchInfo)
	{
		if (useTouchInput) return;
		if (_inputId != touchInfo.pointerId) return;
		currentPosition = touchInfo.position;
	}

	public void OnPointerUp(PointerEventData touchInfo)
	{
		if (useTouchInput) return;
		ProcessOnInputUp(touchInfo.position, touchInfo.pointerId);
	}

	private void ProcessOnInputDown(Vector2 inputPosition, int inputId)
	{
		if (inputActive) return;

		inputActive = true;
		_inputId = inputId;
		currentPosition = inputPosition;
		previousPosition = inputPosition;

		StartCoroutine("ProcessInput");
	}

	private void ProcessOnInputMoved(Vector2 inputPosition, int inputId)
	{
		if (_inputId != inputId) return;
		currentPosition = inputPosition;
	}

	private void ProcessOnInputUp(Vector2 inputPosition, int inputId)
	{
		if (_inputId != inputId) return;

		inputActive = false;
		_inputId = -10;
		horizontalValue = 0f;
		verticalValue = 0f;
	}

	private IEnumerator ProcessInput()
	{
		while (inputActive)
		{
			UpdateTouchPad();
			yield return null;
		}
	}

	private void UpdateTouchPad()
	{
		Vector2 diff = currentPosition - previousPosition;
		if (ParentCanvas != null && ParentCanvas.scaleFactor > 0f)
		{
			diff /= ParentCanvas.scaleFactor;
		}

		float w = (screenWidth > 0f) ? screenWidth : 1920f;
		float h = (screenHeight > 0f) ? screenHeight : 1080f;

		float targetH = (diff.x / w) * _xSensitivity;
		float targetV = (diff.y / h) * _ySensitivity;

		horizontalValue = Mathf.Lerp(horizontalValue, targetH, xGravity);
		verticalValue = Mathf.Lerp(verticalValue, targetV, yGravity);

		previousPosition = currentPosition;
	}

	public void ResetTouchpad()
	{
		inputActive = false;
		_inputId = -10;
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
