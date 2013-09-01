using System;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Client.ViewModel;

namespace WaveBox.Client.ViewModel
{
	public interface IAlbumListViewModel : IListViewModel
	{
		IList<Album> Albums { get; set; }
	}
}

