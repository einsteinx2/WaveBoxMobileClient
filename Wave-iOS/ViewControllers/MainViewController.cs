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

namespace Wave.iOS
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

		static bool test = true;
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			button = new UIButton(new RectangleF(100f, 100f, 200f, 100f));
			button.SetTitle("Test", UIControlState.Normal);
			button.BackgroundColor = UIColor.Red;
			button.AddTarget((object sender, EventArgs args) => {
				if (test)
					player.StartWithOffsetInBytesOrSeconds(0, 265.0);
				else
					player.Stop();
				test = !test;
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
				clientSettings.SessionId = e.SessionId;
				label.Text = "Log in completed, downloading db\n" + clientSettings.SessionId;

				IDatabaseSyncLoader databaseLoader = kernel.Get<IDatabaseSyncLoader>();
				databaseLoader.DatabaseDownloaded += async delegate(object sender2, DatabaseSyncEventArgs e2) 
				{
					await clientDatabase.ReplaceDatabaseWithDownloaded();
					label.Text = "Database swapped, ready to listen!";
					button.Hidden = false;
				};
				databaseLoader.DownloadDatabase();
			};
			loginLoader.Login();


			// Perform any additional setup after loading the view, typically from a nib.
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

