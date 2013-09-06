using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public interface IFolderViewModel : IListViewModel
	{
		Folder Folder { get; set; }

		IList<Folder> SubFolders { get; set; }
		IList<Folder> FilteredSubFolders { get; set; }

		IList<Song> Songs { get; set; }
		IList<Song> FilteredSongs { get; set; }

		IList<Video> Videos { get; set; }
		IList<Video> FilteredVideos { get; set; }

		void PlaySongAtIndex(int index);
	}
}

