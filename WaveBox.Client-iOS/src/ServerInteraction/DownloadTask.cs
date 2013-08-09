using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using WaveBox.Core.Model;
using System.Net;
using System.ComponentModel;
using WaveBox.Client.AudioEngine;

namespace WaveBox.Client.ServerInteraction
{
	public enum DownloadTaskType
	{
		Stream,
		Download
	}

	public enum DownloadTaskState
	{
		New,
		Active,
		Canceled,
		Failed,
		Completed
	}

	public class DownloadTask
	{
		public event DownloadEventHandler ProgressChanged;

		public DownloadTaskType Type { get; set; }

		public DownloadTaskState State { get; set; }

		public IMediaItem MediaItem { get; set; }

		public long BytesReceived { get; set; }

		public int ProgressPercentage { get; set; }

		public Exception Error { get; set; }

		private WebClient webClient;

		private readonly IClientSettings clientSettings;

		public DownloadTask(IClientSettings clientSettings)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.clientSettings = clientSettings;
		}

		public async Task Run()
		{
			if (MediaItem == null)
				throw new ServerInteractionException("Cannot download media item, as the object is null");
			if (MediaItem.ItemId == null || MediaItem.ItemId == 0)
				throw new ServerInteractionException("Cannot download media item, as the ItemId is null or 0, item id:" + MediaItem.ItemId);

			// Initiate the Api call
			webClient = new WebClient();
			webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
			webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

			State = DownloadTaskState.Active;
			string urlString = clientSettings.ServerUrl + "/api/stream?s=" + clientSettings.SessionId + "&id=" + MediaItem.ItemId;
			string downloadPath = MediaItem.DownloadPath();

			if (File.Exists(downloadPath))
			{
				// For now, assume existing file is completed
				State = DownloadTaskState.Completed;
			}
			else
			{
				await webClient.DownloadFileTaskAsync(urlString, downloadPath);
				Console.WriteLine("DownloadFileTaskAsync for " + MediaItem + " returned control");
			}
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			BytesReceived = e.BytesReceived;
			ProgressPercentage = e.ProgressPercentage;

			if (ProgressChanged != null)
				ProgressChanged(this, new DownloadEventArgs(Error, MediaItem, BytesReceived, ProgressPercentage));

			//Console.WriteLine("Download for " + MediaItem.ItemId + " progress changed to bytes received: " + e.BytesReceived + " progress percent: " + e.ProgressPercentage);
		}

		private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) 
		{
			Error = e.Error;
			State = Error == null ? DownloadTaskState.Completed : DownloadTaskState.Failed;

			Console.WriteLine("Download for " + MediaItem + " completed, error: ", e.Error);
		}

		public void Cancel()
		{
			State = DownloadTaskState.Canceled;
			webClient.CancelAsync();
			Console.WriteLine("Download for " + MediaItem + " canceled");
		}

		/*
		 * Equality check
		 */

		public override bool Equals(object obj)
		{
			// If parameter is null return false.
			if (obj == null)
				return false;

			// If MediaItem or MediaItem.ItemId are null, return false
			if (MediaItem == null || MediaItem.ItemId == null)
				return false;

			// If parameter cannot be cast to Point return false.
			DownloadTask otherTask = obj as DownloadTask;
			if ((object)otherTask == null)
				return false;

			// Return true if the MediaItems match
			return MediaItem.ItemId == otherTask.MediaItem.ItemId;
		}

		public bool Equals(DownloadTask otherTask)
		{
			// If parameter is null return false:
			if ((object)otherTask == null)
				return false;

			// Return true if the MediaItems match
			return MediaItem.ItemId == otherTask.MediaItem.ItemId;
		}

		public override int GetHashCode()
		{
			return MediaItem == null || MediaItem.ItemId == null ? 0 : (int)MediaItem.ItemId;
		}
	}
}

