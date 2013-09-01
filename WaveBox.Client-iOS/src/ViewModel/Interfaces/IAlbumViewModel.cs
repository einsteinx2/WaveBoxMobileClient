using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public interface IAlbumViewModel : IListViewModel
	{
		Album Album { get; set; }

		IList<Song> Songs { get; set; }

		void PlaySongAtIndex(int index);
	}
}

