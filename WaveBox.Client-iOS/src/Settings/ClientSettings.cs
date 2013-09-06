using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace WaveBox.Client
{
	public class ClientSettings : IClientSettings
	{
		public bool IsOfflineMode { get { return false; } } 
		public bool IsRecover { get { return false; } } 
		public long? RecoverByteOffset { get; set; }
		public double? RecoverSeekTime { get; set; }

		public string ServerUrl { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SessionId { get; set; }

		public uint StreamQueueLength { get; set; }

		public IDictionary<string, object> StyleDictionary { get; set; }

		public int LastQueryId { get; set; }

		public string DownloadsPath { get { return Path.Combine(clientPlatformSettings.DocumentsPath, "downloads"); } }

		private readonly IClientPlatformSettings clientPlatformSettings;

		public ClientSettings(IClientPlatformSettings clientPlatformSettings)
		{
			if (clientPlatformSettings == null)
				throw new ArgumentNullException("clientPlatformSettings");

			this.clientPlatformSettings = clientPlatformSettings;

			// For now clear folder every launch
			if (Directory.Exists(DownloadsPath))
				Directory.Delete(DownloadsPath, true);

			// Make sure the directory is created
			if (!Directory.Exists(DownloadsPath))
			{
				Directory.CreateDirectory(DownloadsPath);
			}

			// Create the default style
			PopulateStyleDictionary();
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

		private void PopulateStyleDictionary()
		{
			StyleDictionary = new Dictionary<string, object>();

			// Menu
			StyleDictionary["menu.backgroundImage"] = "menu-background.png";

			StyleDictionary["menu.staticHeader.height"] = "51";
			StyleDictionary["menu.staticHeader.backgroundColor"] = "FFFFFF";
			StyleDictionary["menu.staticHeader.backgroundColorAlpha"] = "0.30";
			StyleDictionary["menu.staticHeader.bottomBorderColor"] = "FFFFFF";
			StyleDictionary["menu.staticHeader.bottomBorderColorAlpha"] = "0.65";
			StyleDictionary["menu.staticHeader.fontColor"] = "FFFFFF";
			StyleDictionary["menu.staticHeader.fontName"] = "HelveticaNeue";
			StyleDictionary["menu.staticHeader.fontSize"] = "8.4";

			StyleDictionary["menu.rowHeader.height"] = "20";
			StyleDictionary["menu.rowHeader.fontColor"] = "FFFFFF";
			StyleDictionary["menu.rowHeader.fontColorAlpha"] = "1.0";
			StyleDictionary["menu.rowHeader.fontName"] = "HelveticaNeue";
			StyleDictionary["menu.rowHeader.fontSize"] = "22.72";

			StyleDictionary["menu.row.height"] = "50";
			StyleDictionary["menu.row.backgroundColor"] = "FFFFFF";
			StyleDictionary["menu.row.backgroundColorAlpha"] = "0.0";
			StyleDictionary["menu.row.fontColor"] = "E8E8E8";
			StyleDictionary["menu.row.fontColorAlpha"] = "1.0";
			StyleDictionary["menu.row.fontName"] = "HelveticaNeue";
			StyleDictionary["menu.row.fontSize"] = "14.2";

			StyleDictionary["menu.rowSelected.height"] = "50";
			StyleDictionary["menu.rowSelected.backgroundColor"] = "FFFFFF";
			StyleDictionary["menu.rowSelected.backgroundColorAlpha"] = "0.25";
			StyleDictionary["menu.rowSelected.fontColor"] = "E8E8E8";
			StyleDictionary["menu.rowSelected.fontColorAlpha"] = "1.0";
			StyleDictionary["menu.rowSelected.fontName"] = "HelveticaNeue-Bold";
			StyleDictionary["menu.rowSelected.fontSize"] = "14.2";

			StyleDictionary["menu.newPlaylistRow.fontColorAlpha"] = "1.0";
		}
	}
}

