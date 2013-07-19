using System;
using WaveBox.Model;

namespace WaveBox.Client.AudioEngine
{
	public class PlayQueue : IPlayQueue
	{
		public PlayQueue()
		{
		}

		public Song CurrentSong
		{
			get
			{
				return null;
			}
		}

		public Song NextSong
		{
			get
			{
				return null;
			}
		}

		public int CurrentIndex
		{
			get
			{
				return 0;
			}

			set
			{

			}
		}

		public int NextIndex
		{
			get
			{
				return 0;
			}
		}

		public int PrevIndex
		{
			get
			{
				return 0;
			}
		}

		public bool IsAnySongCached
		{ 
			get
			{
				return true;
			}
		}

		public PlayQueueRepeatMode RepeatMode { get; set; }

		public void IncrementIndex()
		{

		}
	}
}

