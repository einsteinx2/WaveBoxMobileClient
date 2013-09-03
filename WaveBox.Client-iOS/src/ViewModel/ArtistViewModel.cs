using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public class ArtistViewModel : IArtistViewModel
	{
		private Artist artist;
		public Artist Artist { get { return artist; } set { artist = value; ReloadData(); } }

		public IList<Album> Albums { get; set; }

		public ArtistViewModel()
		{

		}

		public void ReloadData()
		{
			if (Artist != null)
			{
				Albums = Artist.ListOfAlbums();
			}
		}
	}
}
