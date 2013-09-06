using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IArtistListViewModel : IListViewModel
	{
		IList<Artist> Artists { get; set; }
		IList<Artist> FilteredArtists { get; set; }
	}
}

