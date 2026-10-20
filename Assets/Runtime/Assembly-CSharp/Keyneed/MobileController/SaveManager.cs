using System.Collections.Generic;
using UnityEngine;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Manages saving and loading of custom button positions and sizes.
	/// </summary>
	public class SaveManager : MonoBehaviour
	{
		public ButtonsData buttonsData;

		private void Start()
		{
			LoadData();
		}

		public void SaveData()
		{
			buttonsData = new ButtonsData();
			MobileUIButton[] buttons = FindObjectsOfType<MobileUIButton>();
			foreach (var btn in buttons)
			{
				if (btn == null) continue;
				RectTransform rt = btn.GetComponent<RectTransform>();
				if (rt == null) continue;

				buttonsData.customizableButtonsList.Add(new CustomizableButtons(
					btn.gameObject.name,
					rt.anchoredPosition.x,
					rt.anchoredPosition.y,
					rt.sizeDelta.x,
					rt.sizeDelta.y
				));
			}

			SaveSystem.SaveGame(buttonsData);
		}

		public void LoadData()
		{
			buttonsData = SaveSystem.LoadData();
			if (buttonsData == null || buttonsData.customizableButtonsList == null) return;

			foreach (var data in buttonsData.customizableButtonsList)
			{
				if (string.IsNullOrEmpty(data.buttonName)) continue;
				GameObject go = GameObject.Find(data.buttonName);
				if (go != null)
				{
					RectTransform rt = go.GetComponent<RectTransform>();
					if (rt != null)
					{
						rt.anchoredPosition = new Vector2(data.buttonPosX, data.buttonPosY);
						rt.sizeDelta = new Vector2(data.buttonWidth, data.buttonHeight);
					}
				}
			}
		}
	}
}
