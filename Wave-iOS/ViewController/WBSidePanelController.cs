using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;
using JASidePanels;
using System.Threading;

namespace Wave.iOS
{
	public partial class WBSidePanelController : JASidePanelController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		private UIView FakeScreen;
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

		public override bool BounceOnSidePanelOpen
		{
			get
			{
				return false;
			}
		}

		public override bool BounceOnCenterPanelChange
		{
			get
			{
				return false;
			}
		}

		public override void HandlePan(UIGestureRecognizer sender)
		{
			base.HandlePan(sender);
			Console.WriteLine(CenterPanel.View);

			if (sender.State == UIGestureRecognizerState.Began && VisiblePanel == CenterPanel)
			{
				AddFakeScreen();
			}

			if (sender.State == UIGestureRecognizerState.Ended || sender.State == UIGestureRecognizerState.Cancelled)
			{
				if (VisiblePanel == CenterPanel)
				{
					PerformSelector(new Selector("RemoveFakeScreen"), null, MaximumAnimationDuration);
				}
			}
		}

		public override void CenterPanelTapped(UIGestureRecognizer gesture)
		{
			base.CenterPanelTapped(gesture);
			PerformSelector(new Selector("RemoveFakeScreen"), null, MaximumAnimationDuration);
		}

		public override void ToggleLeftPanel(NSObject sender)
		{
			// We don't have to worry about the other toggle state, as we're adding
			// the fake screen over all the buttons
			AddFakeScreen();

			base.ToggleLeftPanel(sender);
		}

		public override void ToggleRightPanel(NSObject sender)
		{
			// We don't have to worry about the other toggle state, as we're adding
			// the fake screen over all the buttons
			AddFakeScreen();

			base.ToggleRightPanel(sender);
		}

		public void AddFakeScreen()
		{
			FakeScreen = UIScreen.MainScreen.SnapshotView(false);
			CenterPanel.View.AddSubview(FakeScreen);
			StatusBarHidden = true;
		}

		[Export("RemoveFakeScreen")]
		public void RemoveFakeScreen()
		{
			StatusBarHidden = false;
			NSTimer.CreateScheduledTimer(.01, () => {
				FakeScreen.RemoveFromSuperview();
				FakeScreen = null;
			});
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
				SetNeedsStatusBarAppearanceUpdate();
			}
		}

		private UIStatusBarStyle statusBarStyle;
		public UIStatusBarStyle StatusBarStyle
		{
			get
			{
				return statusBarStyle;
			}
			set
			{
				statusBarStyle = value;
				SetNeedsStatusBarAppearanceUpdate();
			}
		}

		public WBSidePanelController() : base()
		{
			statusBarHidden = false;
		}

		public override bool PrefersStatusBarHidden()
		{
			return StatusBarHidden;
		}

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			return statusBarStyle;
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

