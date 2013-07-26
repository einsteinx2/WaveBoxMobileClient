using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Ninject.Modules;
using WaveBox.Client;
using WaveBox.Core;
using WaveBox.Client.AudioEngine;
using Ninject;
using WaveBox.Core.Model;
using WaveBox.Core.Model.Repository;
using Wave.iOS.ViewControllers;

namespace Wave.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate, IBassGaplessPlayerDataSource
	{
		IKernel kernel = Injection.Kernel;

		UIWindow window;
		WebViewController webController;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Initialize the Ninject kernel
			List<INinjectModule> modules = new List<INinjectModule>();
			modules.Add(new ClientModule());
			modules.Add(new iOSModule());
			Injection.Kernel.Load(modules);

			IClientSettings clientSettings = kernel.Get<IClientSettings>();
			clientSettings.LoadSettings();
			if (clientSettings.ServerUrl == null)
			{
				Console.WriteLine("Setting server url");
				clientSettings.ServerUrl = "http://home.benjamm.in:6500";
				clientSettings.UserName = "test";
				clientSettings.Password = "test";
			}

			BassPlaylistCurrentIndex = 0;

			IBassGaplessPlayer player = kernel.Get<IBassGaplessPlayer>();
			player.AudioEngine = kernel.Get<IAudioEngine>();
			player.DataSource = this;

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			webController = kernel.Get<WebViewController>();
			window.RootViewController = webController;
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public int BassPlaylistCount { get { return 1; } }
		public int BassPlaylistCurrentIndex { get; set; }
		public Song BassPlaylistCurrentSong { get { return Injection.Kernel.Get<ISongRepository>().SongForId(6815); } }
		public Song BassPlaylistNextSong { get { return null; } }
		public bool BassIsOfflineMode { get { return false; } }
	}
}

