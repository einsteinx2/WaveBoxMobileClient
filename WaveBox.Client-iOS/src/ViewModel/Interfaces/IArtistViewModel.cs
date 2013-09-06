using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public interface IArtistViewModel : IListViewModel
	{
		Artist Artist { get; set; }

		IList<Album> Albums { get; set; }
		IList<Album> FilteredAlbums { get; set; }
	}
}

