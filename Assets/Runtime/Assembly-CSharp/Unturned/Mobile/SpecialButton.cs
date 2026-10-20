using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned.Mobile
{
	public enum EButtonAction
	{
		Custom,
		Primary,     // ControlsSettings.primary (Attack / Shoot / Action)
		Secondary,   // ControlsSettings.secondary (Aim / Scope)
		Jump,        // ControlsSettings.jump
		Crouch,      // ControlsSettings.crouch
		Prone,       // ControlsSettings.prone
		Sprint,      // ControlsSettings.sprint
		Reload,      // ControlsSettings.reload
		Interact,    // ControlsSettings.interact
		Inventory,   // ControlsSettings.inventory
		Perspective, // ControlsSettings.perspective
		Tactical,    // ControlsSettings.tactical
		Pause        // KeyCode.Escape
	}

	/// <summary>
	/// Virtual touch button that injects a simulated KeyCode into InputBroker.
	/// Supports dynamic action key remapping via ControlsSettings and tactile visual feedback.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class SpecialButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[Header("Action Configuration")]
		public EButtonAction action = EButtonAction.Custom;

		[Tooltip("The fallback KeyCode to simulate when action is set to Custom.")]
		public KeyCode keySimulate = KeyCode.Space;

		[Header("Visual Feedback")]
		public CanvasGroup m_CanvasGroup;
		[Range(0f, 1f)]
		public float m_AlphaVisibility = 0.85f;
		[Range(0f, 1f)]
		public float pressedAlpha = 1.0f;
		[Range(0f, 1f)]
		public float idleAlpha = 0.65f;

		public bool isPressed { get; private set; }
		private int activePointerId = -1;
		private Vector3 originalScale = Vector3.one;
		private KeyCode boundKey = KeyCode.None;

		private void Awake()
		{
			if (m_CanvasGroup == null)
			{
				m_CanvasGroup = GetComponent<CanvasGroup>();
			}
			originalScale = transform.localScale;
			SetAlpha(idleAlpha);
		}

		public KeyCode GetTargetKey()
		{
			switch (action)
			{
				case EButtonAction.Primary: return ControlsSettings.primary;
				case EButtonAction.Secondary: return ControlsSettings.secondary;
				case EButtonAction.Jump: return ControlsSettings.jump;
				case EButtonAction.Crouch: return ControlsSettings.crouch;
				case EButtonAction.Prone: return ControlsSettings.prone;
				case EButtonAction.Sprint: return ControlsSettings.sprint;
				case EButtonAction.Reload: return ControlsSettings.reload;
				case EButtonAction.Interact: return ControlsSettings.interact;
				case EButtonAction.Inventory: return ControlsSettings.inventory;
				case EButtonAction.Perspective: return ControlsSettings.perspective;
				case EButtonAction.Tactical: return ControlsSettings.tactical;
				case EButtonAction.Pause: return KeyCode.Escape;
				default: return keySimulate;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (activePointerId != -1) return;

			activePointerId = eventData.pointerId;
			isPressed = true;
			transform.localScale = originalScale * 0.92f;
			SetAlpha(pressedAlpha);

			boundKey = GetTargetKey();
			if (boundKey != KeyCode.None)
			{
				InputBroker.SetKeyDown(boundKey);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId == activePointerId)
			{
				ReleaseButton();
			}
		}

		private void ReleaseButton()
		{
			activePointerId = -1;
			isPressed = false;
			transform.localScale = originalScale;
			SetAlpha(idleAlpha);

			if (boundKey != KeyCode.None)
			{
				InputBroker.SetKeyUp(boundKey);
				boundKey = KeyCode.None;
			}
		}

		private void OnDisable()
		{
			if (isPressed)
			{
				ReleaseButton();
			}
		}

		private void SetAlpha(float alpha)
		{
			if (m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = alpha * m_AlphaVisibility;
			}
		}
	}
}
