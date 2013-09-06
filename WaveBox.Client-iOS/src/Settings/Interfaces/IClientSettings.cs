using System;
using System.Collections.Generic;

namespace WaveBox.Client
{
	public interface IClientSettings
	{
		bool IsOfflineMode { get; }
		bool IsRecover { get; }
		long? RecoverByteOffset { get; set; }
		double? RecoverSeekTime { get; set; }

		string ServerUrl { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		string SessionId { get; set; }

		uint StreamQueueLength { get; set; }

		IDictionary<string, object> StyleDictionary { get; set; }

		int LastQueryId { get; set; }

		string DownloadsPath { get; }

		void SaveSettings();
		void LoadSettings();
	}
}

