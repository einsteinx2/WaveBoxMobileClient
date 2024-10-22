using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WaveBox;
using WaveBox.Client;
using WaveBox.Core;
using WaveBox.Core.Model;
using System.Threading.Tasks;
using WaveBox.Core.Extensions;

namespace Wave.iOS
{
	public class Database : IClientDatabase
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string DATABASE_FILE_NAME = "wavebox.db";
		public string DatabaseTemplatePath { get { throw new NotImplementedException(); } }
		public string DatabasePath { get { return Path.Combine(clientPlatformSettings.DocumentsPath, DATABASE_FILE_NAME); } }

		public string QuerylogTemplatePath { get { throw new NotImplementedException(); } }
		public string QuerylogPath { get { throw new NotImplementedException(); } }

		private static readonly object dbBackupLock = new object();
		public object DbBackupLock { get { return dbBackupLock; } }

		public int Version 
		{ 
			get 
			{
				ISQLiteConnection conn = null;
				try
				{
					conn = GetSqliteConnection();
					return conn.ExecuteScalar<int>("SELECT VersionNumber FROM Version LIMIT 1");
				}
				catch (Exception e)
				{
					logger.Error(e);
				}
				finally
				{
					CloseSqliteConnection(conn);
				}

				return 0;
			} 
		}

		// Sqlite connection pool
		private static readonly int MAX_CONNECTIONS = 100;
		private SQLiteConnectionPool mainPool;

		private readonly IClientPlatformSettings clientPlatformSettings;

		public Database(IClientPlatformSettings clientPlatformSettings)
		{
			if (clientPlatformSettings == null)
				throw new ArgumentNullException("clientPlatformSettings");

			this.clientPlatformSettings = clientPlatformSettings;

			mainPool = new SQLiteConnectionPool(MAX_CONNECTIONS, DatabasePath);
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

		public long LastQueryLogId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IList<QueryLog> QueryLogsSinceId(int queryLogId)
		{
			throw new NotImplementedException();
		}

		/*
		 * IClientDatabase
		 */

		public string DatabaseDownloadPath { get { return DatabasePath + ".temp"; } }

		public void ReplaceDatabaseWithDownloaded()
		{
			try
			{
				mainPool.CloseAllConnections(delegate {
					try
					{
						Console.WriteLine("Moving database, main exists: " + File.Exists(DatabasePath) + " download exists: " + File.Exists(DatabaseDownloadPath));
						File.Delete(DatabasePath);
						File.Move(DatabaseDownloadPath, DatabasePath);
						Console.WriteLine("Moved database, main exists: " + File.Exists(DatabasePath) + " download exists: " + File.Exists(DatabaseDownloadPath));
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception replacing database with downloaded in completion action: " + e);
					}
				});
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception replacing database with downloaded: " + e);
			}
		}
	}
}
