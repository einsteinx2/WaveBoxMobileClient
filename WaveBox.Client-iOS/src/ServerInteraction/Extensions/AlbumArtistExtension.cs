using System;
using WaveBox.Core;
using WaveBox.Core.Model;
using Ninject;

namespace WaveBox.Client.ServerInteraction
{
	public static class AlbumArtistExtension
	{
		public static string ArtUrlString(this AlbumArtist albumArtist)
		{
			IClientSettings clientSettings = Injection.Kernel.Get<IClientSettings>();
			if (albumArtist.MusicBrainzId == null)
				return null;
			else
				return string.Format("{0}/api/fanartthumb/?musicBrainzId={1}&s={2}", clientSettings.ServerUrl, albumArtist.MusicBrainzId, clientSettings.SessionId);
		}
	}
}

