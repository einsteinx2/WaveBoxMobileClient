using System;
using WaveBox.Core;
using System.Collections.Generic;
using WaveBox.Core.Model;
using Newtonsoft.Json;

namespace WaveBox.Client
{
	public class ServerSettings : IServerSettings
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string settingsFileName = "wavebox.conf";
		public string SettingsTemplatePath() { return null; }
		public string SettingsPath() { return null; }

		private ServerSettingsData settingsModel = new ServerSettingsData();
		public ServerSettingsData SettingsModel { get { return settingsModel; } }

		public Formatting JsonFormatting { get { return settingsModel.PrettyJson ? Formatting.Indented : Formatting.None; } }

		public short Port { get { return settingsModel.Port; } }

		public short WsPort { get { return settingsModel.WsPort; } }

		public string Theme { get { return settingsModel.Theme; } }

		public IList<Folder> MediaFolders { get; private set; }

		public string PodcastFolder { get { return settingsModel.PodcastFolder; } }

		public int PodcastCheckInterval { get { return settingsModel.PodcastCheckInterval; } }

		public int SessionTimeout { get { return settingsModel.SessionTimeout; } }

		public IList<string> FolderArtNames { get { return settingsModel.FolderArtNames; } }

		public bool CrashReportEnable { get { return settingsModel.CrashReportEnable; } }

		public IList<string> Services { get { return settingsModel.Services; } }

		public ServerSettings()
		{
			Folder folder = new Folder();
			folder.FolderPath = "/mnt/data1/organized_music/MP3";
			folder.FolderName = "MP3";
			MediaFolders = new List<Folder>();
			MediaFolders.Add(folder);
		}

		public void Reload()
		{
			throw new NotImplementedException();
		}

		public bool WriteSettings(string jsonString)
		{
			throw new NotImplementedException();
		}

		public void FlushSettings()
		{
			throw new NotImplementedException();
		}

		public void SettingsSetup()
		{
			throw new NotImplementedException();
		}
	}
}

