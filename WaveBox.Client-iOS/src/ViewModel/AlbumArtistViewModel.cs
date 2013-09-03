using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Client.AudioEngine;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class AlbumArtistViewModel : IAlbumArtistViewModel
	{
		private AlbumArtist albumArtist;
		public AlbumArtist AlbumArtist { get { return albumArtist; } set { albumArtist = value; ReloadData(); } }

		public IList<Album> Albums { get; set; }

		public IList<Song> Singles { get; set; }

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;

		public AlbumArtistViewModel(IPlayQueue playQueue, IAudioEngine audioEngine)
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
			if (AlbumArtist != null)
			{
				Albums = AlbumArtist.ListOfAlbums();
				Singles = AlbumArtist.ListOfSingles();
			}
		}

		public void PlaySongAtIndex(int index)
		{
			playQueue.ResetBothPlayQueues();
			playQueue.AddItems(Singles.ToList<IMediaItem>());

			audioEngine.PlaySongAtPosition((uint)index);
		}
	}
}

