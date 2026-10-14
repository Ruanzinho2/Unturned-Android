using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Virtual floating joystick for character and vehicle movement.
	/// Feeds direction directly into Unturned's ControlsSettings keys via InputBroker.
	/// </summary>
	public class FloatingJoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		[Header("Joystick References")]
		public RectTransform joystickBackground;
		public RectTransform joyStickHandle;

		[Header("Settings")]
		public float joystickMovementRange = 50f;
		public float deadZone = 0.15f;

		public Vector2 joystickInput { get; private set; }
		public bool isDragging { get; private set; }

		private Vector2 startPosition;

		private KeyCode currentHorizontalKey = KeyCode.None;
		private KeyCode currentVerticalKey = KeyCode.None;

		private void Awake()
		{
			if (joystickBackground == null)
			{
				joystickBackground = GetComponent<RectTransform>();
			}
			if (joyStickHandle != null)
			{
				startPosition = joyStickHandle.anchoredPosition;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isDragging = true;
			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
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
			isDragging = false;
			joystickInput = Vector2.zero;

			if (joyStickHandle != null)
			{
				joyStickHandle.anchoredPosition = startPosition;
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
		}

		private void OnDisable()
		{
			ReleaseSimulatedKeys();
			isDragging = false;
		}
	}
}
