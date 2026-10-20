using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual joystick for character and vehicle movement.
	/// Faithful reproduction of the Russian Unturned mobile joystick implementation (IL2CPP 0x107671C - 0x1076D38).
	/// Dynamically centers at the touch point and deflects the inner handle.
	/// </summary>
	public class FloatingJoyStick : Singleton<FloatingJoyStick>, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IDragHandler
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

		private Image m_BackgroundImage;
		private Image m_HandleImage;
		private int _activePointerId = -1;
		private bool _isDragging = false;

		private void Start()
		{
			onGameStart?.Invoke();

			if (joystickBackground != null)
			{
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
			if (_isDragging) return; // Prevent secondary touch hijacking

			_isDragging = true;
			_activePointerId = eventData.pointerId;

			SetJoystickColor(pressedColor);

			joystickCurrentPosition = eventData.position;
			if (joystickBackground != null)
			{
				joystickBackground.position = (Vector3)eventData.position;
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
			if (!_isDragging || eventData.pointerId != _activePointerId) return;

			onStartedDraggingJoystick?.Invoke();

			Vector2 diff = eventData.position - joystickCurrentPosition;
			float radius = (joystickBackground != null) ? (joystickBackground.sizeDelta.x * 0.5f) : 125f;

			if (diff.magnitude > radius)
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
				joyStick.anchoredPosition = (joyStickInput * (joystickBackground.sizeDelta.x * 0.5f)) * range;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;
			if (!_isDragging || eventData.pointerId != _activePointerId) return;

			_isDragging = false;
			_activePointerId = -1;

			SetJoystickColor(normalColor);
			joyStickInput = Vector2.zero;

			if (joyStick != null)
			{
				joyStick.anchoredPosition = Vector2.zero;
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
