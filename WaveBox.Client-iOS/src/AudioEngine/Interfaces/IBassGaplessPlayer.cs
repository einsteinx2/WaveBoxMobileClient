using System;
using WaveBox.Core.Model;

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

		// Callbacks
		void StreamCrossfadeCallback(int handle, int channel, int data, IntPtr user);
		void StreamEndCallback(int handle, int channel, int data, IntPtr user);
		void FileCloseProc(IntPtr user);
		long FileLenProc(IntPtr user);
		int FileReadProc(IntPtr buffer, int length, IntPtr user);
		bool FileSeekProc(long offset, IntPtr user);
		int StreamProc(int handle, IntPtr buffer, int length, IntPtr user);

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

