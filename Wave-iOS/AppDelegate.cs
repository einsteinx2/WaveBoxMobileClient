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
using System.Threading.Tasks;

namespace Wave.iOS
{
	[Register ("AppDelegate")]
	public partial class WBAppDelegate : UIApplicationDelegate, IBassGaplessPlayerDataSource
	{
		IKernel kernel = Injection.Kernel;

		UIWindow window;
		public WBSidePanelController SidePanelController;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Initialize the Ninject kernel
			List<INinjectModule> modules = new List<INinjectModule>();
			modules.Add(new ClientModule());
			modules.Add(new iOSModule());
			kernel.Load(modules);

			IClientSettings clientSettings = kernel.Get<IClientSettings>();
			clientSettings.LoadSettings();

			clientSettings.StreamQueueLength = 2;

			BassPlaylistCurrentIndex = 0;

			IBassGaplessPlayer player = kernel.Get<IBassGaplessPlayer>();
			player.AudioEngine = kernel.Get<IAudioEngine>();
			player.DataSource = this;

			window = new UIWindow (UIScreen.MainScreen.Bounds);

			window.RootViewController = new UIViewController();
			window.MakeKeyAndVisible();

			UITextAttributes titleAttributes = new UITextAttributes();
			titleAttributes.TextColor = UIColor.FromRGB(102, 102, 102);
			titleAttributes.Font = UIFont.FromName("HelveticaNeue-Bold", 18.5f);
			titleAttributes.TextShadowColor = UIColor.Clear;
			UINavigationBar.Appearance.SetTitleTextAttributes(titleAttributes);

			if (clientSettings.ServerUrl != null)
			{
				ILoginLoader loginLoader = kernel.Get<ILoginLoader>();
				loginLoader.LoginCompleted += delegate(object sender, LoginEventArgs e) {
					Console.WriteLine("Login loader login completed, sessionId: " + e.SessionId + ", error: " + e.Error);
					InvokeOnMainThread(delegate {
						if (e.Error == null)
						{
							WBSidePanelController sidePanelController = new WBSidePanelController();
							sidePanelController.PanningLimitedToTopViewController = false;
							sidePanelController.LeftPanel = new MenuViewController(sidePanelController);
							sidePanelController.RightPanel = new PlayQueueViewController(kernel.Get<IPlayQueueViewModel>());
							window.RootViewController = sidePanelController;
							this.SidePanelController = sidePanelController;

							// TODO: rewrite this (Forces menu to load so the center is populated)
							Console.WriteLine("Menu view: " + sidePanelController.LeftPanel.View);
						}
						else
						{
							window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>());
						}
					});
				};
				loginLoader.Login();
			}
			else
			{
				window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>());
			}


			/*ILoginLoader loginLoader = kernel.Get<ILoginLoader>();
			loginLoader.LoginCompleted += delegate(object sender, LoginEventArgs e) 
			{
				Console.WriteLine("Login loader login completed, sessionId: " + e.SessionId + ", error: " + e.Error);
				if (e.Error == null)
				{
					clientSettings.SessionId = e.SessionId;

					IDatabaseSyncLoader databaseLoader = kernel.Get<IDatabaseSyncLoader>();
					databaseLoader.DatabaseDownloaded += delegate(object sender2, DatabaseSyncEventArgs e2) 
					{
						clientDatabase.ReplaceDatabaseWithDownloaded();

						InvokeOnMainThread (delegate { 
							Console.WriteLine("Setting sidepanels");
							sidePanelController = new JASidePanelController();
							sidePanelController.PanningLimitedToTopViewController = false;
							sidePanelController.LeftPanel = new MenuViewController(sidePanelController);
							sidePanelController.RightPanel = new PlayQueueViewController(kernel.Get<IPlayQueueViewModel>());
							window.RootViewController = sidePanelController;

							// TODO: rewrite this (Forces menu to load so the center is populated)
							Console.WriteLine("Menu view: " + sidePanelController.LeftPanel.View);
						});
					};
					databaseLoader.DownloadDatabase();
				}
			};
			loginLoader.Login();*/
			
			return true;
		}

		public int BassPlaylistCount { get { return 1; } }
		public int BassPlaylistCurrentIndex { get; set; }
		public Song BassPlaylistCurrentSong { get { return Injection.Kernel.Get<IPlayQueue>().CurrentItem as Song; } }
		public Song BassPlaylistNextSong { get { return Injection.Kernel.Get<IPlayQueue>().NextItem as Song; } }
		public bool BassIsOfflineMode { get { return false; } }
	}
}

