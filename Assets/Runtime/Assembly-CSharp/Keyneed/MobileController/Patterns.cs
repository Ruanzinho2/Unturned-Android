using UnityEngine;
using SDG.Unturned;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Manages context-sensitive visibility (e.g., vehicle controls only while inside vehicle).
	/// </summary>
	public class Patterns : MonoBehaviour
	{
		public ButtonPattern m_ButtonPattern = ButtonPattern.None;
		public CanvasGroup m_CanvasGroup;
		public SpecialButton m_SpecialButton;
		private string _sceneNameTemp;

		private void Awake()
		{
			if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
			if (m_SpecialButton == null) m_SpecialButton = GetComponent<SpecialButton>();
		}

		private void LateUpdate()
		{
			if (m_CanvasGroup == null) return;

			if (ControllerManager.editMode)
			{
				m_CanvasGroup.alpha = 1f;
				m_CanvasGroup.interactable = true;
				m_CanvasGroup.blocksRaycasts = true;
				return;
			}

			bool isAnyUIOpen = RussianHUDBuilder.IsAnyUIOpen();
			bool isInventoryOpen = RussianHUDBuilder.IsInventoryOpen();

			if (isAnyUIOpen)
			{
				// Exception: Keep the Tab (Inventory) button active and visible while inventory/dashboard is open
				bool isInventoryButton = isInventoryOpen && ((m_SpecialButton != null && m_SpecialButton.keySimulate == KeyCode.Tab) || gameObject.name == "Inventory");

				float alpha = isInventoryButton ? (m_SpecialButton != null ? m_SpecialButton.m_AlphaVisibility : 1f) : 0f;
				m_CanvasGroup.alpha = alpha;
				m_CanvasGroup.interactable = isInventoryButton;
				m_CanvasGroup.blocksRaycasts = isInventoryButton;
				return;
			}

			bool visible = true;
			switch (m_ButtonPattern)
			{
				case ButtonPattern.Vehicle:
					visible = Player.player != null && Player.player.movement != null && Player.player.movement.getVehicle() != null;
					break;

				case ButtonPattern.Weapon:
					visible = Player.player != null && Player.player.equipment != null && Player.player.equipment.useable != null;
					break;

				case ButtonPattern.Interact:
					visible = PlayerInteract.interactable != null;
					break;

				case ButtonPattern.Network:
				case ButtonPattern.None:
				default:
					visible = true;
					break;
			}

			float targetAlpha = visible ? (m_SpecialButton != null ? m_SpecialButton.m_AlphaVisibility : 1f) : 0f;
			m_CanvasGroup.alpha = targetAlpha;
			m_CanvasGroup.interactable = visible;
			m_CanvasGroup.blocksRaycasts = visible;
		}
	}
}
