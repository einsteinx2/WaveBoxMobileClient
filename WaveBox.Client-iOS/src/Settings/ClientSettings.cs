using System;
using Newtonsoft.Json;
using System.IO;

namespace WaveBox.Client
{
	public class ClientSettings : IClientSettings
	{
		public bool IsOfflineMode { get { return true; } } 
		public bool IsRecover { get { return false; } } 
		public long? RecoverByteOffset { get; set; }
		public double? RecoverSeekTime { get; set; }

		public string ServerUrl { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SessionId { get; set; }

		public int LastQueryId { get; set; }

		public string DownloadsPath { get { return Path.Combine(clientPlatformSettings.DocumentsPath, "downloads"); } }

		private readonly IClientPlatformSettings clientPlatformSettings;

		public ClientSettings(IClientPlatformSettings clientPlatformSettings)
		{
			if (clientPlatformSettings == null)
				throw new ArgumentNullException("clientPlatformSettings");

			this.clientPlatformSettings = clientPlatformSettings;
		}

		public void SaveSettings()
		{
			// For now just dump everything into a json file, later we'll better secure the login info
			string json = JsonConvert.SerializeObject(this);
			File.WriteAllText(Path.Combine(clientPlatformSettings.DocumentsPath, "settings.json"), json);
		}

		public void LoadSettings()
		{
			string path = Path.Combine(clientPlatformSettings.DocumentsPath, "settings.json");
			if (File.Exists(path))
			{
				try
				{
					string json = File.ReadAllText(path);
					JsonConvert.PopulateObject(json, this, new JsonSerializerSettings());
				}
				catch {}
			}
		}
	}
}

