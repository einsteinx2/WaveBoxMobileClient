using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public class ArtistViewModel : IArtistViewModel
	{
		public Artist Artist { get; set; }

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
