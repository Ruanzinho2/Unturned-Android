using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Ensures fullscreen mode is applied on boot.
	/// </summary>
	public static class AndroidDisplayConfig
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			Screen.fullScreen = true;
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
		}
	}
}
