using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned
{
	/// <summary>
	/// Dedicated graphics and shader diagnostics logger for Unturned Android.
	/// Records GPU capabilities, shader compilation/initialization status,
	/// rendering warnings/errors, and graphics settings changes to Logs/Shaders.log.
	/// </summary>
	public static class ShaderLog
	{
		private static LogFile shaderLogFile = null;
		private static readonly object fileLock = new object();
		private static bool isInitialized = false;

		// Anti-spam throttling: prevents identical messages from flooding the log every frame
		private static readonly Dictionary<string, int> suppressedCount = new Dictionary<string, int>(StringComparer.Ordinal);
		private static readonly HashSet<string> loggedUniqueEvents = new HashSet<string>(StringComparer.Ordinal);
		private const int MAX_UNIQUE_TRACKED = 500;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			if (isInitialized)
			{
				return;
			}

			lock (fileLock)
			{
				if (isInitialized)
				{
					return;
				}

				isInitialized = true;
				SetupLogFile();
				LogSystemEnvironment();
				RunInitialShaderAudit();
			}
		}

		private static void SetupLogFile()
		{
			try
			{
				string basePath = ReadWrite.PATH;
				if (string.IsNullOrEmpty(basePath))
				{
					basePath = Application.persistentDataPath;
				}

				string logsDir = Path.Combine(basePath, "Logs");
				if (!Directory.Exists(logsDir))
				{
					Directory.CreateDirectory(logsDir);
				}

				string logFilePath = Path.Combine(logsDir, "Shaders.log");

				// Rotate previous log if exists
				if (File.Exists(logFilePath))
				{
					string prevPath = Path.Combine(logsDir, "Shaders_Prev.log");
					try
					{
						if (File.Exists(prevPath))
						{
							File.Delete(prevPath);
						}
						File.Move(logFilePath, prevPath);
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"[ShaderLog] Failed to rotate Shaders.log to Shaders_Prev.log: {ex.Message}");
					}
				}

				shaderLogFile = new LogFile(logFilePath);

				// Register application quit hook to flush and close
				Application.quitting += Close;
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[ShaderLog] Failed to initialize Shaders.log: {ex.Message}");
			}
		}

		public static void Close()
		{
			lock (fileLock)
			{
				if (shaderLogFile != null)
				{
					try
					{
						WriteLineRaw($"--- Closed Shaders.log at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC ---");
						shaderLogFile.close();
					}
					catch
					{
					}
					finally
					{
						shaderLogFile = null;
					}
				}
			}
		}

		private static void WriteLineRaw(string line)
		{
			if (shaderLogFile != null)
			{
				try
				{
					shaderLogFile.writeLine(line);
				}
				catch
				{
				}
			}
		}

		private static void LogEntry(string tag, string stage, string message)
		{
			if (shaderLogFile == null)
			{
				return;
			}

			string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
			string stagePrefix = string.IsNullOrEmpty(stage) ? "" : $"[{stage}] ";
			string formattedLine = $"[{time}] [{tag}] {stagePrefix}{message}";

			lock (fileLock)
			{
				WriteLineRaw(formattedLine);
			}
		}

		#region Public Logging API

		public static void Info(string stage, string message)
		{
			LogEntry("INFO", stage, message);
		}

		public static void Warning(string stage, string message, bool throttle = true)
		{
			if (throttle && ShouldThrottle(stage + ":" + message))
			{
				return;
			}
			LogEntry("WARN", stage, message);
		}

		public static void Error(string stage, string message, Exception ex = null, bool throttle = true)
		{
			string fullMsg = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
			if (throttle && ShouldThrottle(stage + ":" + fullMsg))
			{
				return;
			}
			LogEntry("ERROR", stage, fullMsg);
		}

		/// <summary>
		/// Record shader registration/initialization state.
		/// </summary>
		public static void LogShaderInit(string shaderName, Shader shader, string stage = "Init")
		{
			if (shader == null)
			{
				Error(stage, $"Shader '{shaderName}' was NOT found! Check shader bundle or build inclusion.", null, throttle: false);
				return;
			}

			bool supported = shader.isSupported;
			if (supported)
			{
				LogEntry("SHADER_OK", stage, $"Shader '{shaderName}' loaded successfully (isSupported=True, maximumLOD={shader.maximumLOD})");
			}
			else
			{
				LogEntry("SHADER_FAIL", stage, $"Shader '{shaderName}' is NOT supported by the current GPU/Graphics API (isSupported=False). Subshaders failed to compile for {SystemInfo.graphicsDeviceType}.");
			}
		}

		/// <summary>
		/// Record graphics setting change and its concrete pipeline effects.
		/// </summary>
		public static void LogGraphicsSetting(string settingName, object value, string pipelineEffect)
		{
			LogEntry("CONFIG", "Settings", $"{settingName} set to [{value}]. Effect on pipeline: {pipelineEffect}");
		}

		/// <summary>
		/// Intercept and record graphics-related Unity internal engine log messages.
		/// </summary>
		public static void LogUnityGraphicsMessage(string text, string stack, LogType type)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			// Filter only graphics/shader/GPU relevant messages
			if (!IsGraphicsRelated(text))
			{
				return;
			}

			string key = text.Trim();
			if (ShouldThrottle(key))
			{
				return;
			}

			string tag;
			switch (type)
			{
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					tag = "UNITY_GFX_ERROR";
					break;
				case LogType.Warning:
					tag = "UNITY_GFX_WARN";
					break;
				default:
					tag = "UNITY_GFX_INFO";
					break;
			}

			string message = text;
			if (type == LogType.Error || type == LogType.Exception)
			{
				if (!string.IsNullOrEmpty(stack))
				{
					message = $"{text}\nStack Trace:\n{stack}";
				}
			}

			LogEntry(tag, "UnityEngine", message);
		}

		#endregion

		#region Throttling & Filtering

		private static bool ShouldThrottle(string key)
		{
			lock (fileLock)
			{
				if (suppressedCount.TryGetValue(key, out int count))
				{
					suppressedCount[key] = count + 1;
					if (count == 10 || count == 100 || count == 1000)
					{
						WriteLineRaw($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [THROTTLE] Previous message repeated {count} times.");
					}
					return true;
				}

				if (suppressedCount.Count < MAX_UNIQUE_TRACKED)
				{
					suppressedCount[key] = 1;
				}
				return false;
			}
		}

		private static bool IsGraphicsRelated(string text)
		{
			if (text.IndexOf("shader", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("vulkan", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("opengl", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("gles", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("gl_", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("vk_", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("material", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("rendertexture", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("postprocess", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("depthtexture", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("depth buffer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("framebuffer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("gpu", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("bloom", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("ssao", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("sunshaft", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("instancing", StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (text.IndexOf("gerstner", StringComparison.OrdinalIgnoreCase) >= 0) return true;

			return false;
		}

		#endregion

		#region Environment & Shader Audits

		private static void LogSystemEnvironment()
		{
			WriteLineRaw("================================================================================");
			WriteLineRaw($"Unturned Android Graphics & Shaders Diagnostic Log");
			WriteLineRaw($"Started At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
			WriteLineRaw($"OS: {SystemInfo.operatingSystem}");
			WriteLineRaw($"Device Model: {SystemInfo.deviceModel} ({SystemInfo.deviceName})");
			WriteLineRaw("--------------------------------------------------------------------------------");
			WriteLineRaw($"GPU Device: {SystemInfo.graphicsDeviceName}");
			WriteLineRaw($"GPU Vendor: {SystemInfo.graphicsDeviceVendor} (ID: {SystemInfo.graphicsDeviceVendorID})");
			WriteLineRaw($"GPU API & Version: {SystemInfo.graphicsDeviceType} | {SystemInfo.graphicsDeviceVersion}");
			WriteLineRaw($"GPU Memory (VRAM): {SystemInfo.graphicsMemorySize} MB");
			WriteLineRaw($"Max Texture Size: {SystemInfo.maxTextureSize} px");
			WriteLineRaw($"Shader Level: {SystemInfo.graphicsShaderLevel}");
			WriteLineRaw($"Display Resolution: {Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRate}Hz (DPI: {Screen.dpi:F1})");
			WriteLineRaw("--------------------------------------------------------------------------------");
			WriteLineRaw("GRAPHICS CAPABILITIES AUDIT:");
			WriteLineRaw($" - Supports GPU Instancing: {SystemInfo.supportsInstancing}");
			WriteLineRaw($" - Supports Compute Shaders: {SystemInfo.supportsComputeShaders}");
			WriteLineRaw($" - Supports 2D Array Textures: {SystemInfo.supports2DArrayTextures}");
			WriteLineRaw($" - Supports 3D Textures: {SystemInfo.supports3DTextures}");
			WriteLineRaw($" - Supports Cubemap RenderTarget: {SystemInfo.supportsRenderToCubemap}");
			WriteLineRaw($" - Supports Multiple Render Targets (MRT): {SystemInfo.supportsSeparatedRenderTargets}");
			WriteLineRaw($" - Supports Depth Format (RT): {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)}");
			WriteLineRaw($" - Supports Shadowmap Format: {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap)}");
			WriteLineRaw($" - Supports R8 Format (FastSSAO RT): {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)}");
			WriteLineRaw($" - Supports ARGBHalf (HDR RT): {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)}");
			WriteLineRaw($" - Supports Default HDR Format: {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR)}");
			WriteLineRaw("================================================================================");

			// Diagnostics warnings for known limitations
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
			{
				Warning("Compat", "RenderTextureFormat.R8 is NOT supported on this GPU! FastSSAO will require ARGB32 or fallback RT.", throttle: false);
			}

			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
			{
				Warning("Compat", "RenderTextureFormat.Depth is NOT supported on this GPU! Depth edge blend and screen-space depth reads will fail.", throttle: false);
			}

			if (!SystemInfo.supportsInstancing)
			{
				Warning("Compat", "GPU Instancing is NOT supported! Foliage and grass will render using CPU submission fallback.", throttle: false);
			}
		}

		private static void RunInitialShaderAudit()
		{
			WriteLineRaw("--------------------------------------------------------------------------------");
			WriteLineRaw("CORE SHADERS INTEGRITY AUDIT:");

			string[] coreShaders = new string[]
			{
				// Custom Mobile Post-Processing Shaders
				"Hidden/Custom/FastSSAO",
				"Hidden/Custom/MobileBloom",
				"Hidden/Custom/MobileSunShafts",
				"Hidden/Custom/MobileColorGrading",
				"Hidden/Custom/ScopeVignette",
				"Hidden/Custom/GaussianBlur",

				// Mobile Water Shader
				"Custom/Water_Fallback",

				// Landscape & Terrain Shaders
				"Landscapes/HeightTransition/Forward/FirstPass",
				"Landscapes/HeightTransition/Forward/AddPass",
				"Landscapes/LinearTransition/FirstPass",
				"Landscapes/LinearTransition/AddPass",
				"StandardLandscape",

				// Foliage & Grass Shaders
				"Framework/Grass",
				"Framework/Grass_AssumeUniformScale",
				"Custom/Foliage",
				"Custom/FoliageSnow",
				"Custom/Flag",

				// Standard Core Shaders
				"Standard",
				"Standard (Specular setup)",
				"StandardSnow"
			};

			foreach (string name in coreShaders)
			{
				Shader s = Shader.Find(name);
				LogShaderInit(name, s, "Audit");
			}

			WriteLineRaw("================================================================================");
		}

		#endregion
	}
}
