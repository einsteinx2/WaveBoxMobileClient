using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;

namespace WaveBox.Client.ViewModel
{
	public class AlbumListViewModel : IAlbumListViewModel
	{
		public IList<Album> Albums { get; set; }

		private readonly IAlbumRepository albumRepository;

		public AlbumListViewModel(IAlbumRepository albumRepository)
		{
			if (albumRepository == null)
				throw new ArgumentNullException("albumRepository");

			this.albumRepository = albumRepository;

			Albums = new List<Album>();
			ReloadData();
		}

		public void ReloadData()
		{
			Albums = albumRepository.AllAlbums();
		}
	}
}

