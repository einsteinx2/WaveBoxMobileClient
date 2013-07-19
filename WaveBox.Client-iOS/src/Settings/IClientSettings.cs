using System;

namespace WaveBox.Client
{
	public interface IClientSettings
	{
		bool IsOfflineMode { get; }
		bool IsRecover { get; }
		long? RecoverByteOffset { get; set; }
		double? RecoverSeekTime { get; set; }
	}
}

