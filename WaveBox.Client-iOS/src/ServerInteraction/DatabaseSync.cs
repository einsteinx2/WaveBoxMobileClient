using System;
using WaveBox.Core;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace WaveBox.Client.ServerInteraction
{
	public class DatabaseSyncLoader : IDatabaseSyncLoader
	{

		public event DatabaseSyncEventHandler DatabaseDownloaded;

		public bool isDatabaseExist { get { return File.Exists(clientDatabase.DatabasePath); } }

		private readonly IClientDatabase clientDatabase;
		private readonly IClientSettings clientSettings;

		public DatabaseSyncLoader(IClientDatabase clientDatabase, IClientSettings clientSettings)
		{
			if (clientDatabase == null)
				throw new ArgumentNullException("clientDatabase");
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.clientDatabase = clientDatabase;
			this.clientSettings = clientSettings;
		}

		public async Task DownloadDatabase()
		{
			await DownloadDatabase(clientSettings.ServerUrl, clientSettings.SessionId, clientDatabase.DatabaseDownloadPath);
		}

		public async Task DownloadDatabase(string serverUrl, string sessionId, string downloadPath)
		{
			if (serverUrl == null)
			{
				throw new MissingServerUrlException();
			}

			if (sessionId == null)
			{
				throw new MissingSessionIdException();
			}

			// Initiate the Api call
			WebClient client = new WebClient();
			client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e) {
				Console.WriteLine("Download progress changed to bytes received: " + e.BytesReceived + " progress percent: " + e.ProgressPercentage);
			};
			client.DownloadFileCompleted += delegate(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
				Console.WriteLine("Download file completed, error: ", e.Error);
			};
			await client.DownloadFileTaskAsync(serverUrl + "/api/database?s=" + sessionId, downloadPath);

			// Retreive the last query id
			int? lastQueryId = null;
			string idString = client.ResponseHeaders.Get("WaveBox-LastQueryId");
			int id;
			if (int.TryParse(idString, out id))
			{
				lastQueryId = id;
			}

			// Inform any delegates
			if (DatabaseDownloaded != null)
			{
				DatabaseDownloaded(this, new DatabaseSyncEventArgs(lastQueryId, null));
			}
		}
	}
}

