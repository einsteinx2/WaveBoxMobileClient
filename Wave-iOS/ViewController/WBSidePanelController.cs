using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using JASidePanels;

namespace Wave.iOS
{
	public partial class WBSidePanelController : JASidePanelController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		private UIImageView FakeStatusBar;
		public override float LeftVisibleWidth
		{
			get
			{
				return 250.0f;
			}
		}

		public override float RightVisibleWidth
		{
			get
			{
				return 260.0f;
			}
		}

		public override void HandlePan(UIGestureRecognizer sender)
		{
			base.HandlePan(sender);
			Console.WriteLine(CenterPanel.View);

			if (sender.State == UIGestureRecognizerState.Began)
			{
				Console.WriteLine("PANEL PAN BEGAN");
				FakeStatusBar = CopyStatusBar();
				StatusBarHidden = true;
//				CenterPanelContainer.Frame = new RectangleF(CenterPanelContainer.Frame.Left, 20f, CenterPanelContainer.Frame.Width, CenterPanelContainer.Frame.Height - 20f);
			}

			if (sender.State == UIGestureRecognizerState.Ended || sender.State == UIGestureRecognizerState.Cancelled)
			{
				StatusBarHidden = false;
				Console.WriteLine("PANEL PAN ENDED OR CANCELED");
			}
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

		private UIStatusBarStyle statusBarStyle;
		public UIStatusBarStyle StatusBarStyle
		{
			get
			{
				return this.statusBarStyle;
			}
			set
			{
				this.statusBarStyle = value;
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

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			return this.statusBarStyle;
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

		private UIImageView CopyStatusBar()
		{
			return new UIImageView();
		}
	}
}

