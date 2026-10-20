using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Standalone virtual touchpad component (used alongside or alternate to UltimateTouchpad).
	/// </summary>
	public class SpecialTouchPad : Singleton<SpecialTouchPad>, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public Vector2 distanceBetweenTouch = Vector2.zero;
		public Vector2 PointerOld = Vector2.zero;
		public int eventPointerID = -1;
		public bool pressingTouchPad = false;
		public Vector2 touchPadInput = Vector2.zero;
		public float senstivityX = 1f;
		public float senstivityY = 1f;
		public State touchPadState = State.Interactable;

		public UnityEvent onGameStart = new UnityEvent();
		public UnityEvent onPressedTouchPad = new UnityEvent();
		public UnityEvent onStartedDraggingTouchPad = new UnityEvent();
		public UnityEvent onStoppedDraggingTouchPad = new UnityEvent();

		private void Start()
		{
			onGameStart?.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (touchPadState == State.Un_Interactable) return;
			if (pressingTouchPad) return;

			pressingTouchPad = true;
			eventPointerID = eventData.pointerId;
			PointerOld = eventData.position;
			touchPadInput = Vector2.zero;
			onPressedTouchPad?.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!pressingTouchPad || eventData.pointerId != eventPointerID) return;

			onStartedDraggingTouchPad?.Invoke();
			Vector2 currentPointer = eventData.position;
			distanceBetweenTouch = currentPointer - PointerOld;
			PointerOld = currentPointer;

			touchPadInput = new Vector2(
				(distanceBetweenTouch.x / (Screen.width > 0 ? Screen.width : 1920f)) * senstivityX * 50f,
				(distanceBetweenTouch.y / (Screen.height > 0 ? Screen.height : 1080f)) * senstivityY * 50f
			);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == eventPointerID)
			{
				pressingTouchPad = false;
				eventPointerID = -1;
				touchPadInput = Vector2.zero;
				distanceBetweenTouch = Vector2.zero;
				onStoppedDraggingTouchPad?.Invoke();
			}
		}

		public float GetVerticalValue() => touchPadInput.y;
		public float GetHorizontalValue() => touchPadInput.x;
		public Vector2 GetHorizontalAndVerticalValue() => touchPadInput;

		public void SetState(State state) => touchPadState = state;
		public State GetState() => touchPadState;

		private void Update()
		{
			// Reset touchPadInput each frame if not updated
			if (!pressingTouchPad)
			{
				touchPadInput = Vector2.zero;
			}
		}
	}
}
