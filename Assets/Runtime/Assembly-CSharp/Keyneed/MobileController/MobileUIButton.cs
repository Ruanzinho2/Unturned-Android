using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Keyneed.MobileController
{
	/// <summary>
	/// Component attached to customizable buttons enabling repositioning and resizing in edit mode.
	/// </summary>
	public class MobileUIButton : MonoBehaviour, IDragHandler, IPointerDownHandler
	{
		public bool buttonExists = true;
		public bool isSelected = false;
		public RectTransform rectTransform;
		public Slider resizeSilder;
		public Color defualtColor = Color.white;
		public static List<MobileUIButton> buttonList = new List<MobileUIButton>();
		public string resizeSliderName = "SliderSizeRegulator_048gr3gerbr5535g";
		public Canvas myCanvas;

		private Vector2 defaultPosition;
		private Vector2 defaultSize;

		private void Start()
		{
			if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
			if (myCanvas == null) myCanvas = GetComponentInParent<Canvas>();

			if (rectTransform != null)
			{
				defaultPosition = rectTransform.anchoredPosition;
				defaultSize = rectTransform.sizeDelta;
			}

			Image img = GetComponent<Image>();
			if (img != null) defualtColor = img.color;

			if (!buttonList.Contains(this))
			{
				buttonList.Add(this);
			}

			if (resizeSilder == null)
			{
				GameObject sliderGo = GameObject.Find(resizeSliderName);
				if (sliderGo != null)
				{
					resizeSilder = sliderGo.GetComponent<Slider>();
				}
			}
		}

		public void PressButton()
		{
			if (!ControllerManager.editMode) return;

			foreach (var b in buttonList)
			{
				if (b != null && b != this)
				{
					b.isSelected = false;
				}
			}
			isSelected = true;

			if (resizeSilder != null)
			{
				resizeSilder.onValueChanged.RemoveAllListeners();
				resizeSilder.minValue = 40f;
				resizeSilder.maxValue = 250f;
				resizeSilder.value = rectTransform.sizeDelta.x;
				resizeSilder.onValueChanged.AddListener((float val) =>
				{
					if (isSelected && rectTransform != null)
					{
						rectTransform.sizeDelta = new Vector2(val, val);
					}
				});
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (ControllerManager.editMode)
			{
				PressButton();
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (ControllerManager.editMode && isSelected && rectTransform != null)
			{
				float scale = myCanvas != null ? myCanvas.scaleFactor : 1f;
				rectTransform.anchoredPosition += eventData.delta / scale;
			}
		}

		public void SetDefaultValues()
		{
			if (rectTransform != null)
			{
				rectTransform.anchoredPosition = defaultPosition;
				rectTransform.sizeDelta = defaultSize;
			}
		}

		public void LoadButtonData()
		{
			// Coordinates loaded by SaveManager
		}

		private void Update()
		{
		}
	}
}
