using System;
using WaveBox.Core.Model;

namespace WaveBox.Client
{
	public delegate void PlayQueueEventHandler(object sender, PlayQueueEventArgs e);

	public class PlayQueueEventArgs : EventArgs
	{
		public IMediaItem MediaItem { get; set; }

		public PlayQueueEventArgs(IMediaItem mediaItem) 
		{
			this.MediaItem = mediaItem;
		}
	}
}

