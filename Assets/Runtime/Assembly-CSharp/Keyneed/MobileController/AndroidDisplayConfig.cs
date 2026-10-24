using System;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Configures Android display cutout mode and immersive fullscreen flags
	/// to eliminate black bars (letterboxing) on notched / punch-hole displays.
	/// </summary>
	public class AndroidDisplayConfig : MonoBehaviour
	{
		private static AndroidDisplayConfig instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			if (instance != null) return;

			Screen.fullScreen = true;

			GameObject go = new GameObject("AndroidDisplayConfig");
			DontDestroyOnLoad(go);
			instance = go.AddComponent<AndroidDisplayConfig>();
		}

		private void Awake()
		{
			ApplyCutoutAndImmersiveMode();
		}

		private void Start()
		{
			ApplyCutoutAndImmersiveMode();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				ApplyCutoutAndImmersiveMode();
			}
		}

		public static void ApplyCutoutAndImmersiveMode()
		{
			Screen.fullScreen = true;

#if UNITY_ANDROID && !UNITY_EDITOR
			try
			{
				if (Application.platform != RuntimePlatform.Android) return;

				using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					if (activity == null) return;

					activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
					{
						try
						{
							using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
							{
								if (window == null) return;

								using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
								{
									int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
									if (sdkInt >= 28) // Android 9.0 (Pie) or higher
									{
										using (AndroidJavaObject layoutParams = window.Call<AndroidJavaObject>("getAttributes"))
										{
											// 1 = LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES
											// In Android R / API 30+: 3 = LAYOUT_IN_DISPLAY_CUTOUT_MODE_ALWAYS
											int cutoutMode = (sdkInt >= 30) ? 3 : 1;
											layoutParams.Set("layoutInDisplayCutoutMode", cutoutMode);
											window.Call("setAttributes", layoutParams);
										}
									}
								}

								using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
								{
									if (decorView != null)
									{
										// View.SYSTEM_UI_FLAG_FULLSCREEN = 4
										// View.SYSTEM_UI_FLAG_HIDE_NAVIGATION = 2
										// View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY = 4096
										// View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN = 1024
										// View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION = 512
										// View.SYSTEM_UI_FLAG_LAYOUT_STABLE = 256
										int uiOptions = 4 | 2 | 4096 | 1024 | 512 | 256;
										decorView.Call("setSystemUiVisibility", uiOptions);
									}
								}
							}
						}
						catch (Exception innerEx)
						{
							UnturnedLog.warn($"[AndroidDisplayConfig] runOnUiThread error: {innerEx.Message}");
						}
					}));
				}

				UnturnedLog.info($"[AndroidDisplayConfig] Display resolution: {Screen.width}x{Screen.height}, SafeArea: {Screen.safeArea}");
			}
			catch (Exception ex)
			{
				UnturnedLog.warn($"[AndroidDisplayConfig] Error configuring Android display: {ex.Message}");
			}
#endif
		}
	}
}
