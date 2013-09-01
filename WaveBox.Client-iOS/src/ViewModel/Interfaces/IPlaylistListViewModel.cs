using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IPlaylistListViewModel : IListViewModel
	{
		IList<Playlist> Playlists { get; set; }
	}
}

