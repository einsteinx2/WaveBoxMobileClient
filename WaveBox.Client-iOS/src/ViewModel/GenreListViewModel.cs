using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class GenreListViewModel : IGenreListViewModel
	{
		public IList<Genre> Genres { get; set; }

		public IList<Genre> FilteredGenres { get; set; }

		private readonly IGenreRepository genreRepository;

		public GenreListViewModel(IGenreRepository genreRepository)
		{
			if (genreRepository == null)
				throw new ArgumentNullException("genreRepository");

			this.genreRepository = genreRepository;

			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredGenres = Genres.Where(x => x.GenreName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			Genres = genreRepository.AllGenres();
			FilteredGenres = new List<Genre>(Genres);
		}
	}
}

