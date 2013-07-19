using System;
using WaveBox.Model;

namespace WaveBox.Client.AudioEngine
{
	public interface IBassGaplessPlayer
	{
		event PlayerEventHandler PositionStarted;
		event PlayerEventHandler SeekToPositionStarted;
		event PlayerEventHandler SeekToPositionSuccess;
		event PlayerEventHandler Stopped;
		event PlayerEventHandler FirstStreamStarted;
		event PlayerEventHandler SongEndedCalled;
		event PlayerEventHandler SongEndedPlaylistIncremented;
		event PlayerEventHandler SongEndedFinishedIsPlaying;
		event PlayerEventHandler SongFailedToPlay;

		IBassGaplessPlayerDataSource DataSource { get; set; }
		bool IsPlaying { get; }
		bool IsInitialBuffering { get; set; }
		void StartWithOffsetInBytesOrSeconds(long? byteOffset, double? seconds);
		bool IsStarted { get; }
		long CurrentByteOffset { get; }
		double Progress { get; }
		BassStream CurrentStream { get; }
		int BitRate { get; }
		void Start();
		void Stop();
		void Pause();
		void PlayPause();
		void SeekToPositionInBytes(long bytes);
		void SeekToPositionInSeconds(double seconds);
		void RaiseSongFailedToPlayEvent(Song song);
		IAudioEngine AudioEngine { get; set; }
	}
}

