using System.Collections.Generic;
using UnityEngine;
using SDG.Unturned.Mobile;

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
		public static bool isSimulatingMouse = false;

		private static readonly HashSet<KeyCode> keysDown = new HashSet<KeyCode>();
		private static readonly HashSet<KeyCode> keysPressed = new HashSet<KeyCode>();
		private static readonly HashSet<KeyCode> keysUp = new HashSet<KeyCode>();

		private static Vector3 _mousePosition;

		public static float GetAxis(string axisName)
		{
			if (axisName == "mouse_x")
			{
				float simulated = SpecialTouchPad.Instance != null ? SpecialTouchPad.Instance.touchPadInput.x : 0f;
				return Input.GetAxis("mouse_x") + simulated;
			}
			if (axisName == "mouse_y")
			{
				float simulated = SpecialTouchPad.Instance != null ? SpecialTouchPad.Instance.touchPadInput.y : 0f;
				return Input.GetAxis("mouse_y") + simulated;
			}
			return Input.GetAxis(axisName);
		}

		public static Vector3 mousePosition
		{
			get => isSimulatingMouse ? _mousePosition : Input.mousePosition;
			set
			{
				_mousePosition = value;
				isSimulatingMouse = true;
			}
		}

		// Backward compatibility property
		public static bool isSimulation
		{
			get => isSimulatingMouse;
			set => isSimulatingMouse = value;
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

		public static void SetKeyUp(KeyCode key)
		{
			if (key == KeyCode.None) return;
			if (keysPressed.Contains(key))
			{
				keysPressed.Remove(key);
				keysUp.Add(key);
			}
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
