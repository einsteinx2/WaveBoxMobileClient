using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public interface IAlbumArtistViewModel : IListViewModel
	{
		AlbumArtist AlbumArtist { get; set; }

		IList<Album> Albums { get; set; }
	}
}
