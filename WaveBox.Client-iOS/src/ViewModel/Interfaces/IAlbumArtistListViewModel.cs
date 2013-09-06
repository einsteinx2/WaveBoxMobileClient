using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IAlbumArtistListViewModel : IListViewModel
	{
		IList<AlbumArtist> AlbumArtists { get; set; }
		IList<AlbumArtist> FilteredAlbumArtists { get; set; }
	}
}

