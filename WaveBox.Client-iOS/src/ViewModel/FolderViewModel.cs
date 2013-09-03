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

		public IList<Song> Songs { get; set; }

		public IList<Video> Videos { get; set; }

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

		public void ReloadData()
		{
			if (Folder != null)
			{
				SubFolders = Folder.ListOfSubFolders();
				Songs = Folder.ListOfSongs();
				Videos = Folder.ListOfVideos();
			}
		}

		public void PlaySongAtIndex(int index)
		{
			playQueue.ResetBothPlayQueues();
			playQueue.AddItems(Songs.ToList<IMediaItem>());

			audioEngine.PlaySongAtPosition((uint)index);
		}
	}
}

