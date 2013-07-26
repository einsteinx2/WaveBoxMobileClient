using System;
using WaveBox.Client;
using System.IO;

namespace Wave.iOS
{
	public class iOSPlatformSettings : IClientPlatformSettings
	{
		public string DocumentsPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); } }
		public string CachesPath { get { return Path.Combine(DocumentsPath, "..", "Library", "Caches"); } }

		public string PathSeparator { get { return "/"; } }

		public iOSPlatformSettings()
		{
		}
	}
}

