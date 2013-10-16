using System;
using MonoTouch.UIKit;
using JASidePanels;

namespace Wave.iOS.ViewController.Extensions
{
	public static class UIViewControllerExtensions
	{
		public static WBSidePanelController GetSidePanelController(this UIViewController vc)
		{
			// Maybe we should be walking the responder chain to find a JASidePanelController, but we only
			// have one side panel controller in our app, so we'll worry about that if/when the time comes.
			WBAppDelegate del = UIApplication.SharedApplication.Delegate as WBAppDelegate;
			return del.SidePanelController;
		}
	}
}

