using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual floating joystick for character and vehicle movement.
	/// </summary>
	public class FloatingJoyStick : Singleton<FloatingJoyStick>, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public RectTransform joystickBackground;
		public RectTransform joyStick;
		public float joystickMovementRange = 75f;
		public Vector2 joyStickInput = Vector2.zero;
		public Vector2 joystickCurrentPosition = Vector2.zero;

		public Color normalColor = new Color(1f, 1f, 1f, 0.4f);
		public Color pressedColor = new Color(1f, 1f, 1f, 0.8f);
		public State joystickState = State.Interactable;

		public UnityEvent onGameStart = new UnityEvent();
		public UnityEvent onPressedJoystick = new UnityEvent();
		public UnityEvent onStartedDraggingJoystick = new UnityEvent();
		public UnityEvent onStoppedDraggingJoystick = new UnityEvent();
		private string _sceneNameTemp;

		private Canvas m_Canvas;
		private Vector2 m_InitialBackgroundPos;
		private Image m_BackgroundImage;
		private Image m_HandleImage;

		private void Start()
		{
			m_Canvas = GetComponentInParent<Canvas>();

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
			onGameStart?.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (joystickState == State.Un_Interactable) return;
			if (ControllerManager.editMode) return;

			onPressedJoystick?.Invoke();
			SetJoystickColor(pressedColor);

			// Floating positioning on touch down
			if (joystickBackground != null && m_Canvas != null)
			{
				RectTransform parentRt = joystickBackground.parent as RectTransform;
				if (parentRt != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
				{
					joystickBackground.anchoredPosition = localPoint;
				}
			}

			OnDrag(eventData);
		}

		private bool wasAutoSprinting = false;

		public void OnDrag(PointerEventData eventData)
		{
			if (joystickState == State.Un_Interactable) return;
			if (ControllerManager.editMode) return;

			onStartedDraggingJoystick?.Invoke();

			if (joystickBackground != null && joyStick != null)
			{
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
				{
					float distance = Mathf.Clamp(localPoint.magnitude, 0f, joystickMovementRange);
					Vector2 direction = localPoint.normalized;
					joyStick.anchoredPosition = direction * distance;
					joyStickInput = joyStick.anchoredPosition / (joystickMovementRange > 0f ? joystickMovementRange : 1f);
					joystickCurrentPosition = joyStick.anchoredPosition;

					if (joyStickInput.y > 0.85f)
					{
						wasAutoSprinting = true;
						SDG.Unturned.InputBroker.SetKeyDown(SDG.Unturned.ControlsSettings.sprint);
					}
					else if (wasAutoSprinting)
					{
						wasAutoSprinting = false;
						SDG.Unturned.InputBroker.SetKeyUp(SDG.Unturned.ControlsSettings.sprint);
					}
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (joystickState == State.Un_Interactable) return;

			onStoppedDraggingJoystick?.Invoke();
			SetJoystickColor(normalColor);

			if (wasAutoSprinting)
			{
				wasAutoSprinting = false;
				SDG.Unturned.InputBroker.SetKeyUp(SDG.Unturned.ControlsSettings.sprint);
			}

			if (joyStick != null) joyStick.anchoredPosition = Vector2.zero;
			if (joystickBackground != null) joystickBackground.anchoredPosition = m_InitialBackgroundPos;

			joyStickInput = Vector2.zero;
			joystickCurrentPosition = Vector2.zero;
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
