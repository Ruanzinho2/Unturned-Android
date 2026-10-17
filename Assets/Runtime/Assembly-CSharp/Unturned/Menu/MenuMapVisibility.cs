////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDG.Unturned
{
	/// <summary>
	/// Component in the root Menu scene.
	/// Additively loads decoration levels without modifying main scene.
	/// </summary>
	public class MenuMapVisibility : MonoBehaviour
	{
		public void Awake()
		{
			if (HelperClass.clNoAdditiveMenu)
			{
				UnturnedLog.info("Skipping loading of additive menu scenes");
				return;
			}

			UnturnedLog.info("Loading additive default menu (DefaultMenu)");
			if (Application.CanStreamedLevelBeLoaded("DefaultMenu"))
			{
				SceneManager.LoadSceneAsync("DefaultMenu", LoadSceneMode.Additive);
			}
			else
			{
				SceneManager.LoadSceneAsync("Menu_Fallback", LoadSceneMode.Additive);
			}
		}

		/// <summary>
		/// Prevents static member from being initialized during MonoBehaviour construction. (Unity warning)
		/// </summary>
		private static class HelperClass
		{
			public static CommandLineString clAdditiveMenuOverride = new CommandLineString("-AdditiveMenuOverride");
			public static CommandLineFlag clNoAdditiveMenu = new CommandLineFlag(false, "-NoAdditiveMenu");
		}
	}
}
