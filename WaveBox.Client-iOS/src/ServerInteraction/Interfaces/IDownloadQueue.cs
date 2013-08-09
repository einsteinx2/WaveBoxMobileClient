using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ServerInteraction
{
	public interface IDownloadQueue
	{
		event DownloadEventHandler DownloadStarted;
		event DownloadEventHandler DownloadProgressChanged;
		event DownloadEventHandler DownloadFinished;

		Task StartQueue(DownloadTaskType type);
		void CancelQueue(DownloadTaskType type);
		Task QueueTask(DownloadTask task, bool startQueue = true);
		Task FillQueueWithMediaItems(IList<IMediaItem> items, bool startQueue = true);
		bool IsMediaItemDownloading(IMediaItem item);
	}
}

