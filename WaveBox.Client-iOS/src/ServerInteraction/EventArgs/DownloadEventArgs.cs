using System;
using WaveBox.Core.Model;

namespace WaveBox.Client.ServerInteraction
{
	public delegate void DownloadEventHandler(object sender, DownloadEventArgs e);

	public class DownloadEventArgs : EventArgs
	{
		public Exception Error { get; set; }
		public IMediaItem MediaItem { get; set; }
		public long BytesReceived { get; set; }
		public int ProgressPercentage { get; set; }

		public DownloadEventArgs(Exception error, IMediaItem mediaItem, long bytesReceived, int progressPercentage)
		{
			this.Error = error;
			this.MediaItem = mediaItem;
			this.BytesReceived = bytesReceived;
			this.ProgressPercentage = progressPercentage;
		}
	}
}

