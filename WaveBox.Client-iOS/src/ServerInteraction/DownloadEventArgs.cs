using System;
using WaveBox.Core.Model;

namespace WaveBox.Client.ServerInteraction
{
	public delegate void DownloadEventHandler(object sender, DownloadEventArgs e);

	public class DownloadEventArgs : EventArgs
	{
		public Exception Error;
		public IMediaItem MediaItem;

		public DownloadEventArgs(Exception error, IMediaItem mediaItem)
		{
			this.Error = error;
			this.MediaItem = mediaItem;
		}
	}
}

