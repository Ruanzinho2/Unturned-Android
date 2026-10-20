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
		private RectTransform _parentRect;

		private void Start()
		{
			onGameStart?.Invoke();

			_parentRect = transform as RectTransform;

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

			if (_parentRect == null)
			{
				_parentRect = transform as RectTransform;
			}

			// Position the background where the touch occurred within the parent touch zone
			if (joystickBackground != null)
			{
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
					_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointInParent))
				{
					joystickBackground.anchoredPosition = localPointInParent;
				}
				else
				{
					joystickBackground.position = (Vector3)eventData.position;
				}
			}

			if (joyStick != null)
			{
				joyStick.anchoredPosition = Vector2.zero;
			}

			joyStickInput = Vector2.zero;
			onPressedJoystick?.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (joystickState == State.Un_Interactable) return;
			if (!_isDragging || eventData.pointerId != _activePointerId) return;

			onStartedDraggingJoystick?.Invoke();

			if (joystickBackground != null)
			{
				// Compute deflection in the background's local coordinate space (Darrell4/touch-input BaseJoystick)
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
					joystickBackground, eventData.position, eventData.pressEventCamera, out Vector2 localPointInBg))
				{
					float radius = joystickBackground.rect.width * 0.5f;
					if (radius <= 0f) radius = 125f;

					Vector2 normalized = localPointInBg / radius;
					if (normalized.magnitude > 0.05f)
					{
						joyStickInput = Vector2.ClampMagnitude(normalized, 1f);
					}
					else
					{
						joyStickInput = Vector2.zero;
					}

					if (joyStick != null)
					{
						float range = (joystickMovementRange != 0f) ? joystickMovementRange : 1f;
						joyStick.anchoredPosition = joyStickInput * (radius * range);
					}
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
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
