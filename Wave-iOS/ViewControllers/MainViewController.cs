using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WaveBox.Core;
using WaveBox.Model;
using System.IO;
using WaveBox.Core.Injection;
using WaveBox.Static;
using Ninject;
using WaveBox.Model.Repository;
using WaveBox.Client.AudioEngine;

namespace WaveiOS
{
	public partial class MainViewController : UIViewController, IBassGaplessPlayerDataSource
	{
		IDatabase database = Injection.Kernel.Get<IDatabase>();

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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Artist anArtist = new Artist ();
			Console.WriteLine("anArtist: " + anArtist);

			if (!File.Exists(database.DatabasePath()))
			{
				File.Copy("./wavebox.db", database.DatabasePath());
			}

			Console.WriteLine("song: " + new Song.Factory().CreateSong(6815).SongName);

			IBassGaplessPlayer player = Injection.Kernel.Get<IBassGaplessPlayer>();
			player.DataSource = this;
			player.StartWithOffsetInBytesOrSeconds(0, 0);

			BassPlaylistCurrentIndex = 0;

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

		public int BassPlaylistCount { get { return 1; } }
		public int BassPlaylistCurrentIndex { get; set; }
		public Song BassPlaylistCurrentSong { get { return new Song.Factory().CreateSong(6815); } }
		public Song BassPlaylistNextSong { get { return null; } }
		public bool BassIsOfflineMode { get { return false; } }
	}
}

