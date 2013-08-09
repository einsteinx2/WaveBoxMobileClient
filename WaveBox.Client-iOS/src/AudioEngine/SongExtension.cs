using System;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;
using WaveBox.Client.ServerInteraction;
using System.IO;

namespace WaveBox.Client.AudioEngine
{
	public static class SongExtension
	{
		public static bool IsTempCached(this Song aSong)
		{
			return false;
		}

		public static bool IsFullyCached(this Song aSong)
		{
			// TODO: fix this
			return !Injection.Kernel.Get<IDownloadQueue>().IsMediaItemDownloading(aSong);
		}

		public static bool IsDownloaded(this Song aSong)
		{
			return false;
		}

		public static long LocalFileSize(this Song aSong)
		{
			long size = 0;
			try
			{
				size = new FileInfo(aSong.CurrentPath()).Length;
			}
			catch {}

			return size;
		}

		public static bool FileExists(this Song aSong)
		{
			return File.Exists(aSong.CurrentPath());
		}

		public static string CurrentPath(this Song aSong)
		{
			return aSong.DownloadPath();
		}
	}
}

