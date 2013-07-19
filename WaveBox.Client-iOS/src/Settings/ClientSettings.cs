using System;

namespace WaveBox.Client
{
	public class ClientSettings : IClientSettings
	{
		public bool IsOfflineMode { get { return true; } } 
		public bool IsRecover { get { return false; } } 
		public long? RecoverByteOffset { get; set; }
		public double? RecoverSeekTime { get; set; }

		public ClientSettings()
		{
		}
	}
}

