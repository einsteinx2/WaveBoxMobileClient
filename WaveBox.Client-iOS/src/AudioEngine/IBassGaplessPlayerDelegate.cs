using System;
using WaveBox.Model;

namespace WaveBox.Client
{
	public interface IBassGaplessPlayerDelegate
	{
		void BassSeekToPositionStarted();
		void BassSeekToPositionSuccess();
		void BassStopped();
		void BassFirstStreamStarted();
		void BassSongEndedCalled();
		void BassSongEndedPlaylistIncremented(Song endedSong);
		void BassSongEndedFinishedIsPlaying();
		void BassSongFailedToPlay(Song stalledSong);
	}
}

