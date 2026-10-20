using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual floating joystick for character and vehicle movement.
	/// Faithful 1:1 reproduction of the Russian Unturned mobile joystick implementation.
	/// Uses screen-space relative touch drag mathematics and provides continuous analog values.
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

		private void Start()
		{
			onGameStart?.Invoke();

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

			SetJoystickColor(pressedColor);

			// Authentic Russian APK floating behavior:
			// Record the screen-space touch origin and center the background under the finger
			joystickCurrentPosition = eventData.position;

			if (joystickBackground != null)
			{
				joystickBackground.position = eventData.position;
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

			onStartedDraggingJoystick?.Invoke();

			// Authentic Russian APK vector math in screen space:
			Vector2 diff = eventData.position - joystickCurrentPosition;
			float mag = diff.magnitude;
			float radius = (joystickBackground != null) ? (joystickBackground.sizeDelta.x * 0.5f) : 125f;

			if (mag > radius)
			{
				joyStickInput = diff.normalized;
			}
			else
			{
				joyStickInput = (radius > 0f) ? (diff / radius) : Vector2.zero;
			}

			if (joyStick != null && joystickBackground != null)
			{
				float range = (joystickMovementRange != 0f) ? joystickMovementRange : 1f;
				joyStick.anchoredPosition = (joyStickInput * (joystickBackground.sizeDelta / 2f)) * range;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;

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
