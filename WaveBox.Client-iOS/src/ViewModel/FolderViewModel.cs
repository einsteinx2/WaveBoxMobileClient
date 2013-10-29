using System;
using WaveBox.Core.Model;
using System.Collections.Generic;
using WaveBox.Core.Model.Repository;
using WaveBox.Client.AudioEngine;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class FolderViewModel : IFolderViewModel
	{
		private Folder folder;
		public Folder Folder { get { return folder; } set { folder = value; ReloadData(); } }

		public IList<Folder> SubFolders { get; set; }
		public IList<Folder> FilteredSubFolders { get; set; }

		public IList<Song> Songs { get; set; }
		public IList<Song> FilteredSongs { get; set; }

		public IList<Video> Videos { get; set; }
		public IList<Video> FilteredVideos { get; set; }

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;

		public FolderViewModel(IPlayQueue playQueue, IAudioEngine audioEngine)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (audioEngine == null)
				throw new ArgumentNullException("audioEngine");

			this.playQueue = playQueue;
			this.audioEngine = audioEngine;
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredSubFolders = SubFolders.Where(x => x.FolderName.ToLower().Contains(searchTerm.ToLower())).ToList();
			FilteredSongs = Songs.Where(x => x.SongName.ToLower().Contains(searchTerm.ToLower())).ToList();
			FilteredVideos = Videos.Where(x => x.FileName.ToLower().Contains(searchTerm.ToLower())).ToList();
		}

		public void ReloadData()
		{
			if (Folder != null)
			{
				SubFolders = Folder.ListOfSubFolders();
				FilteredSubFolders = new List<Folder>(SubFolders);

				Songs = Folder.ListOfSongs();
				FilteredSongs = new List<Song>(Songs);

				Videos = Folder.ListOfVideos();
				FilteredVideos = new List<Video>(Videos);
			}
		}

		public void PlaySongAtIndex(int index)
		{
			playQueue.ResetBothPlayQueues();
			playQueue.AddItems(FilteredSongs.ToList<IMediaItem>());

			audioEngine.PlaySongAtPosition((uint)index);
		}
	}
}

