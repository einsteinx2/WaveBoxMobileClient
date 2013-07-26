using System;

namespace WaveBox.Client
{
	public interface IClientPlatformSettings
	{
		string DocumentsPath { get; }
		string CachesPath { get; }

		string PathSeparator { get; }
	}
}

