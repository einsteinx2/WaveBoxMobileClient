using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WaveBox.Client.ServerInteraction
{
	public class DownloadQueue
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event DownloadEventHandler DownloadStarted;
		public event DownloadEventHandler DownloadFinished;
	
		private List<DownloadTask> taskQueue = new List<DownloadTask>();

		public async Task StartQueue()
		{
			while (taskQueue.Count > 0)
			{
				DownloadTask task;
				lock (taskQueue)
				{
					task = taskQueue[0];
				}

				if (DownloadStarted != null)
					DownloadStarted(this, new DownloadEventArgs(null, task.MediaItem));

				await task.Run();

				if (DownloadFinished != null)
					DownloadFinished(this, new DownloadEventArgs(task.Error, task.MediaItem));
			}
		}

		public void StopQueue()
		{
			// Cancel any downloading tasks
			foreach (DownloadTask task in taskQueue)
			{
				if (task.State == DownloadTaskState.Downloading)
					task.Cancel();
			}
		}

		public void QueueTask(DownloadTask task)
		{
			lock (taskQueue)
			{
				bool taskExists = taskQueue.Contains(task);
				if (task.Type == DownloadTaskType.Download && taskExists)
				{
					DownloadTask existingTask = taskQueue[taskQueue.IndexOf(task)];
					existingTask.Type = DownloadTaskType.Download;
				}
				else if (!taskExists)
				{
					taskQueue.Add(task);
				}
			}
		}
	}
}

