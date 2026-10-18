using UnityEngine;

namespace SDG.Unturned
{
	public partial class LevelLighting
	{
		static partial void UpdateSunShafts(Transform sunTransform, Color sunColor)
		{
			MobileSunShafts.sunTransform = sunTransform;
			MobileSunShafts.sunColor = sunColor;
		}

		static partial void UpdateSunShaftsIntensity(float intensity)
		{
			MobileSunShafts.sunIntensity = intensity;
		}
	}

	public partial class GraphicsSettings
	{
		static partial void ApplySunShaftsSettings()
		{
			if (UnturnedPostProcess.instance != null)
			{
				UnturnedPostProcess.instance.syncSunShafts();
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void InitPlayerGrassDisplacement()
		{
			Player.onPlayerCreated += (player) =>
			{
				if (player != null && player.channel != null && player.channel.IsLocalPlayer)
				{
					if (player.GetComponent<GrassDisplacement>() == null)
					{
						player.gameObject.AddComponent<GrassDisplacement>();
					}
				}
			};
		}
	}
}
