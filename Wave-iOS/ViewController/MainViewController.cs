using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WaveBox.Core;
using WaveBox.Core.Model;
using System.IO;
using Ninject;
using WaveBox.Core.Model.Repository;
using WaveBox.Client.AudioEngine;
using WaveBox.Client.ServerInteraction;
using WaveBox.Client;
using System.Collections.Generic;
using System.Linq;

namespace Wave.iOS.ViewController
{
	public partial class MainViewController : UIViewController
	{
		IKernel kernel = Injection.Kernel;
		IBassGaplessPlayer player = Injection.Kernel.Get<IBassGaplessPlayer>();
		IClientDatabase clientDatabase = Injection.Kernel.Get<IClientDatabase>();
		IClientSettings clientSettings = Injection.Kernel.Get<IClientSettings>();

		UIButton button;
		UILabel label;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public MainViewController () : base (UserInterfaceIdiomIsPhone ? "MainViewController_iPhone" : "MainViewController_iPad", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		//static bool test = true;
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			RectangleF frame = View.Frame;
			View.Frame = new RectangleF(frame.Location, new SizeF(250.0f, frame.Size.Height));

			button = new UIButton(new RectangleF(100f, 100f, 200f, 100f));
			button.SetTitle("Test", UIControlState.Normal);
			button.BackgroundColor = UIColor.Red;
			button.AddTarget((object sender, EventArgs args) => {
				/*if (test)
					player.StartWithOffsetInBytesOrSeconds(0, 265.0);
				else
					player.Stop();
				test = !test;*/

				player.SeekToPositionInSeconds(player.Progress + 30.0);

			}, UIControlEvent.TouchUpInside);
			button.Hidden = true;
			View.AddSubview(button);

			label = new UILabel(new RectangleF(0f, 0f, 320f, 80f));
			label.BackgroundColor = UIColor.Clear;
			label.Font = UIFont.SystemFontOfSize(20f);
			label.TextAlignment = UITextAlignment.Center;
			label.Lines = 0;
			label.Text = "Logging in...";
			View.AddSubview(label);

			ILoginLoader loginLoader = kernel.Get<ILoginLoader>();
			loginLoader.LoginCompleted += delegate(object sender, LoginEventArgs e) 
			{
				if (e.Error == null)
				{
					clientSettings.SessionId = e.SessionId;
					label.Text = "Log in completed, downloading db\n" + clientSettings.SessionId;

					IDatabaseSyncLoader databaseLoader = kernel.Get<IDatabaseSyncLoader>();
					databaseLoader.DatabaseDownloaded += async delegate(object sender2, DatabaseSyncEventArgs e2) 
					{
						await clientDatabase.ReplaceDatabaseWithDownloaded();
						label.Text = "Database swapped, ready to listen!";
						button.Hidden = false;

						// Get a list of songs for 2Pac
						var folder = kernel.Get<IFolderRepository>().FolderForId(57);
						var songs = folder.ListOfSongs();

						var playQueue = kernel.Get<IPlayQueue>();
						playQueue.AddItems(songs as List<IMediaItem>);
					};
					databaseLoader.DownloadDatabase();

					/*label.Text = "Database swapped, ready to listen!";
					button.Hidden = false;

					// Get a list of songs for 2Pac
					var folder = kernel.Get<IFolderRepository>().FolderForId(40420);
					var songs = folder.ListOfSongs();

					var playQueue = kernel.Get<IPlayQueue>();
					playQueue.AddItems(songs.Cast<IMediaItem>().ToList());*/

					//kernel.Get<IAudioEngine>().StartSong();
				}
				else
				{
					label.Text = "Login failed: " + e.Error;
				}
			};

			//loginLoader.Login();

			// Perform any additional setup after loading the view, typically from a nib

		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (UserInterfaceIdiomIsPhone) 
			{
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} 
			else 
			{
				return true;
			}
		}
	}
}
