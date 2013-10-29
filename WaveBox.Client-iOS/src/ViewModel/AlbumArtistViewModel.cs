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
		public IList<Album> FilteredAlbums { get; set; }

		public IList<Song> Singles { get; set; }
		public IList<Song> FilteredSingles { get; set; }

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

		public void PerformSearch(string searchTerm)
		{
			FilteredAlbums = Albums.Where(x => x.AlbumName.ToLower().Contains(searchTerm.ToLower())).ToList();
			FilteredSingles = Singles.Where(x => x.SongName.ToLower().Contains(searchTerm.ToLower())).ToList();
		}

		public void ReloadData()
		{
			if (AlbumArtist != null)
			{
				Albums = AlbumArtist.ListOfAlbums();
				FilteredAlbums = new List<Album>(Albums);

				Singles = AlbumArtist.ListOfSingles();
				FilteredSingles = new List<Song>(Singles);
			}
		}

		public void PlaySongAtIndex(int index)
		{
			playQueue.ResetBothPlayQueues();
			playQueue.AddItems(FilteredSingles.ToList<IMediaItem>());

			audioEngine.PlaySongAtPosition((uint)index);
		}
	}
}

