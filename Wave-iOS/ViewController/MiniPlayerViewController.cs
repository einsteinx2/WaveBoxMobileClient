using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using WaveBox.Client.ViewModel;
using WaveBox.Client.AudioEngine;
using WaveBox.Core;
using WaveBox.Core.Model;
using Ninject;

namespace Wave.iOS
{
	public class MiniPlayerViewController : UIViewController
	{
		UIImageView PlayPauseButton, FullScreenPlayerButton;
		UILabel Duration, TimeElapsed, SongTitle, ArtistName;

		UIImage playImage;
		UIImage PlayImage
		{
			get
			{
				if (playImage == null)
				{
					playImage = new UIImage("Play.png");
				}
				return playImage;
			}
		}

		UIImage pauseImage;
		UIImage PauseImage
		{
			get
			{
				if (pauseImage == null)
				{
					pauseImage = new UIImage("Pause.png");
				}
				return pauseImage;
			}

		}

		IPlayQueueViewModel PlayQueue = Injection.Kernel.Get<IPlayQueueViewModel>();
		IBassGaplessPlayer Player = Injection.Kernel.Get<IBassGaplessPlayer>();

		public MiniPlayerViewController(float xOffset, float yOffset)
		{
			View.Frame = new RectangleF(xOffset, yOffset, 260.0f, 64.0f);
		}

		public MiniPlayerViewController(PlayQueueViewModel p)
		{
			View.Frame = new RectangleF(0, 0, 260.0f, 64.0f);
		}

		#region Life cycle
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = new UIColor(1, 1, 1, 0.3f);

			UIView infoContainer = new UIView();
			infoContainer.BackgroundColor = new UIColor(1, 1, 1, 0.3f);
			infoContainer.Frame = new RectangleF(50f, 0f, 160f, 64f);
			View.AddSubview(infoContainer);

			// add play/pause toggle
			PlayPauseButton = new UIImageView(PlayImage);
			PlayPauseButton.Frame = new RectangleF(0f, 7.0f, 50.0f, 50.0f);
			PlayPauseButton.UserInteractionEnabled = true;
			UITapGestureRecognizer playPauseTap = new UITapGestureRecognizer(PlayPauseAction);
			PlayPauseButton.AddGestureRecognizer(playPauseTap);
			View.AddSubview(PlayPauseButton);

			// add full screen button
			FullScreenPlayerButton = new UIImageView(new UIImage("fullscreen.png"));
			FullScreenPlayerButton.Frame = new RectangleF(210f, 7f, 50f, 50f);
			View.AddSubview(FullScreenPlayerButton);

			// add elapsed time
			TimeElapsed = new UILabel(new RectangleF(110f, 15f, 45f, 13f));
			TimeElapsed.Font = UIFont.FromName("HelveticaNeue-Bold", 13f);
			TimeElapsed.TextColor = UIColor.FromRGB(51f / 255f, 51f / 255f, 51f / 255f);
			TimeElapsed.ShadowColor = UIColor.White;
			TimeElapsed.ShadowOffset = new SizeF(0f, 1f);
			TimeElapsed.Text = "0";
			TimeElapsed.TextAlignment = UITextAlignment.Right;
			infoContainer.AddSubview(TimeElapsed);

			// add duration
			Duration = new UILabel(new RectangleF(110f, 40f, 45f, 10f));
			Duration.Text = "0";
			Duration.TextAlignment = UITextAlignment.Right;
			Duration.Font = UIFont.FromName("HelveticaNeue-Bold", 10f);
			Duration.TextColor = UIColor.FromRGB(111f / 255f, 111f / 255f, 111f / 255f);
			Duration.ShadowColor = UIColor.White;
			Duration.ShadowOffset = new SizeF(0f, 1f);
			infoContainer.AddSubview(Duration);

			// add artist name
			ArtistName = new UILabel(new RectangleF(5f, 40f, 105f, 10f));
			ArtistName.Text = "";
			ArtistName.Font = UIFont.FromName("HelveticaNeue-Bold", 10f);
			ArtistName.TextColor = UIColor.FromRGB(111f / 255f, 111f / 255f, 111f / 255f);
			ArtistName.ShadowColor = UIColor.White;
			ArtistName.ShadowOffset = new SizeF(0f, 1f);
			infoContainer.AddSubview(ArtistName);

			// add song title
			SongTitle = new UILabel(new RectangleF(5f, 15f, 105f, 13f));
			SongTitle.Text = "";
			SongTitle.Font = UIFont.FromName("HelveticaNeue-Bold", 13f);
			SongTitle.TextColor = UIColor.FromRGB(51f / 255f, 51f / 255f, 51f / 255f);
			SongTitle.ShadowColor = UIColor.White;
			SongTitle.ShadowOffset = new SizeF(0f, 1f);
			infoContainer.AddSubview(SongTitle);



//			PlayQueue.DataChanged += (object sender, ViewModelEventArgs e) => {
//
//			};

			// add full screen player button

			// add scrubber

			// add media labels

			// add background layer
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			// register for events
			Player.PositionUpdate += UpdateElapsed;
			Player.SongPlaybackStarted += PlaybackStarted;
			Player.SongPlaybackPaused += PlaybackPaused;
			Player.SongPlaybackEnded += PlaybackEnded;
			PlayQueue.DataChanged += UpdateSong;

			if (Player.IsPlaying)
			{
				PlayPauseButton.Image = PauseImage;
			}

			// make sure labels are up to date
			UpdateSong(null, null);
			UpdateElapsed(null, null);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			// unregister events
			Player.PositionUpdate -= UpdateElapsed;
			Player.SongPlaybackStarted -= PlaybackStarted;
			Player.SongPlaybackPaused -= PlaybackPaused;
			Player.SongPlaybackEnded -= PlaybackEnded;
			PlayQueue.DataChanged -= UpdateSong;
		}

		#endregion

		#region Callbacks
		public void PlaybackStarted(object sender, PlayerEventArgs e)
		{
			BeginInvokeOnMainThread(() => {
				PlayPauseButton.Image = PauseImage;
			});
		}

		public void PlaybackPaused(object sender, PlayerEventArgs e)
		{
			BeginInvokeOnMainThread(() => {
				PlayPauseButton.Image = PlayImage;
			});
		}

		public void PlaybackEnded(object sender, PlayerEventArgs e)
		{
			BeginInvokeOnMainThread(() => {
				PlayPauseButton.Image = PlayImage;
			});
		}

		public void PlayPauseAction()
		{
			PlayQueue.PlayPauseToggle();
		}

		public void UpdateSong(object sender, ViewModelEventArgs e)
		{
			Song song = PlayQueue.CurrentItem as Song;
			if (song != null)
			{
				BeginInvokeOnMainThread(() => {
					SongTitle.Text = song.SongName;
					ArtistName.Text = song.ArtistName;
					Duration.Text = FormattedTime(song.Duration.Value);
				});
			}
		}

		public void UpdateElapsed(object sender, PlayerEventArgs e)
		{
			BeginInvokeOnMainThread(() => {
				TimeElapsed.Text = FormattedTime((int)Player.Position);
			});
		}
		#endregion

		private string FormattedTime(int duration)
		{
			TimeSpan t = TimeSpan.FromSeconds(duration);
			string time = "";
			if (t.Hours > 0)
			{
				time += String.Format("{0}:{1:D2}:", t.Hours, t.Minutes);
			}
			else
			{
				time += String.Format("{0}:", t.Minutes);
			}

			time += String.Format("{0:D2}", t.Seconds);

			return time;
		}
	}
}

