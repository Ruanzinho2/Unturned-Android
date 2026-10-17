////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || UNITY_ANDROID
using SDG.Unturned;
using System.IO;

namespace SDG.Provider.Services.Cloud
{
	/// <summary>
	/// For once the provider interface actually came in useful!
	/// Fakes loading the Steam remote storage files from a separate folder.
	/// </summary>
	public class DebugCloudService : Service, ICloudService
	{
		public bool read(string path, byte[] data)
		{
			string absolutePath = makeAbsolutePath(path);
			if (!File.Exists(absolutePath))
			{
				return false;
			}
			byte[] file_data = ReadWrite.readBytes(absolutePath, false, false);
			if (file_data == null || file_data.Length != data.Length)
			{
				UnturnedLog.error("Cloud read {0} bytes from {1}, but expected {2} bytes", file_data?.Length ?? 0, absolutePath, data.Length);
				return false;
			}
			else
			{
				System.Array.Copy(file_data, data, data.Length);
				UnturnedLog.info("Cloud reading {0} bytes from {1}", data.Length, absolutePath);
				return true;
			}
		}

		public bool write(string path, byte[] data, int size)
		{
			string absolutePath = makeAbsolutePath(path);
			ReadWrite.writeBytes(absolutePath, false, false, data, size);
			UnturnedLog.info("Cloud writing {0} bytes to {1}", size, absolutePath);
			return true;
		}

		public bool getSize(string path, out int size)
		{
			string absolutePath = makeAbsolutePath(path);
			if (File.Exists(absolutePath))
			{
				size = (int) new FileInfo(absolutePath).Length;
				UnturnedLog.info("Cloud size of {0} is {1} bytes", absolutePath, size);
				return true;
			}
			size = 0;
			return false;
		}

		public bool exists(string path, out bool exists)
		{
			string absolutePath = makeAbsolutePath(path);
			exists = ReadWrite.fileExists(absolutePath, false, false);
			UnturnedLog.info("Cloud does {0} exist? {1}", absolutePath, exists);
			return true;
		}

		public bool delete(string path)
		{
			string absolutePath = makeAbsolutePath(path);
			ReadWrite.deleteFile(absolutePath, false, false);
			UnturnedLog.info("Cloud delete {0}", absolutePath);
			return true;
		}

		public override void initialize()
		{
#if UNITY_ANDROID
			basePath = Path.Combine(Path.GetFullPath(ReadWrite.PATH), "Cloud");
#else
			basePath = Path.Combine(Path.GetFullPath(ReadWrite.PATH), "Cloud", "Debug");
#endif
			if (!Directory.Exists(basePath))
			{
				Directory.CreateDirectory(basePath);
			}
			UnturnedLog.info("Cloud base path {0}", basePath);
		}

		private string makeAbsolutePath(string relativePath)
		{
			if (relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
			{
				relativePath = relativePath.Substring(1);
			}
			return Path.Combine(basePath, relativePath);
		}

		private string basePath;
	}
}
#endif // UNITY_EDITOR || UNITY_ANDROID
