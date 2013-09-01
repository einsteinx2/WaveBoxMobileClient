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
using Wave.iOS.ViewController;
using JASidePanels;
using WaveBox.Client.ViewModel;
using WaveBox.Client.ServerInteraction;

namespace Wave.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate, IBassGaplessPlayerDataSource
	{
		IKernel kernel = Injection.Kernel;

		UIWindow window;
		JASidePanelController sidePanelController;
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Initialize the Ninject kernel
			List<INinjectModule> modules = new List<INinjectModule>();
			modules.Add(new ClientModule());
			modules.Add(new iOSModule());
			kernel.Load(modules);

			IClientSettings clientSettings = kernel.Get<IClientSettings>();
			clientSettings.LoadSettings();
			if (clientSettings.ServerUrl == null)
			{
				Console.WriteLine("Setting server url");
				clientSettings.ServerUrl = "http://home.benjamm.in:6500";
				clientSettings.UserName = "test";
				clientSettings.Password = "test";
			}

			clientSettings.StreamQueueLength = 2;

			BassPlaylistCurrentIndex = 0;

			IBassGaplessPlayer player = kernel.Get<IBassGaplessPlayer>();
			player.AudioEngine = kernel.Get<IAudioEngine>();
			player.DataSource = this;

			window = new UIWindow (UIScreen.MainScreen.Bounds);

			UIViewController blankController = new UIViewController();
			blankController.View.BackgroundColor = UIColor.White;

			window.RootViewController = new UIViewController();
			window.MakeKeyAndVisible ();

			UINavigationBar.Appearance.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			UINavigationBar.Appearance.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			UITextAttributes titleAttributes = new UITextAttributes();
			titleAttributes.TextColor = UIColor.FromRGB(102, 102, 102);
			titleAttributes.Font = UIFont.FromName("HelveticaNeue-Bold", 18.5f);
			titleAttributes.TextShadowColor = UIColor.Clear;
			UINavigationBar.Appearance.SetTitleTextAttributes(titleAttributes);

			ILoginLoader loginLoader = kernel.Get<ILoginLoader>();
			loginLoader.LoginCompleted += delegate(object sender, LoginEventArgs e) 
			{
				Console.WriteLine("Login loader login completed, sessionId: " + e.SessionId + ", error: " + e.Error);
				if (e.Error == null)
				{
					clientSettings.SessionId = e.SessionId;

					InvokeOnMainThread (delegate { 
						Console.WriteLine("Setting sidepanels");
						sidePanelController = new JASidePanelController();
						sidePanelController.LeftPanel = new MenuViewController(sidePanelController);
						sidePanelController.CenterPanel = new MainViewController();
						sidePanelController.RightPanel = new PlayQueueViewController(kernel.Get<IPlayQueueViewModel>());
						window.RootViewController = sidePanelController;
					});

					/*IDatabaseSyncLoader databaseLoader = kernel.Get<IDatabaseSyncLoader>();
					databaseLoader.DatabaseDownloaded += delegate(object sender2, DatabaseSyncEventArgs e2) 
					{
						Console.WriteLine("Database loader database downloaded, e2: " + e2);
						clientDatabase.ReplaceDatabaseWithDownloaded();
						Console.WriteLine("Database replaced");

						InvokeOnMainThread (delegate { 
							Console.WriteLine("Setting sidepanels");
							sidePanelController = new JASidePanelController();
							sidePanelController.LeftPanel = new MenuViewController(sidePanelController);
							sidePanelController.CenterPanel = new MainViewController();
							sidePanelController.RightPanel = new UIViewController();
							window.RootViewController = sidePanelController;
						});
					};
					databaseLoader.DownloadDatabase();*/
				}
			};
			loginLoader.Login();
			
			return true;
		}

		public int BassPlaylistCount { get { return 1; } }
		public int BassPlaylistCurrentIndex { get; set; }
		public Song BassPlaylistCurrentSong { get { return Injection.Kernel.Get<IPlayQueue>().CurrentItem as Song; } }
		public Song BassPlaylistNextSong { get { return Injection.Kernel.Get<IPlayQueue>().NextItem as Song; } }
		public bool BassIsOfflineMode { get { return false; } }
	}
}

