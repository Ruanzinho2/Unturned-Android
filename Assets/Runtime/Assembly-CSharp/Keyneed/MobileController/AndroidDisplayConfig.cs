using System;
using System.Reflection;
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

				Type unityPlayerType = Type.GetType("UnityEngine.AndroidJavaClass, UnityEngine.AndroidJNIModule")
					?? Type.GetType("UnityEngine.AndroidJavaClass, UnityEngine");

				if (unityPlayerType != null)
				{
					object unityPlayer = Activator.CreateInstance(unityPlayerType, "com.unity3d.player.UnityPlayer");
					MethodInfo getStaticMethod = unityPlayerType.GetMethod("GetStatic");
					Type androidJavaObjectType = Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule")
						?? Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine");

					if (getStaticMethod != null && androidJavaObjectType != null)
					{
						MethodInfo getStaticActivity = getStaticMethod.MakeGenericMethod(androidJavaObjectType);
						object activity = getStaticActivity.Invoke(unityPlayer, new object[] { "currentActivity" });

						if (activity != null)
						{
							Type runnableType = Type.GetType("UnityEngine.AndroidJavaRunnable, UnityEngine.AndroidJNIModule")
								?? Type.GetType("UnityEngine.AndroidJavaRunnable, UnityEngine");

							Action runAction = () =>
							{
								try
								{
									MethodInfo callWindow = androidJavaObjectType.GetMethod("Call", new Type[] { typeof(string), typeof(object[]) });
									if (callWindow == null)
										callWindow = androidJavaObjectType.GetMethod("Call", new Type[] { typeof(string) });

									object window = callWindow?.Invoke(activity, new object[] { "getWindow", Array.Empty<object>() });
									if (window != null)
									{
										object versionClass = Activator.CreateInstance(unityPlayerType, "android.os.Build$VERSION");
										MethodInfo getStaticInt = getStaticMethod.MakeGenericMethod(typeof(int));
										int sdkInt = (int)(getStaticInt?.Invoke(versionClass, new object[] { "SDK_INT" }) ?? 0);

										if (sdkInt >= 28)
										{
											object layoutParams = callWindow.Invoke(window, new object[] { "getAttributes", Array.Empty<object>() });
											if (layoutParams != null)
											{
												MethodInfo setField = androidJavaObjectType.GetMethod("Set");
												MethodInfo setIntField = setField?.MakeGenericMethod(typeof(int));
												int cutoutMode = (sdkInt >= 30) ? 3 : 1;
												setIntField?.Invoke(layoutParams, new object[] { "layoutInDisplayCutoutMode", cutoutMode });

												MethodInfo callVoid = androidJavaObjectType.GetMethod("Call", new Type[] { typeof(string), typeof(object[]) });
												callVoid?.Invoke(window, new object[] { "setAttributes", new object[] { layoutParams } });
											}
										}

										object decorView = callWindow.Invoke(window, new object[] { "getDecorView", Array.Empty<object>() });
										if (decorView != null)
										{
											int uiOptions = 4 | 2 | 4096 | 1024 | 512 | 256;
											MethodInfo callVoid = androidJavaObjectType.GetMethod("Call", new Type[] { typeof(string), typeof(object[]) });
											callVoid?.Invoke(decorView, new object[] { "setSystemUiVisibility", new object[] { uiOptions } });
										}
									}
								}
								catch (Exception innerEx)
								{
									UnturnedLog.warn($"[AndroidDisplayConfig] runOnUiThread error: {innerEx.Message}");
								}
							};

							if (runnableType != null)
							{
								Delegate runnable = Delegate.CreateDelegate(runnableType, runAction.Target, runAction.Method);
								MethodInfo callActivity = androidJavaObjectType.GetMethod("Call", new Type[] { typeof(string), typeof(object[]) });
								callActivity?.Invoke(activity, new object[] { "runOnUiThread", new object[] { runnable } });
							}
						}
					}
				}

				UnturnedLog.info($"[AndroidDisplayConfig] Screen resolution: {Screen.width}x{Screen.height}, SafeArea: {Screen.safeArea}");
			}
			catch (Exception ex)
			{
				UnturnedLog.warn($"[AndroidDisplayConfig] Error configuring Android display: {ex.Message}");
			}
#endif
		}
	}
}
