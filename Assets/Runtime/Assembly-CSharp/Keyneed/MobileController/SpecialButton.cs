using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using SDG.Unturned;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Virtual button capable of simulating KeyCodes via InputBroker,
	/// triggering UnityEvents, and optionally streaming pointer events into camera look rotation (UltimateTouchpad).
	/// Faithful reproduction of the Russian Unturned APK IL2CPP 0x107865C - 0x1078F34.
	/// </summary>
	public class SpecialButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
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
			if (m_Selected) return;

			if (keySimulate != KeyCode.None)
			{
				InputBroker.SetKeyDown(keySimulate);
			}

			m_Selected = true;
			OnSelected();
			onButtondown?.Invoke();

			if (lookSimulate)
			{
				UltimateTouchpad ut = UltimateTouchpad.GetUltimateTouchpad("LookTouchPad_8tdg5543ffre");
				if (ut != null)
				{
					ut.OnPointerDown(eventData);
				}
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (buttonState == State.Un_Interactable) return;

			if (lookSimulate)
			{
				UltimateTouchpad ut = UltimateTouchpad.GetUltimateTouchpad("LookTouchPad_8tdg5543ffre");
				if (ut != null)
				{
					ut.OnDrag(eventData);
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (ControllerManager.editMode) return;
			if (buttonState == State.Un_Interactable) return;
			if (!m_Selected) return;

			if (lookSimulate)
			{
				UltimateTouchpad ut = UltimateTouchpad.GetUltimateTouchpad("LookTouchPad_8tdg5543ffre");
				if (ut != null)
				{
					ut.OnPointerUp(eventData);
				}
			}

			m_Selected = false;

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

		public void ResetButton()
		{
			if (m_Selected)
			{
				m_Selected = false;
				if (keySimulate != KeyCode.None)
				{
					InputBroker.DeleteKey(keySimulate);
					InputBroker.SetKeyUp(keySimulate);
				}
				onButtonup?.Invoke();
			}
		}

		private void Update()
		{
			if (m_Selected)
			{
				if (RussianHUDBuilder.IsAnyUIOpen())
				{
					bool isInventoryButton = RussianHUDBuilder.ShouldShowTabButton() && (keySimulate == KeyCode.Tab || gameObject.name == "Inventory");
					if (!isInventoryButton)
					{
						ResetButton();
					}
				}
				else if (RussianHUDBuilder.IsInVehicle())
				{
					if (m_Patterns != null && m_Patterns.m_ButtonPattern != ButtonPattern.Vehicle && !RussianHUDBuilder.IsGlobalButton(gameObject.name))
					{
						ResetButton();
					}
				}
				else
				{
					if (m_Patterns != null && m_Patterns.m_ButtonPattern == ButtonPattern.Vehicle)
					{
						ResetButton();
					}
				}
			}
		}
	}
}
