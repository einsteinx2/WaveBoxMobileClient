using System;
using WaveBox.Core.Model;

namespace WaveBox.Client.AudioEngine
{
	public enum PlayQueueRepeatMode
	{
		Normal,
		RepeatOne,
		RepeatAll
	}

	public interface IPlayQueue
	{
		Song CurrentSong { get; }

		Song NextSong { get; }

		int CurrentIndex { get; set; }

		int NextIndex { get; }

		int PrevIndex { get; }

		bool IsAnySongCached { get; }

		PlayQueueRepeatMode RepeatMode { get; set; }

		void IncrementIndex();
	}
}

