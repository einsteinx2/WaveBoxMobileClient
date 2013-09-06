using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class AlbumArtistListViewModel : IAlbumArtistListViewModel
	{
		public IList<AlbumArtist> AlbumArtists { get; set; }

		public IList<AlbumArtist> FilteredAlbumArtists { get; set; }

		private readonly IAlbumArtistRepository albumArtistRepository;

		public AlbumArtistListViewModel(IAlbumArtistRepository albumArtistRepository)
		{
			if (albumArtistRepository == null)
				throw new ArgumentNullException("albumArtistRepository");

			this.albumArtistRepository = albumArtistRepository;

			AlbumArtists = new List<AlbumArtist>();
			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredAlbumArtists = AlbumArtists.Where(x => x.AlbumArtistName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			AlbumArtists = albumArtistRepository.AllAlbumArtists();
			FilteredAlbumArtists = new List<AlbumArtist>(AlbumArtists);
		}
	}
}

