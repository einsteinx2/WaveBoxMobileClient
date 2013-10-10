using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using JASidePanels;

namespace Wave.iOS
{
	public partial class WBSidePanelController : JASidePanelController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		private bool statusBarHidden;
		public bool StatusBarHidden { 
			get
			{
				return statusBarHidden;
			}
			set
			{
				statusBarHidden = value;
				this.SetNeedsStatusBarAppearanceUpdate();
			}
		}

		public WBSidePanelController() : base()
		{
			statusBarHidden = false;
		}

		public override bool PrefersStatusBarHidden()
		{
			return this.StatusBarHidden;
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void StylePanel(UIView panel)
		{
			return;
		}

	}
}

