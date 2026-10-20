using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Virtual floating joystick for character and vehicle movement.
	/// Supports dynamic touch centering, auto-sprint threshold, and multi-touch isolation.
	/// Feeds direction directly into Unturned's ControlsSettings keys via InputBroker.
	/// </summary>
	public class FloatingJoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		[Header("Joystick References")]
		public RectTransform joystickBackground;
		public RectTransform joyStickHandle;

		[Header("Settings")]
		public float joystickMovementRange = 75f;
		public float deadZone = 0.15f;
		public bool isFloating = true;
		public bool autoSprintEnabled = true;
		public float autoSprintThreshold = 0.85f;

		public Vector2 joystickInput { get; private set; }
		public bool isDragging { get; private set; }

		private int activePointerId = -1;
		private Vector2 backgroundDefaultAnchoredPos;
		private RectTransform touchZoneRect;

		private KeyCode currentHorizontalKey = KeyCode.None;
		private KeyCode currentVerticalKey = KeyCode.None;
		private bool isAutoSprinting = false;

		private void Awake()
		{
			touchZoneRect = GetComponent<RectTransform>();
			if (joystickBackground != null)
			{
				backgroundDefaultAnchoredPos = joystickBackground.anchoredPosition;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (activePointerId != -1) return;

			activePointerId = eventData.pointerId;
			isDragging = true;

			if (isFloating && joystickBackground != null && touchZoneRect != null)
			{
				Vector2 localPoint;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
					touchZoneRect,
					eventData.position,
					eventData.pressEventCamera,
					out localPoint))
				{
					joystickBackground.anchoredPosition = localPoint;
				}
			}

			if (joyStickHandle != null)
			{
				joyStickHandle.anchoredPosition = Vector2.zero;
			}

			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerId != activePointerId) return;
			if (joystickBackground == null || joyStickHandle == null) return;

			Vector2 position;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				joystickBackground,
				eventData.position,
				eventData.pressEventCamera,
				out position
			);

			position = Vector2.ClampMagnitude(position, joystickMovementRange);
			joyStickHandle.anchoredPosition = position;

			Vector2 rawInput = position / joystickMovementRange;
			joystickInput = rawInput.magnitude > deadZone ? rawInput : Vector2.zero;

			UpdateSimulatedKeys();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId != activePointerId) return;

			activePointerId = -1;
			isDragging = false;
			joystickInput = Vector2.zero;

			if (joyStickHandle != null)
			{
				joyStickHandle.anchoredPosition = Vector2.zero;
			}

			if (isFloating && joystickBackground != null)
			{
				joystickBackground.anchoredPosition = backgroundDefaultAnchoredPos;
			}

			ReleaseSimulatedKeys();
		}

		private void UpdateSimulatedKeys()
		{
			// Map horizontal axis (A / D)
			KeyCode targetHorizontal = KeyCode.None;
			if (joystickInput.x > deadZone)
			{
				targetHorizontal = ControlsSettings.right;
			}
			else if (joystickInput.x < -deadZone)
			{
				targetHorizontal = ControlsSettings.left;
			}

			if (targetHorizontal != currentHorizontalKey)
			{
				if (currentHorizontalKey != KeyCode.None)
				{
					InputBroker.SetKeyUp(currentHorizontalKey);
				}
				if (targetHorizontal != KeyCode.None)
				{
					InputBroker.SetKeyDown(targetHorizontal);
				}
				currentHorizontalKey = targetHorizontal;
			}

			// Map vertical axis (W / S)
			KeyCode targetVertical = KeyCode.None;
			if (joystickInput.y > deadZone)
			{
				targetVertical = ControlsSettings.up;
			}
			else if (joystickInput.y < -deadZone)
			{
				targetVertical = ControlsSettings.down;
			}

			if (targetVertical != currentVerticalKey)
			{
				if (currentVerticalKey != KeyCode.None)
				{
					InputBroker.SetKeyUp(currentVerticalKey);
				}
				if (targetVertical != KeyCode.None)
				{
					InputBroker.SetKeyDown(targetVertical);
				}
				currentVerticalKey = targetVertical;
			}

			// Auto-sprint check when pushed near full forward
			if (autoSprintEnabled)
			{
				bool wantsAutoSprint = joystickInput.y > autoSprintThreshold && joystickInput.magnitude > autoSprintThreshold;
				if (wantsAutoSprint != isAutoSprinting)
				{
					isAutoSprinting = wantsAutoSprint;
					if (isAutoSprinting)
					{
						InputBroker.SetKeyDown(ControlsSettings.sprint);
					}
					else
					{
						InputBroker.SetKeyUp(ControlsSettings.sprint);
					}
				}
			}
		}

		private void ReleaseSimulatedKeys()
		{
			if (currentHorizontalKey != KeyCode.None)
			{
				InputBroker.SetKeyUp(currentHorizontalKey);
				currentHorizontalKey = KeyCode.None;
			}
			if (currentVerticalKey != KeyCode.None)
			{
				InputBroker.SetKeyUp(currentVerticalKey);
				currentVerticalKey = KeyCode.None;
			}
			if (isAutoSprinting)
			{
				InputBroker.SetKeyUp(ControlsSettings.sprint);
				isAutoSprinting = false;
			}
		}

		private void OnDisable()
		{
			ReleaseSimulatedKeys();
			activePointerId = -1;
			isDragging = false;
			joystickInput = Vector2.zero;
		}
	}
}
