using System;
using WaveBox.Core.Model;
using System.Collections.Generic;
using WaveBox.Client.AudioEngine;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class AlbumViewModel : IAlbumViewModel
	{
		private Album album;
		public Album Album { get { return album; } set { album = value; ReloadData(); } }

		public IList<Song> Songs { get; set; }

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;

		public AlbumViewModel(IPlayQueue playQueue, IAudioEngine audioEngine)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (audioEngine == null)
				throw new ArgumentNullException("audioEngine");

			this.playQueue = playQueue;
			this.audioEngine = audioEngine;
		}

		public void ReloadData()
		{
			if (Album != null)
			{
				Songs = Album.ListOfSongs();
			}
		}

		public void PlaySongAtIndex(int index)
		{
			playQueue.ResetBothPlayQueues();
			playQueue.AddItems(Songs.ToList<IMediaItem>());

			audioEngine.PlaySongAtPosition((uint)index);
		}
	}
}
