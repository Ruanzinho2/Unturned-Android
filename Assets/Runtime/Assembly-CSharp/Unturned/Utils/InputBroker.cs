using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Virtual input abstraction layer for mobile touch controls and simulation.
	/// Enables simulated button presses, touches, and mouse coordinates to feed
	/// transparently into Unturned's input system (InputEx, PlayerMovement, PlayerInput).
	/// Works concurrently with physical keyboard and mouse without conflicts.
	/// </summary>
	public static class InputBroker
	{
		static InputBroker()
		{
#if UNITY_ANDROID
			Input.simulateMouseWithTouches = false;
#endif
		}

		public static bool isSimulation = false;
		public static bool isSimulatingMouse = false;

		private static readonly HashSet<KeyCode> keysDown = new HashSet<KeyCode>();
		private static readonly HashSet<KeyCode> keysPressed = new HashSet<KeyCode>();
		private static readonly HashSet<KeyCode> keysUp = new HashSet<KeyCode>();

		private static Vector3 _mousePosition;

		public static float GetAxis(string axisName)
		{
			if (axisName == "mouse_x")
			{
				float simulated = UltimateTouchpad.GetHorizontalAxis("LookTouchPad_8tdg5543ffre");
#if UNITY_ANDROID
				// When touchscreen is active, suppress Unity's touch-to-mouse axis emulation
				// which produces wild jumps between multiple fingers.
				if (Input.touchCount > 0)
				{
					return simulated;
				}
#endif
				return Input.GetAxis("mouse_x") + simulated;
			}
			if (axisName == "mouse_y")
			{
				float simulated = UltimateTouchpad.GetVerticalAxis("LookTouchPad_8tdg5543ffre");
#if UNITY_ANDROID
				// Suppress multitouch cross-finger delta jumps
				if (Input.touchCount > 0)
				{
					return simulated;
				}
#endif
				return Input.GetAxis("mouse_y") + simulated;
			}
			return Input.GetAxis(axisName);
		}

		public static Vector3 mousePosition
		{
			get => (isSimulation || isSimulatingMouse) ? _mousePosition : Input.mousePosition;
			set
			{
				_mousePosition = value;
				isSimulatingMouse = true;
				isSimulation = true;
			}
		}

		public static void SetKeyDown(KeyCode key)
		{
			if (key == KeyCode.None) return;
			if (!keysPressed.Contains(key))
			{
				keysDown.Add(key);
				keysPressed.Add(key);
			}
		}

		public static void SetKey(KeyCode key)
		{
			SetKeyDown(key);
		}

		public static void SetKeyUp(KeyCode key)
		{
			if (key == KeyCode.None) return;
			if (keysPressed.Contains(key))
			{
				keysPressed.Remove(key);
				keysUp.Add(key);
			}
		}

		public static void DeleteKey(KeyCode key)
		{
			keysPressed.Remove(key);
			keysDown.Remove(key);
			keysUp.Remove(key);
		}

		public static void ClickKey(KeyCode key)
		{
			SetKeyDown(key);
		}

		public static void HoldKey(KeyCode key)
		{
			SetKeyDown(key);
		}

		public static void ReleaseKey(KeyCode key)
		{
			SetKeyUp(key);
		}

		public static bool GetKey(KeyCode key)
		{
			return keysPressed.Contains(key);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return keysDown.Contains(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return keysUp.Contains(key);
		}

		public static bool anyKey => keysPressed.Count > 0 || Input.anyKey;
		public static bool anyKeyDown => keysDown.Count > 0 || Input.anyKeyDown;

		public static void ClearFrameDeltas()
		{
			keysDown.Clear();
			keysUp.Clear();
		}
	}

	/// <summary>
	/// Cursor locking broker that prevents mobile touchscreens from being locked
	/// or hidden like a PC desktop cursor.
	/// </summary>
	public static class ScreenBroker
	{
		private static bool m_lockCursor;

		public static bool lockCursor
		{
			get => m_lockCursor;
			set
			{
				m_lockCursor = value;
#if !UNITY_ANDROID
				Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !value;
#endif
			}
		}
	}
}
