using System;
using WaveBox.Client.AudioEngine;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public class PlayQueueViewModel : IPlayQueueViewModel
	{
		public event ViewModelEventHandler DataChanged;

		public IList<IMediaItem> MediaItems { get { return playQueue.CurrentPlayQueueList; } }

		public uint CurrentIndex { get { return playQueue.CurrentIndex; } }

		public IMediaItem CurrentItem { get { return playQueue.CurrentItem; } }

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;
		private readonly IBassGaplessPlayer player;

		public PlayQueueViewModel(IPlayQueue playQueue, IAudioEngine audioEngine, IBassGaplessPlayer player)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (audioEngine == null)
				throw new ArgumentNullException("audioEngine");
			if (player == null)
				throw new ArgumentNullException("player");

			this.playQueue = playQueue;
			this.audioEngine = audioEngine;
			this.player = player;

			playQueue.IndexChanged += delegate(object sender, PlayQueueEventArgs e) {
				InformDataChanged();
			};
			playQueue.ShuffleToggled += delegate(object sender, PlayQueueEventArgs e) {
				InformDataChanged();
			};
			playQueue.OrderChanged += delegate(object sender, PlayQueueEventArgs e) {
				InformDataChanged();
			};
		}

		private void InformDataChanged()
		{
			if (DataChanged != null) DataChanged(this, new ViewModelEventArgs());
		}

		public void PlayItemAtIndex(int index)
		{
			audioEngine.PlaySongAtPosition((uint)index);
		}

		public void PlayPauseToggle()
		{
			player.PlayPause();
		}
	}
}

