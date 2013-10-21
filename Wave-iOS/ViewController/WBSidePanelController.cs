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

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			return statusBarStyle;
		}

		public override void StylePanel(UIView panel)
		{
			// do nothing
			return;
		}
	}
}

