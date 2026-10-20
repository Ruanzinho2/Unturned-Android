using System;
using System.Collections.Generic;

namespace Keyneed.MobileController
{
	[Serializable]
	public class CustomizableButtons
	{
		public string buttonName;
		public float buttonPosX;
		public float buttonPosY;
		public float buttonWidth;
		public float buttonHeight;

		public CustomizableButtons()
		{
		}

		public CustomizableButtons(string name, float posX, float posY, float width, float height)
		{
			buttonName = name;
			buttonPosX = posX;
			buttonPosY = posY;
			buttonWidth = width;
			buttonHeight = height;
		}
	}

	[Serializable]
	public class ButtonsData
	{
		public List<CustomizableButtons> customizableButtonsList = new List<CustomizableButtons>();
	}
}
