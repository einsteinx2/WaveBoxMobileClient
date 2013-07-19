using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WaveBox.Core.Injection;
using WaveBox.Static;
using WaveBox;

namespace WaveiOS
{
	public class Database : IDatabase
	{
		private static readonly string DATABASE_FILE_NAME = "wavebox.db";
		public string DatabaseTemplatePath() { throw new NotImplementedException(); }
		public string DatabasePath() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DATABASE_FILE_NAME); }

		public string QuerylogTemplatePath() { throw new NotImplementedException(); }
		public string QuerylogPath() { throw new NotImplementedException(); }

		private static readonly object dbBackupLock = new object();
		public object DbBackupLock { get { return dbBackupLock; } }

		// Sqlite connection pool
		private static readonly int MAX_CONNECTIONS = 100;
		private SQLiteConnectionPool mainPool;

		public Database()
		{
			mainPool = new SQLiteConnectionPool(MAX_CONNECTIONS, DatabasePath());
		}

		public void DatabaseSetup()
		{
		}

		public ISQLiteConnection GetSqliteConnection()
		{
			return mainPool.GetSqliteConnection();
		}

		public void CloseSqliteConnection(ISQLiteConnection conn)
		{
			mainPool.CloseSqliteConnection(conn);
		}

		public ISQLiteConnection GetQueryLogSqliteConnection()
		{
			throw new NotImplementedException();
		}

		public void CloseQueryLogSqliteConnection(ISQLiteConnection conn)
		{
			throw new NotImplementedException();
		}

		public long LastQueryLogId()
		{
			throw new NotImplementedException();
		}
	}
}
