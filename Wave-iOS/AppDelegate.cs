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
using WaveBox.Client.ViewModel;
using WaveBox.Client.ServerInteraction;
using System.Threading.Tasks;
using System.Threading;

namespace Wave.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate, IBassGaplessPlayerDataSource
	{
		IKernel kernel = Injection.Kernel;

		UIWindow window;

		IBassGaplessPlayer player;

		private int backgroundTask;
		private bool isInBackground;

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

			player = kernel.Get<IBassGaplessPlayer>();
			player.AudioEngine = kernel.Get<IAudioEngine>();
			player.DataSource = this;

			window = new UIWindow (UIScreen.MainScreen.Bounds);

			UIViewController blankController = new UIViewController();
			blankController.View.BackgroundColor = UIColor.White;

			window.RootViewController = blankController;
			window.MakeKeyAndVisible ();

			UINavigationBar.Appearance.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			UINavigationBar.Appearance.BackgroundColor = UIColor.FromRGB(233, 233, 233);
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
							JASidePanelController sidePanelController = new JASidePanelController();
							sidePanelController.PanningLimitedToTopViewController = false;
							sidePanelController.LeftPanel = new MenuViewController(sidePanelController, clientSettings.StyleDictionary);
							sidePanelController.RightPanel = new PlayQueueViewController(kernel.Get<IPlayQueueViewModel>());
							window.RootViewController = sidePanelController;

							// TODO: rewrite this (Forces menu to load so the center is populated)
							Console.WriteLine("Menu view: " + sidePanelController.LeftPanel.View);
						}
						else
						{
							window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>(), clientSettings.StyleDictionary);
						}
					});
				};
				loginLoader.Login();
			}
			else
			{
				window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>(), clientSettings.StyleDictionary);
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

		public override void WillEnterForeground(UIApplication application)
		{
			this.isInBackground = false;
			if (this.backgroundTask != 0)
			{
				//DLog(@"the app was backgrounded and NOT put to sleep and is now active again");
				application.EndBackgroundTask(backgroundTask);
				this.backgroundTask = 0;
			}
		}

		public override void DidEnterBackground(UIApplication application)
		{
			this.backgroundTask = application.BeginBackgroundTask( () => {
				// App is about to be put to sleep, stop the cache download queue
				// do that here

				// Make sure to end the background so we don't get killed by the OS
				application.EndBackgroundTask(this.backgroundTask);
				this.backgroundTask = 0;
			});

			new Task(() => {
				this.isInBackground = true;

				while (application.BackgroundTimeRemaining > 2.0 && this.isInBackground)
				{
					// Sleep early if no music is playing and we're not downloading songs or playing music
					if (application.BackgroundTimeRemaining < 300.0 && !player.IsPlaying)//&& !cacheQueueManagerS.isQueueDownloading && !AudioEngine.player.isPlaying)
					{
						application.EndBackgroundTask(this.backgroundTask);
						this.backgroundTask = 0;
						break;
					}

					/*// Warn at 2 minute mark if cache queue is downloading
					if ([application backgroundTimeRemaining] < 120.0 && cacheQueueManagerS.isQueueDownloading)
					{
						UILocalNotification *localNotif = [[UILocalNotification alloc] init];
						if (localNotif) 
						{
							localNotif.alertBody = NSLocalizedString(@"Songs are still downloading. Please return to Anghami within 2 minutes, as we will save resources by putting the application to sleep.", nil);
							localNotif.alertAction = NSLocalizedString(@"Open Anghami", nil);
							[application presentLocalNotificationNow:localNotif];
							break;
						}
					}*/

					// Sleep for a second to avoid a fast loop eating all cpu cycles
					Thread.Sleep(1000);
				}
			}).Start();
		}
		
		public int BassPlaylistCount { get { return 1; } }
		public int BassPlaylistCurrentIndex { get; set; }
		public Song BassPlaylistCurrentSong { get { return Injection.Kernel.Get<IPlayQueue>().CurrentItem as Song; } }
		public Song BassPlaylistNextSong { get { return Injection.Kernel.Get<IPlayQueue>().NextItem as Song; } }
		public bool BassIsOfflineMode { get { return false; } }
	}
}

