using System;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;

namespace WaveBox.Client.ServerInteraction
{
	public static class IItemExtension
	{
		public static string ArtUrlString(this IItem item, int size = 640)
		{
			IClientSettings clientSettings = Injection.Kernel.Get<IClientSettings>();
			if (item.ArtId == null)
				return null;
			else
				return string.Format("{0}/api/art?size={1}&s={2}&id={3}", clientSettings.ServerUrl, size, clientSettings.SessionId, item.ArtId);
		}
	}
}

