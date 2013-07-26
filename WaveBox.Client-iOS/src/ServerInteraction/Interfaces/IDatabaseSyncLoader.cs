using System;
using System.Threading.Tasks;

namespace WaveBox.Client.ServerInteraction
{
	public interface IDatabaseSyncLoader
	{
		event DatabaseSyncEventHandler DatabaseDownloaded;

		Task DownloadDatabase();
	}
}

