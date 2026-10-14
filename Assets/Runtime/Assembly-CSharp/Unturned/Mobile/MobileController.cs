using UnityEngine;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Main manager for mobile touch controls.
	/// Clears frame deltas at LateUpdate and controls HUD visibility.
	/// </summary>
	public class MobileController : MonoBehaviour
	{
		public static MobileController Instance { get; private set; }

		[Header("HUD Canvas")]
		public Canvas mobileCanvas;

		[Header("Configuration")]
		public bool autoEnableOnMobile = true;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);

			if (mobileCanvas == null)
			{
				mobileCanvas = GetComponentInChildren<Canvas>(true);
			}

#if UNITY_ANDROID || UNITY_IOS
			if (autoEnableOnMobile && mobileCanvas != null)
			{
				mobileCanvas.gameObject.SetActive(true);
			}
#endif
		}

		private void LateUpdate()
		{
			// Clean up frame-specific key transitions
			InputBroker.ClearFrameDeltas();
		}

		public void SetControlsVisible(bool visible)
		{
			if (mobileCanvas != null)
			{
				mobileCanvas.gameObject.SetActive(visible);
			}
		}
	}
}
