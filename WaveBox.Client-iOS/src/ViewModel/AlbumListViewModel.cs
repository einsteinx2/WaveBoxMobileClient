using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class AlbumListViewModel : IAlbumListViewModel
	{
		public IList<Album> Albums { get; set; }

		public IList<Album> FilteredAlbums { get; set; }

		private readonly IAlbumRepository albumRepository;

		public AlbumListViewModel(IAlbumRepository albumRepository)
		{
			if (albumRepository == null)
				throw new ArgumentNullException("albumRepository");

			this.albumRepository = albumRepository;

			Albums = new List<Album>();
			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredAlbums = Albums.Where(x => x.AlbumName.ToLower().Contains(searchTerm.ToLower())).ToList();
		}

		public void ReloadData()
		{
			Albums = albumRepository.AllAlbums();
			FilteredAlbums = new List<Album>(Albums);
		}
	}
}

