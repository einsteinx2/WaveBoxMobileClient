using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IGenreListViewModel : IListViewModel
	{
		IList<Genre> Genres { get; set; }
	}
}

