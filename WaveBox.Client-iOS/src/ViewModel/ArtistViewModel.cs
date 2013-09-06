using System;
using WaveBox.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class ArtistViewModel : IArtistViewModel
	{
		private Artist artist;
		public Artist Artist { get { return artist; } set { artist = value; ReloadData(); } }

		public IList<Album> Albums { get; set; }
		public IList<Album> FilteredAlbums { get; set; }

		public ArtistViewModel()
		{

		}

		public void PerformSearch(string searchTerm)
		{
			FilteredAlbums = Albums.Where(x => x.AlbumName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			if (Artist != null)
			{
				Albums = Artist.ListOfAlbums();
				FilteredAlbums = new List<Album>(Albums);
			}
		}
	}
}
