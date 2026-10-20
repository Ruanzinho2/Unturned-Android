using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual joystick for character and vehicle movement.
	/// Faithful reproduction of the Russian Unturned mobile joystick implementation.
	/// Keeps the base stationary at its HUD position and moves only the inner knob (Handle),
	/// with strict multitouch finger tracking to prevent any cross-touch conflicts.
	/// </summary>
	public class FloatingJoyStick : Singleton<FloatingJoyStick>, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public RectTransform joystickBackground;
		public RectTransform joyStick;
		public float joystickMovementRange = 1f;
		public Vector2 joyStickInput = Vector2.zero;
		public Vector2 joystickCurrentPosition = Vector2.zero;

		public Color normalColor = new Color(1f, 1f, 1f, 0.39215686f);
		public Color pressedColor = new Color(1f, 1f, 1f, 1f);
		public State joystickState = State.Interactable;

		public UnityEvent onGameStart = new UnityEvent();
		public UnityEvent onPressedJoystick = new UnityEvent();
		public UnityEvent onStartedDraggingJoystick = new UnityEvent();
		public UnityEvent onStoppedDraggingJoystick = new UnityEvent();
		private string _sceneNameTemp;

		private Vector2 m_InitialBackgroundPos;
		private Image m_BackgroundImage;
		private Image m_HandleImage;
		private Canvas m_ParentCanvas;

		private int _activePointerId = -1;
		private bool _isDragging = false;

		private void Start()
		{
			onGameStart?.Invoke();

			m_ParentCanvas = GetComponentInParent<Canvas>();

			if (joystickBackground != null)
			{
				m_InitialBackgroundPos = joystickBackground.anchoredPosition;
				m_BackgroundImage = joystickBackground.GetComponent<Image>();
			}
			if (joyStick != null)
			{
				m_HandleImage = joyStick.GetComponent<Image>();
			}

			SetJoystickColor(normalColor);

			_sceneNameTemp = SceneManager.GetActiveScene().name;
			if (string.Equals(_sceneNameTemp, "Menu"))
			{
				gameObject.SetActive(false);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;
			if (_isDragging) return; // Multitouch isolation: lock onto first finger

			_isDragging = true;
			_activePointerId = eventData.pointerId;

			SetJoystickColor(pressedColor);

			// Keep the base stationary at its configured position
			if (joystickBackground != null)
			{
				joystickBackground.anchoredPosition = m_InitialBackgroundPos;
			}
			if (joyStick != null)
			{
				joyStick.anchoredPosition = Vector2.zero;
			}

			OnDrag(eventData);
			onPressedJoystick?.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;
			if (!_isDragging || eventData.pointerId != _activePointerId) return; // Ignore any other finger

			onStartedDraggingJoystick?.Invoke();

			// Calculate vector from the stationary background center on screen to the touch point
			Vector2 bgCenter = (joystickBackground != null) ? (Vector2)joystickBackground.position : eventData.position;
			Vector2 diff = eventData.position - bgCenter;

			float scale = (m_ParentCanvas != null && m_ParentCanvas.scaleFactor > 0f) ? m_ParentCanvas.scaleFactor : 1f;
			float radiusCanvas = (joystickBackground != null) ? (joystickBackground.sizeDelta.x * 0.5f) : 125f;
			float radiusScreen = radiusCanvas * scale;

			float mag = diff.magnitude;
			if (mag > radiusScreen)
			{
				joyStickInput = diff.normalized;
			}
			else
			{
				joyStickInput = (radiusScreen > 0f) ? (diff / radiusScreen) : Vector2.zero;
			}

			// Only move the inner knob (Handle), base remains stationary
			if (joyStick != null && joystickBackground != null)
			{
				float range = (joystickMovementRange != 0f) ? joystickMovementRange : 1f;
				joyStick.anchoredPosition = (joyStickInput * radiusCanvas) * range;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;
			if (!_isDragging || eventData.pointerId != _activePointerId) return; // Ignore other finger releases

			_isDragging = false;
			_activePointerId = -1;

			SetJoystickColor(normalColor);
			joyStickInput = Vector2.zero;

			if (joyStick != null)
			{
				joyStick.anchoredPosition = Vector2.zero;
			}
			if (joystickBackground != null)
			{
				joystickBackground.anchoredPosition = m_InitialBackgroundPos;
			}

			onStoppedDraggingJoystick?.Invoke();
		}

		public void SetJoystickColor(Color color)
		{
			if (m_BackgroundImage != null) m_BackgroundImage.color = color;
			if (m_HandleImage != null) m_HandleImage.color = color;
		}

		public float GetVerticalValue() => joyStickInput.y;
		public float GetHorizontalValue() => joyStickInput.x;
		public Vector2 GetHorizontalAndVerticalValue() => joyStickInput;

		public void SetState(State state) => joystickState = state;
		public State GetState() => joystickState;
	}
}
