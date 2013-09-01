using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;

namespace WaveBox.Client.ViewModel
{
	public class PlaylistListViewModel : IPlaylistListViewModel
	{
		public IList<Playlist> Playlists { get; set; }

		private readonly IPlaylistRepository playlistRepository;

		public PlaylistListViewModel(IPlaylistRepository playlistRepository)
		{
			if (playlistRepository == null)
				throw new ArgumentNullException("playlistRepository");

			this.playlistRepository = playlistRepository;

			Playlists = new List<Playlist>();
			ReloadData();
		}

		public void ReloadData()
		{
			Playlists = playlistRepository.AllPlaylists();
		}
	}
}

