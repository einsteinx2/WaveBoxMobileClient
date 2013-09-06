using System;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using WaveBox.Core.Model.Repository;
using WaveBox.Core.Model;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class ArtistListViewModel : IArtistListViewModel
	{
		public IList<Artist> Artists { get; set; }

		public IList<Artist> FilteredArtists { get; set; }

		private readonly IArtistRepository artistRepository;

		public ArtistListViewModel(IArtistRepository artistRepository)
		{
			if (artistRepository == null)
				throw new ArgumentNullException("artistRepository");

			this.artistRepository = artistRepository;

			Artists = new List<Artist>();
			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredArtists = Artists.Where(x => x.ArtistName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			Artists = artistRepository.AllArtists();
			FilteredArtists = new List<Artist>(Artists);
		}
	}
}

