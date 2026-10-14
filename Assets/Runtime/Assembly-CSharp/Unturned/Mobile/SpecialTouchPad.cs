using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Virtual touchpad for camera and look rotation on mobile touchscreens.
	/// Tracks finger delta on the right half of the screen and exposes look delta.
	/// </summary>
	public class SpecialTouchPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		public static SpecialTouchPad Instance { get; private set; }

		[Header("Sensitivity Settings")]
		public float sensitivityX = 2.0f;
		public float sensitivityY = 2.0f;

		public Vector2 touchPadInput { get; private set; }
		public bool pressingTouchPad { get; private set; }

		private int eventPointerID = -1;
		private Vector2 pointerOld;

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
				touchPadInput = Vector2.zero;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerId == eventPointerID)
			{
				Vector2 currentPos = eventData.position;
				Vector2 delta = currentPos - pointerOld;
				pointerOld = currentPos;

				// Calculate scaled look delta
				touchPadInput = new Vector2(
					delta.x * sensitivityX * (1000f / Mathf.Max(Screen.width, 1)),
					delta.y * sensitivityY * (1000f / Mathf.Max(Screen.height, 1))
				);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == eventPointerID)
			{
				eventPointerID = -1;
				pressingTouchPad = false;
				touchPadInput = Vector2.zero;
			}
		}

		private void LateUpdate()
		{
			// Reset instantaneous delta at the end of each frame
			if (!pressingTouchPad)
			{
				touchPadInput = Vector2.zero;
			}
		}

		private void OnDisable()
		{
			eventPointerID = -1;
			pressingTouchPad = false;
			touchPadInput = Vector2.zero;
		}
	}
}
