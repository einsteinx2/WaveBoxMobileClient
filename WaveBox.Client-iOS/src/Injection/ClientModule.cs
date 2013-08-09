using System;
using Ninject.Modules;
using WaveBox.Client.AudioEngine;
using WaveBox.Client.ServerInteraction;
using WaveBox.Core;

namespace WaveBox.Client
{
	public class ClientModule : NinjectModule
	{
		public override void Load() 
		{
			// Audio Engine
			Bind<IAudioEngine>().To<AudioEngine.AudioEngine>().InSingletonScope();
			Bind<IBassGaplessPlayer>().To<BassGaplessPlayer>().InSingletonScope();
			Bind<IBassWrapper>().To<BassWrapper>().InSingletonScope();

			// Other
			Bind<IPlayQueue>().To<PlayQueue>().InSingletonScope();
			Bind<IClientSettings>().To<ClientSettings>().InSingletonScope();
			Bind<IServerSettings>().To<ServerSettings>().InSingletonScope();

			// Loaders
			Bind<IDownloadQueue>().To<DownloadQueue>().InSingletonScope();
			Bind<IDatabaseSyncLoader>().To<DatabaseSyncLoader>();
			Bind<ILoginLoader>().To<LoginLoader>();
		}
	}
}

