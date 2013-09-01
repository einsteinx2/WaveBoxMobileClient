using System;
using Ninject.Modules;
using WaveBox.Client.AudioEngine;
using WaveBox.Client.ServerInteraction;
using WaveBox.Core;
using WaveBox.Client.ViewModel;

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

			// ViewModels
			Bind<IAlbumArtistListViewModel>().To<AlbumArtistListViewModel>();
			Bind<IAlbumListViewModel>().To<AlbumListViewModel>();
			Bind<IArtistListViewModel>().To<ArtistListViewModel>();
			Bind<IFolderListViewModel>().To<FolderListViewModel>();
			Bind<IGenreListViewModel>().To<GenreListViewModel>();
			Bind<IPlaylistListViewModel>().To<PlaylistListViewModel>();
			Bind<IMenuItemListViewModel>().To<MenuItemListViewModel>();

			Bind<IAlbumArtistViewModel>().To<AlbumArtistViewModel>();
			Bind<IAlbumViewModel>().To<AlbumViewModel>();
			Bind<IArtistViewModel>().To<ArtistViewModel>();
			Bind<IFolderViewModel>().To<FolderViewModel>();

			Bind<IPlayQueueViewModel>().To<PlayQueueViewModel>();
		}
	}
}

