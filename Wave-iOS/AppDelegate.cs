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

namespace Wave.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate, IBassGaplessPlayerDataSource
	{
		IKernel kernel = Injection.Kernel;

		// class-level declarations
		UIWindow window;
		MainViewController viewController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Initialize the Ninject kernel
			List<INinjectModule> modules = new List<INinjectModule>();
			modules.Add(new ClientModule());
			modules.Add(new iOSModule());
			Injection.Kernel.Load(modules);

			IClientSettings clientSettings = kernel.Get<IClientSettings>();
			clientSettings.ServerUrl = "http://home.benjamm.in:6500";
			clientSettings.UserName = "test";
			clientSettings.Password = "test";

			BassPlaylistCurrentIndex = 0;

			IBassGaplessPlayer player = kernel.Get<IBassGaplessPlayer>();
			player.AudioEngine = kernel.Get<IAudioEngine>();
			player.DataSource = this;

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new MainViewController ();
			window.RootViewController = viewController;
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

