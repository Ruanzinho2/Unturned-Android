using UnityEngine;
using UnityEngine.EventSystems;

public class UltimateTouchpadScreenSizeUpdater : UIBehaviour
{
	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		YieldPositioning();
	}

	public void YieldPositioning()
	{
		UltimateTouchpad touchpad = GetComponent<UltimateTouchpad>();
		if (touchpad != null)
		{
			touchpad.UpdateTouchpadPositioning();
		}
	}
}
