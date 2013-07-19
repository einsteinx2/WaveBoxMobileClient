using System;
using WaveBox.Model;

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
			return true;
		}

		public static bool IsDownloaded(this Song aSong)
		{
			return true;
		}

		public static long LocalFileSize(this Song aSong)
		{
			return (long)aSong.FileSize;
		}

		public static bool FileExists(this Song aSong)
		{
			return true;
		}

		public static string CurrentPath(this Song aSong)
		{
			return "";
		}
	}
}

