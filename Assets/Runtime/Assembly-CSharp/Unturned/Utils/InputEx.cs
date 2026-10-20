////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Extensions to the built-in Input class.
	/// </summary>
	public static class InputEx
	{
		private static bool IsPhysicalKeyPressed(KeyCode key)
		{
#if UNITY_ANDROID
			// On Android touchscreen devices, any touch emulates Mouse0/Mouse1 in legacy Input.
			// When the screen is being touched (touchCount > 0), ignore touch-emulated mouse clicks
			// so that touching the joystick or screen does not trigger attacking/aiming.
			// Real physical mice (USB/Bluetooth) do not produce Input.touchCount > 0.
			if ((key == KeyCode.Mouse0 || key == KeyCode.Mouse1) && Input.touchCount > 0)
			{
				return false;
			}
#endif
			return Input.GetKey(key);
		}

		private static bool IsPhysicalKeyDown(KeyCode key)
		{
#if UNITY_ANDROID
			if ((key == KeyCode.Mouse0 || key == KeyCode.Mouse1) && Input.touchCount > 0)
			{
				return false;
			}
#endif
			return Input.GetKeyDown(key);
		}

		private static bool IsPhysicalKeyUp(KeyCode key)
		{
#if UNITY_ANDROID
			if ((key == KeyCode.Mouse0 || key == KeyCode.Mouse1) && Input.touchCount > 0)
			{
				return false;
			}
#endif
			return Input.GetKeyUp(key);
		}

		/// <summary>
		/// Wrapper for Input.GetKey, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKey(KeyCode key)
		{
			return (IsPhysicalKeyPressed(key) || InputBroker.GetKey(key)) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Wrapper for Input.GetKeyDown, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKeyDown(KeyCode key)
		{
			return (IsPhysicalKeyDown(key) || InputBroker.GetKeyDown(key)) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Wrapper for Input.GetKeyUp, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKeyUp(KeyCode key)
		{
			return (IsPhysicalKeyUp(key) || InputBroker.GetKeyUp(key)) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Should be used anywhere that Input.GetKeyDown opens a UI.
		///
		/// Each frame one input event can be consumed. This is a hack to prevent multiple UI-related key presses from
		/// interfering during the same frame. Only the first input event proceeds, while the others are ignored.
		/// </summary>
		/// <returns>True if caller should proceed, false otherwise.</returns>
		public static bool ConsumeKeyDown(KeyCode key)
		{
			return GetKeyDown(key) && keyGuard.Consume();
		}

		/// <summary>
		/// Get mouse position in viewport coordinates where zero is the bottom left and one is the top right.
		/// </summary>
		public static Vector2 NormalizedMousePosition
		{
			get
			{
				Vector2 mousePosition = InputBroker.isSimulation ? (Vector2)InputBroker.mousePosition : (Vector2)Input.mousePosition;
				mousePosition.x /= Screen.width;
				mousePosition.y /= Screen.height;
				return mousePosition;
			}
		}

		private static OncePerFrameGuard keyGuard;
	}
}
