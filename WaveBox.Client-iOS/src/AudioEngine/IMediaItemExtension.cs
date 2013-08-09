using System;
using System.IO;
using WaveBox.Core;
using Ninject;
using WaveBox.Core.Model;

namespace WaveBox.Client.AudioEngine
{
	public static class IMediaItemExtension
	{
		public static string DownloadPath(this IMediaItem item)
		{
			return Path.Combine(Injection.Kernel.Get<IClientSettings>().DownloadsPath, item.FileName);
		}
	}
}

