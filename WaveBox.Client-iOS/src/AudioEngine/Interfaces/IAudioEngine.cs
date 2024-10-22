using System;

namespace WaveBox.Client.AudioEngine
{
	public interface IAudioEngine
	{
		void StartWithOffsetInBytesOrSeconds(long? byteOffset, double? seconds);
		void Start();
		void Stop();
		void StartSongAtOffsetInBytesOrSeconds(long? bytes, double? seconds, bool delay);
		void StartSongAtOffsetInBytesOrSecondsInternal(long? bytes, double? seconds);
		void StartSong();
		void PlaySongAtPosition(uint position);
		void PlaySongAtPosition(uint position, bool delay);
		void PrevSong();
		void NextSong();
		void ResumeSong();
	}
}

