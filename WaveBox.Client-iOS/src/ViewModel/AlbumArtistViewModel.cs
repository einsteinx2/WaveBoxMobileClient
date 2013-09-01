using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public class AlbumArtistViewModel : IAlbumArtistViewModel
	{
		public AlbumArtist AlbumArtist { get; set; }

		public IList<Album> Albums { get; set; }

		public AlbumArtistViewModel()
		{

		}

		public void ReloadData()
		{
			if (AlbumArtist != null)
			{
				Albums = AlbumArtist.ListOfAlbums();
			}
		}
	}
}

