using System;
using WaveBox.Core.Model;

namespace WaveBox.Client.AudioEngine
{
	public interface IBassGaplessPlayerDataSource
	{
		int BassPlaylistCount { get; }
		int BassPlaylistCurrentIndex { get; set; }
		Song BassPlaylistCurrentSong { get; }
		Song BassPlaylistNextSong { get; }
		bool BassIsOfflineMode { get; }
	}
}

