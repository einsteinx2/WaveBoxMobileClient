using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using WaveBox.Core.Model;
using System.Linq;
using WaveBox.Client.AudioEngine;
using WaveBox.Core.Extensions;
using System.Collections;

namespace WaveBox.Client.ServerInteraction
{
	public enum DownloadQueueState
	{
		New, 
		Active,
		Canceled,
		Finished
	}

	// Need to do this because Xamarin.iOS can't use primitive types as keys in dictionaries without a custom 
	// IEqualityComparer passed to the Dictionary contructor
	public class DownloadTaskTypeComparer<DownloadTaskType> : IEqualityComparer<DownloadTaskType>
	{
		public bool Equals(DownloadTaskType x, DownloadTaskType y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(DownloadTaskType type)
		{
			return type.GetHashCode();
		}
	}

	public class DownloadQueue : IDownloadQueue
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event DownloadEventHandler DownloadStarted;
		public event DownloadEventHandler DownloadProgressChanged;
		public event DownloadEventHandler DownloadFinished;
		
		public IDictionary<DownloadTaskType, DownloadQueueState> state = new Dictionary<DownloadTaskType, DownloadQueueState>(new DownloadTaskTypeComparer<DownloadTaskType>());
	
		private IList<DownloadTask> taskQueue = new List<DownloadTask>();
		private IList<IMediaItem> upcomingItems = new List<IMediaItem>();

		private readonly IClientSettings clientSettings;

		public DownloadQueue(IClientSettings clientSettings)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.clientSettings = clientSettings;

			// Setup initial state
			state[DownloadTaskType.Stream] = DownloadQueueState.New;
			state[DownloadTaskType.Download] = DownloadQueueState.New;
		}

		public async Task StartQueue(DownloadTaskType type)
		{
			// If we're already running, bail
			if (state[type] == DownloadQueueState.Active)
				return;

			// Before looping, mark any canceled tasks as new so they will be downloaded
			lock (taskQueue)
			{
				foreach (DownloadTask task in taskQueue)
				{
					if (task.State == DownloadTaskState.Canceled)
						task.State = DownloadTaskState.New;
				}
			}

			while (true)
			{
				// Grab a list of the tasks that match this type or if the type is Stream, are upcoming in the play queue.
				// Since we use the same queue for both streams and downloads, then for instance, the next stream
				// may actually be a download. But we always want to make sure that the next stream is available for playback.
				IList<DownloadTask> tasksForType;
				lock (taskQueue)
				{
					IEnumerable<DownloadTask> result;

					if (type == DownloadTaskType.Stream)
					{
						result = taskQueue.Where(t => t.State == DownloadTaskState.New && (t.Type == type || upcomingItems.Contains(t.MediaItem)));
					}
					else
					{
						result = taskQueue.Where(t => t.State == DownloadTaskState.New && t.Type == type);
					}

					tasksForType = new List<DownloadTask>(result);
				}

				if (tasksForType.Count > 0)
				{
					DownloadTask task = tasksForType[0];
					task.ProgressChanged += new DownloadEventHandler(ProgressChanged);

					// Inform delegates that the download has started
					if (DownloadStarted != null)
						DownloadStarted(this, new DownloadEventArgs(null, task.MediaItem, task.BytesReceived, task.ProgressPercentage));

					// Start the download
					await task.Run();

					// Remove it's media item from the upcoming items
					lock (upcomingItems)
					{
						upcomingItems.Remove(task.MediaItem);
					}

					// Remove the finished task from the queue
					lock (taskQueue)
					{
						taskQueue.Remove(task);
					}

					// Inform delegates that the download has finished
					if (DownloadFinished != null)
						DownloadFinished(this, new DownloadEventArgs(task.Error, task.MediaItem, task.BytesReceived, task.ProgressPercentage));
				}
				else
				{
					// No more tasks
					break;
				}
			}
		}

		private void ProgressChanged(object sender, DownloadEventArgs e)
		{
			if (DownloadProgressChanged != null)
				DownloadProgressChanged(this, e);
		}

		public void CancelQueue(DownloadTaskType type)
		{
			// If we're not already running, bail
			if (state[type] != DownloadQueueState.Active)
				return;

			// Cancel any downloading tasks
			lock (taskQueue)
			{
				foreach (DownloadTask task in taskQueue)
				{
					if (task.State == DownloadTaskState.Active && task.Type == type)
						task.Cancel();
				}
			}
		}

		public async Task QueueTask(DownloadTask task, bool startQueue = true)
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

			if (startQueue)
			{
				await StartQueue(task.Type);
			}
		}

		// This keeps the task queue filled with the currently necessary streams and ensures they
		// are properly ordered and at the start of the queue
		public async Task FillQueueWithMediaItems(IList<IMediaItem> items, bool startQueue = true)
		{
			lock (taskQueue)
			{
				// Save the upcoming items
				lock (upcomingItems)
				{
					upcomingItems.Clear();
					upcomingItems.AddRange(items);
				}

				// Remove anything from the queue that is not in the list of items unless it's a download
				for (int i = taskQueue.Count - 1; i >= 0; i--)
				{
					DownloadTask task = taskQueue[i];
					if (!items.Contains(task.MediaItem) && task.Type != DownloadTaskType.Download)
					{
						taskQueue.Remove(task);
					}
				}

				// Now add these items to the queue if they don't already exist, or move the task position if they do
				IList<DownloadTask> temp = new List<DownloadTask>(taskQueue);
				taskQueue.Clear();
				for (int i = 0; i < items.Count; i++)
				{
					IMediaItem item = items[i];
					DownloadTask task = taskQueue.SingleOrDefault(x => x.MediaItem.Equals(item));
					if (task == null)
					{
						// Create a new task
						task = new DownloadTask(clientSettings) { MediaItem = item };
					}
					else
					{
						// Make sure the task is marked as stream
						task.Type = DownloadTaskType.Stream;

						// Remove the task from the temporary queue
						temp.Remove(task);
					}

					// Queue the new or existing task
					taskQueue.Add(task);
				}

				// Queue the remaining tasks (these would be any other download type tasks)
				taskQueue.AddRange(temp);
			}

			if (startQueue)
			{
				await StartQueue(DownloadTaskType.Stream);
			}
		}

		public bool IsMediaItemDownloading(IMediaItem item)
		{
			lock (taskQueue)
			{
				return taskQueue.Any((i) => (i.MediaItem.Equals(item)));
			}
		}
	}
}

