using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Virtual touch button that injects a simulated KeyCode into InputBroker.
	/// Used for Jump, Crouch, Prone, Sprint, Reload, Interact, Fire, etc.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class SpecialButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[Header("Input Simulation")]
		[Tooltip("The KeyCode to simulate when this virtual button is held down.")]
		public KeyCode keySimulate = KeyCode.Space;

		[Header("Visual Feedback")]
		public CanvasGroup m_CanvasGroup;
		[Range(0f, 1f)]
		public float m_AlphaVisibility = 0.85f;
		[Range(0f, 1f)]
		public float pressedAlpha = 1.0f;
		[Range(0f, 1f)]
		public float idleAlpha = 0.6f;

		private bool isPressed = false;

		private void Awake()
		{
			if (m_CanvasGroup == null)
			{
				m_CanvasGroup = GetComponent<CanvasGroup>();
			}
			SetAlpha(idleAlpha);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isPressed = true;
			SetAlpha(pressedAlpha);
			if (keySimulate != KeyCode.None)
			{
				InputBroker.SetKeyDown(keySimulate);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			isPressed = false;
			SetAlpha(idleAlpha);
			if (keySimulate != KeyCode.None)
			{
				InputBroker.SetKeyUp(keySimulate);
			}
		}

		private void OnDisable()
		{
			if (isPressed && keySimulate != KeyCode.None)
			{
				isPressed = false;
				InputBroker.SetKeyUp(keySimulate);
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
