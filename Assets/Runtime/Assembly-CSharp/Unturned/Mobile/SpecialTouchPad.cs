using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Virtual touchpad for camera and look rotation on mobile touchscreens.
	/// Tracks finger delta on the touchable screen area and exposes scaled look delta.
	/// Cleanses frame delta at LateUpdate to prevent infinite camera spinning when stationary.
	/// </summary>
	public class SpecialTouchPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		public static SpecialTouchPad Instance { get; private set; }

		[Header("Sensitivity Settings")]
		public float sensitivityX = 2.0f;
		public float sensitivityY = 2.0f;

		public bool pressingTouchPad { get; private set; }

		private int eventPointerID = -1;
		private Vector2 pointerOld;
		private Vector2 currentFrameDelta;

		public Vector2 touchPadInput
		{
			get
			{
				if (!pressingTouchPad || currentFrameDelta == Vector2.zero)
				{
					return Vector2.zero;
				}

				return new Vector2(
					currentFrameDelta.x * sensitivityX * (1000f / Mathf.Max(Screen.width, 1)),
					currentFrameDelta.y * sensitivityY * (1000f / Mathf.Max(Screen.height, 1))
				);
			}
		}

		private void Awake()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventPointerID == -1)
			{
				eventPointerID = eventData.pointerId;
				pointerOld = eventData.position;
				pressingTouchPad = true;
				currentFrameDelta = Vector2.zero;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerId == eventPointerID)
			{
				Vector2 currentPos = eventData.position;
				Vector2 delta = currentPos - pointerOld;
				pointerOld = currentPos;
				currentFrameDelta += delta;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == eventPointerID)
			{
				eventPointerID = -1;
				pressingTouchPad = false;
				currentFrameDelta = Vector2.zero;
			}
		}

		private void LateUpdate()
		{
			// Reset frame delta after it has been sampled in Update() (e.g. by PlayerLook)
			currentFrameDelta = Vector2.zero;
		}

		private void OnDisable()
		{
			eventPointerID = -1;
			pressingTouchPad = false;
			currentFrameDelta = Vector2.zero;
		}
	}
}
