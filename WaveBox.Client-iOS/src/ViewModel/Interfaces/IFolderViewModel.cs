using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public interface IFolderViewModel : IListViewModel
	{
		Folder Folder { get; set; }

		IList<Folder> SubFolders { get; set; }

		IList<Song> Songs { get; set; }

		IList<Video> Videos { get; set; }

		void PlaySongAtIndex(int index);
	}
}

