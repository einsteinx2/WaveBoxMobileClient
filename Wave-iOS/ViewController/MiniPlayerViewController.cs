using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

namespace Wave.iOS
{
	public class MiniPlayerViewController : UIViewController
	{
		UIImageView PlayPauseButton;
		UIImageView fullScreenPlayerButton;


		public MiniPlayerViewController(float xOffset, float yOffset)
		{
			View.Frame = new RectangleF(xOffset, yOffset, 260.0f, 64.0f);
		}

		public MiniPlayerViewController()
		{
			View.Frame = new RectangleF(0, 0, 260.0f, 64.0f);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = new UIColor(1, 1, 1, 0.6f);

			// add play/pause toggle
			PlayPauseButton = new UIImageView(new UIImage("Play.png"));
			PlayPauseButton.Frame = new RectangleF(0f, 20.0f, 50.0f, 50.0f);
			PlayPauseButton.UserInteractionEnabled = true;
			UITapGestureRecognizer playPauseTap = new UITapGestureRecognizer(this, new Selector("PlayPauseAction"));
			PlayPauseButton.AddGestureRecognizer(playPauseTap);
			View.AddSubview(PlayPauseButton);

			// add full screen player button

			// add scrubber

			// add media labels

			// add background layer
		}
		public void PlayPauseAction(object sender, EventArgs e)
		{
		}
	}
}

