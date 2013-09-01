using System;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using WaveBox.Core.Model.Repository;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public class ArtistListViewModel : IArtistListViewModel
	{
		public IList<Artist> Artists { get; set; }

		private readonly IArtistRepository artistRepository;

		public ArtistListViewModel(IArtistRepository artistRepository)
		{
			if (artistRepository == null)
				throw new ArgumentNullException("artistRepository");

			this.artistRepository = artistRepository;

			Artists = new List<Artist>();
			ReloadData();
		}

		public void ReloadData()
		{
			Artists = artistRepository.AllArtists();
		}
	}
}

