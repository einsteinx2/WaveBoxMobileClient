using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using WaveBox.Client.ViewModel;
using WaveBox.Client.AudioEngine;
using WaveBox.Core;
using Ninject;

namespace Wave.iOS
{
	public class MiniPlayerViewController : UIViewController
	{
		UIImageView PlayPauseButton;
		UIImageView FullScreenPlayerButton;
		UILabel Duration;
		UILabel TimeElapsed;

		IPlayQueueViewModel PlayQueue = Injection.Kernel.Get<IPlayQueueViewModel>();
		IBassGaplessPlayer Player = Injection.Kernel.Get<IBassGaplessPlayer>();

		public MiniPlayerViewController(float xOffset, float yOffset)
		{
			View.Frame = new RectangleF(xOffset, yOffset, 260.0f, 64.0f);
		}

		public MiniPlayerViewController(PlayQueueViewModel p)
		{
			View.Frame = new RectangleF(0, 0, 260.0f, 64.0f);
			PlayQueue = p;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Player.PositionUpdate += UpdateElapsed;
			View.BackgroundColor = new UIColor(1, 1, 1, 0.6f);

			// add play/pause toggle
			PlayPauseButton = new UIImageView(new UIImage("Play.png"));
			PlayPauseButton.Frame = new RectangleF(0f, 20.0f, 50.0f, 50.0f);
			PlayPauseButton.UserInteractionEnabled = true;
			UITapGestureRecognizer playPauseTap = new UITapGestureRecognizer(this, new Selector("PlayPauseAction"));
			PlayPauseButton.AddGestureRecognizer(playPauseTap);
			View.AddSubview(PlayPauseButton);

			TimeElapsed = new UILabel(new RectangleF(50f, 0f, 50f, 50f));
			TimeElapsed.Text = "0";
			View.AddSubview(TimeElapsed);



//			PlayQueue.DataChanged += (object sender, ViewModelEventArgs e) => {
//
//			};

			// add full screen player button

			// add scrubber

			// add media labels

			// add background layer
		}
		public void PlayPauseAction(object sender, EventArgs e)
		{
			PlayQueue.PlayPauseToggle();
		}

		public void UpdateSong()
		{
		}

		public void UpdateElapsed(object sender, PlayerEventArgs e)
		{
			BeginInvokeOnMainThread(() => {
				TimeElapsed.Text = String.Format("{0}", Player.Position);
			});
		}
	}
}

