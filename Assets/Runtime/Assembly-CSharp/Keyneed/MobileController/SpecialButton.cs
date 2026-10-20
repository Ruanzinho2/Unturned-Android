using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SDG.Unturned;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual button capable of simulating KeyCodes via InputBroker,
	/// triggering UnityEvents, and optionally streaming drag deltas into camera look rotation.
	/// </summary>
	public class SpecialButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public UnityEvent onButtondown = new UnityEvent();
		public UnityEvent onSelect = new UnityEvent();
		public UnityEvent onButtonup = new UnityEvent();

		public KeyCode keySimulate = KeyCode.None;
		public bool lookSimulate = false;
		public State buttonState = State.Interactable;

		public CanvasGroup m_CanvasGroup;
		public Patterns m_Patterns;

		public float m_AlphaVisibility = 1f;
		public float m_TimeToHide = 0f;
		public bool m_Selected = false;
		private string _sceneNameTemp;

		public float AlphaVisibility => m_AlphaVisibility;

		private void Start()
		{
			if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
			if (m_Patterns == null) m_Patterns = GetComponent<Patterns>();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (buttonState == State.Un_Interactable) return;

			if (keySimulate != KeyCode.None)
			{
				InputBroker.SetKeyDown(keySimulate);
			}

			onButtondown?.Invoke();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (buttonState == State.Un_Interactable) return;

			if (lookSimulate)
			{
				UltimateTouchpad touchpad = UltimateTouchpad.GetUltimateTouchpad("LookTouchPad_8tdg5543ffre");
				if (touchpad != null)
				{
					touchpad.FeedExternalLookDelta(eventData.delta);
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (buttonState == State.Un_Interactable) return;

			if (keySimulate != KeyCode.None)
			{
				InputBroker.SetKeyUp(keySimulate);
			}

			onButtonup?.Invoke();
		}

		public void OnSelected()
		{
			onSelect?.Invoke();
		}

		public void SetState(State state)
		{
			buttonState = state;
		}

		public State GetState() => buttonState;

		private void Update()
		{
		}
	}
}
