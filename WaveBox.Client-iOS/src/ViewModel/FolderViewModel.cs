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
		public Folder Folder { get; set; }

		public IList<Folder> SubFolders { get; set; }

		public IList<Song> Songs { get; set; }

		public IList<Video> Videos { get; set; }

		public string CoverUrl { get { return clientSettings.ServerUrl + "/api/art?size=640&s=" + clientSettings.SessionId + "&id=" + Folder.ArtId; } }

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;
		private readonly IClientSettings clientSettings;

		public FolderViewModel(IPlayQueue playQueue, IAudioEngine audioEngine, IClientSettings clientSettings)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (audioEngine == null)
				throw new ArgumentNullException("audioEngine");
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.playQueue = playQueue;
			this.audioEngine = audioEngine;
			this.clientSettings = clientSettings;
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

