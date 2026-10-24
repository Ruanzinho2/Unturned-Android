////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SDG.Unturned
{
	public class MenuFallbackPreprocess : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			Execute();
			ConfigureAndroidGraphics();
		}

		[MenuItem("Unturned/Configure Android Graphics Settings")]
		public static void ConfigureAndroidGraphics()
		{
			try
			{
				BuildTargetGroup targetGroup = BuildTargetGroup.Android;
				GraphicsTier[] tiers = new GraphicsTier[] { GraphicsTier.Tier1, GraphicsTier.Tier2, GraphicsTier.Tier3 };

				foreach (GraphicsTier tier in tiers)
				{
					TierSettings tierSettings = EditorGraphicsSettings.GetTierSettings(targetGroup, tier);
					tierSettings.cascadedShadowMaps = false;
					tierSettings.renderingPath = RenderingPath.DeferredShading;
					EditorGraphicsSettings.SetTierSettings(targetGroup, tier, tierSettings);
				}

				Debug.Log("[MenuFallbackPreprocess] Successfully configured Android TierSettings: cascadedShadowMaps=false (disables Internal-ScreenSpaceShadows in Forward) and renderingPath=DeferredShading.");
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[MenuFallbackPreprocess] Exception configuring Android TierSettings: {ex}");
			}
		}

		[MenuItem("Unturned/Fix Menu Fallback Meshes")]
		public static void Execute()
		{
			string scenePath = "Assets/Game/Sources/Scenes/Menu_Fallback.unity";
			if (!File.Exists(scenePath))
			{
				Debug.LogWarning($"[MenuFallbackPreprocess] Scene file not found: {scenePath}");
				return;
			}

			Debug.Log("[MenuFallbackPreprocess] Pre-importing all MainMenu meshes...");
			string meshDir = "Assets/Game/Sources/MainMenu/Meshes";
			if (Directory.Exists(meshDir))
			{
				string[] objFiles = Directory.GetFiles(meshDir, "*.obj");
				foreach (string objFile in objFiles)
				{
					AssetDatabase.ImportAsset(objFile, ImportAssetOptions.ForceSynchronousImport);
				}
			}

			// Parse scene YAML to map GameObject ID -> Mesh GUID
			Dictionary<string, string> goNameToMeshGuid = new Dictionary<string, string>();
			Dictionary<int, string> mfIdToMeshGuid = new Dictionary<int, string>();
			try
			{
				string sceneText = File.ReadAllText(scenePath);
				// Match MeshFilter blocks: --- !u!33 &(\d+)\nMeshFilter:[\s\S]*?m_GameObject: \{fileID: (\d+)\}[\s\S]*?m_Mesh: \{fileID: \d+, guid: ([0-9a-fA-F]+)
				MatchCollection matches = Regex.Matches(sceneText, @"--- !u!33 &(\d+)\nMeshFilter:[\s\S]*?m_GameObject: \{fileID: (\d+)\}[\s\S]*?m_Mesh: \{fileID: \d+, guid: ([0-9a-fA-F]+)", RegexOptions.Multiline);
				foreach (Match match in matches)
				{
					if (int.TryParse(match.Groups[1].Value, out int mfId))
					{
						string guid = match.Groups[3].Value;
						mfIdToMeshGuid[mfId] = guid;
					}
				}
				Debug.Log($"[MenuFallbackPreprocess] Parsed {mfIdToMeshGuid.Count} MeshFilter GUID bindings from scene YAML.");
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[MenuFallbackPreprocess] Could not parse scene text: {ex.Message}");
			}

			Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
			if (!scene.IsValid())
			{
				Debug.LogError($"[MenuFallbackPreprocess] Could not open scene {scenePath}");
				return;
			}

			int totalMeshFilters = 0;
			int alreadyValid = 0;
			int fixedCount = 0;
			int failedCount = 0;

			GameObject[] roots = scene.GetRootGameObjects();
			foreach (GameObject root in roots)
			{
				MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
				foreach (MeshFilter mf in meshFilters)
				{
					totalMeshFilters++;
					if (mf.sharedMesh != null)
					{
						alreadyValid++;
						continue;
					}

					// Mesh is null, attempt to resolve
					Mesh resolvedMesh = null;
					string cleanName = mf.gameObject.name;
					int parenIdx = cleanName.IndexOf(" (");
					if (parenIdx > 0)
					{
						cleanName = cleanName.Substring(0, parenIdx);
					}

					// 1. Try resolving via exact GUID recorded in scene YAML
					try
					{
						ulong targetFileId = GlobalObjectId.GetGlobalObjectIdSlow(mf).targetObjectId;
						int localId = (int)(targetFileId & 0x7FFFFFFF);
						if (mfIdToMeshGuid.TryGetValue(localId, out string guid) || mfIdToMeshGuid.TryGetValue((int)targetFileId, out guid))
						{
							string pathFromGuid = AssetDatabase.GUIDToAssetPath(guid);
							if (!string.IsNullOrEmpty(pathFromGuid))
							{
								resolvedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(pathFromGuid);
								if (resolvedMesh == null)
								{
									UnityEngine.Object[] sub = AssetDatabase.LoadAllAssetsAtPath(pathFromGuid);
									foreach (var obj in sub)
									{
										if (obj is Mesh m)
										{
											resolvedMesh = m;
											break;
										}
									}
								}
							}
						}
					}
					catch (Exception)
					{
					}

					// 2. Try finding by matching GameObject name
					if (resolvedMesh == null)
					{
						string directPath = $"Assets/Game/Sources/MainMenu/Meshes/{cleanName}.obj";
						if (File.Exists(directPath))
						{
							resolvedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(directPath);
							if (resolvedMesh == null)
							{
								UnityEngine.Object[] sub = AssetDatabase.LoadAllAssetsAtPath(directPath);
								foreach (var obj in sub)
								{
									if (obj is Mesh m)
									{
										resolvedMesh = m;
										break;
									}
								}
							}
						}
					}

					// 2. If still null, search all MainMenu meshes by name
					if (resolvedMesh == null)
					{
						string[] matchingFiles = Directory.GetFiles("Assets/Game/Sources/MainMenu/Meshes", $"{cleanName}_*.obj");
						if (matchingFiles.Length > 0)
						{
							string candPath = matchingFiles[0];
							resolvedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(candPath);
							if (resolvedMesh == null)
							{
								UnityEngine.Object[] sub = AssetDatabase.LoadAllAssetsAtPath(candPath);
								foreach (var obj in sub)
								{
									if (obj is Mesh m)
									{
										resolvedMesh = m;
										break;
									}
								}
							}
						}
					}

					if (resolvedMesh != null)
					{
						mf.sharedMesh = resolvedMesh;
						EditorUtility.SetDirty(mf);
						fixedCount++;
					}
					else
					{
						failedCount++;
						Debug.LogWarning($"[MenuFallbackPreprocess] Failed to resolve mesh for GameObject '{mf.gameObject.name}'");
					}
				}
			}

			Debug.Log($"[MenuFallbackPreprocess] Results - Total: {totalMeshFilters}, AlreadyValid: {alreadyValid}, Fixed: {fixedCount}, Failed: {failedCount}");

			if (fixedCount > 0 || failedCount == 0)
			{
				EditorSceneManager.MarkSceneDirty(scene);
				EditorSceneManager.SaveScene(scene);
				Debug.Log("[MenuFallbackPreprocess] Saved scene Menu_Fallback with updated mesh bindings.");
			}

			EditorSceneManager.CloseScene(scene, true);
		}
	}
}
