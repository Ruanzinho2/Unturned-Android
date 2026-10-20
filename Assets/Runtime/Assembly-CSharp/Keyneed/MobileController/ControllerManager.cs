using UnityEngine;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Main manager orchestrating edit mode, button customization, and interaction states.
	/// </summary>
	public class ControllerManager : Singleton<ControllerManager>
	{
		public static bool editMode = false;
		public Slider resizeSlider;
		public bool InteractMode = false;

		private void Start()
		{
			if (resizeSlider != null)
			{
				resizeSlider.gameObject.SetActive(false);
			}
		}

		public void PressEditMode()
		{
			editMode = !editMode;
			if (resizeSlider != null)
			{
				resizeSlider.gameObject.SetActive(editMode);
			}

			if (!editMode)
			{
				// Exiting edit mode saves user changes
				SaveManager saveMgr = GetComponent<SaveManager>();
				if (saveMgr != null)
				{
					saveMgr.SaveData();
				}
			}
		}

		public void SetInteract(State state)
		{
			InteractMode = (state == State.Interactable);
		}
	}
}
