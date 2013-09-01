using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;

namespace WaveBox.Client.ViewModel
{
	public class AlbumArtistListViewModel : IAlbumArtistListViewModel
	{
		public IList<AlbumArtist> AlbumArtists { get; set; }

		private readonly IAlbumArtistRepository albumArtistRepository;

		public AlbumArtistListViewModel(IAlbumArtistRepository albumArtistRepository)
		{
			if (albumArtistRepository == null)
				throw new ArgumentNullException("albumArtistRepository");

			this.albumArtistRepository = albumArtistRepository;

			AlbumArtists = new List<AlbumArtist>();
			ReloadData();
		}

		public void ReloadData()
		{
			AlbumArtists = albumArtistRepository.AllAlbumArtists();
		}
	}
}

