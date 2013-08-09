using System;
using System.Collections.Generic;

namespace WaveBox.Client.ServerInteraction
{
	public delegate void DatabaseSyncEventHandler(object sender, DatabaseSyncEventArgs e);
	
	public class DatabaseSyncEventArgs : EventArgs
	{
		public int? LastQueryId;
		public List<DatabaseQuery> Queries;

		public DatabaseSyncEventArgs(int? lastQueryId, List<DatabaseQuery> queries)
		{
			this.LastQueryId = lastQueryId;
			this.Queries = queries;
		}
	}
}

