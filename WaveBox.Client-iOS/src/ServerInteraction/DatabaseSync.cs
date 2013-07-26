using System;
using WaveBox.Core;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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
			await DownloadDatabase(clientSettings.ServerUrl, clientSettings.SessionId);
		}

		public async Task DownloadDatabase(string serverUrl, string sessionId)
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
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(clientSettings.ServerUrl + "/api/database?s=" + clientSettings.SessionId);

			// Check that response was successful or throw exception
			response.EnsureSuccessStatusCode();

			// Retreive the last query id
			int? lastQueryId = null;
			if (response.Headers.Contains("WaveBox-LastQueryId"))
			{
				foreach (string value in response.Headers.GetValues("WaveBox-LastQueryId"))
				{
					int id;
					if (int.TryParse(value, out id))
					{
						lastQueryId = id;
						break;
					}
				}
			}

			// Save the file
			using (FileStream stream = new FileStream(clientDatabase.DatabaseDownloadPath, FileMode.Create))
			{
				await response.Content.CopyToAsync(stream);
			}

			// Inform any delegates
			if (DatabaseDownloaded != null)
			{
				DatabaseDownloaded(this, new DatabaseSyncEventArgs(lastQueryId, null));
			}
		}
	}
}

