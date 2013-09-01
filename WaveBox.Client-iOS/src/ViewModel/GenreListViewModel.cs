using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;

namespace WaveBox.Client.ViewModel
{
	public class GenreListViewModel : IGenreListViewModel
	{
		public IList<Genre> Genres { get; set; }

		private readonly IGenreRepository genreRepository;

		public GenreListViewModel(IGenreRepository genreRepository)
		{
			if (genreRepository == null)
				throw new ArgumentNullException("genreRepository");

			this.genreRepository = genreRepository;

			ReloadData();
		}

		public void ReloadData()
		{
			Genres = genreRepository.AllGenres();
		}
	}
}

