using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class PlaylistListViewModel : IPlaylistListViewModel
	{
		public IList<Playlist> Playlists { get; set; }

		public IList<Playlist> FilteredPlaylists { get; set; }

		private readonly IPlaylistRepository playlistRepository;

		public PlaylistListViewModel(IPlaylistRepository playlistRepository)
		{
			if (playlistRepository == null)
				throw new ArgumentNullException("playlistRepository");

			this.playlistRepository = playlistRepository;

			Playlists = new List<Playlist>();
			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredPlaylists = Playlists.Where(x => x.PlaylistName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			Playlists = playlistRepository.AllPlaylists();
			FilteredPlaylists = new List<Playlist>(Playlists);
		}
	}
}

