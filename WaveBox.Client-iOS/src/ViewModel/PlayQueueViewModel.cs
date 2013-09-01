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

		private readonly IPlayQueue playQueue;
		private readonly IAudioEngine audioEngine;

		public PlayQueueViewModel(IPlayQueue playQueue, IAudioEngine audioEngine)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (audioEngine == null)
				throw new ArgumentNullException("audioEngine");

			this.playQueue = playQueue;
			this.audioEngine = audioEngine;

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
	}
}

