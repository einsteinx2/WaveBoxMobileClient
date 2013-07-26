using System;

namespace WaveBox.Client.ServerInteraction
{
	public class DatabaseQuery
	{
		public int QueryId;
		public string QueryString;
		public string ValuesString;

		public DatabaseQuery(int queryId, string queryString, string valuesString)
		{
			this.QueryId = queryId;
			this.QueryString = queryString;
			this.ValuesString = valuesString;
		}
	}
}

