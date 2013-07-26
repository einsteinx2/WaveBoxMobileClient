using System;
using WaveBox.Core;
using System.Threading.Tasks;

namespace WaveBox.Client
{
	public interface IClientDatabase : IDatabase
	{
		string DatabaseDownloadPath { get; }

		Task ReplaceDatabaseWithDownloaded();
	}
}

