using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using WaveBox.Client.ViewModel;
using System.Drawing;
using MonoTouch.Foundation;
using WaveBox.Core.Model;

namespace Wave.iOS.ViewController
{
	public class PlayQueueViewController : UIViewController
	{
		private MiniPlayerViewController MiniPlayer { get; set; }
		private UIView HeaderView { get; set; }
		private UIButton PlayPauseButton { get; set; }
		private UILabel SongNameLabel { get; set; }
		private UILabel ArtistNameLabel { get; set; }

		private UITableView TableView { get; set; }

		private TableSource Source { get; set; }

		private readonly IPlayQueueViewModel playQueueViewModel;

		public PlayQueueViewController(IPlayQueueViewModel playQueueViewModel)
		{
			if (playQueueViewModel == null)
				throw new ArgumentNullException("playQueueViewModel");

			this.playQueueViewModel = playQueueViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			float width = 260f;
			float xOffset = View.Frame.Width - width;

			AddBackgroundLayer();

			MiniPlayer = new MiniPlayerViewController(xOffset, 0f);
			View.AddSubview(MiniPlayer.View);
//			CreateHeader();

			TableView = new UITableView();
			TableView.Frame = new RectangleF(xOffset, 64.0f, width, View.Frame.Size.Height - 64.0f);
			View.AddSubview(TableView);
			TableView.BackgroundColor = UIColor.Clear;
			TableView.SeparatorInset = UIEdgeInsets.Zero;
//			var a = TableView.AutoresizingMask;
//			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			Source = new TableSource(this.playQueueViewModel);

//			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
//			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
//			TableView.RowHeight = 60f;

			TableView.Source = Source;
			TableView.ReloadData();

			playQueueViewModel.DataChanged += delegate(object sender, ViewModelEventArgs e) {
				InvokeOnMainThread(()=> {
					// Reload the table
					TableView.ReloadData();

					// Update the player info
					Song currentSong = playQueueViewModel.CurrentItem as Song;
//					if (currentSong == null)
//					{
//						SongNameLabel.Text = null;
//						ArtistNameLabel.Text = null;
//					}
//					else
//					{
//						SongNameLabel.Text = currentSong.SongName;
//						ArtistNameLabel.Text = currentSong.ArtistName;
//					}
				});
			};
		}

		public void AddBackgroundLayer()
		{
			CGColor c1 = new CGColor(219f/255f, 147f/255f, 197f/255f, 1f);
			CGColor c2 = new CGColor(169f/255f, 164f/255f, 205f/255f, 1);
			CGColor c3 = new CGColor(148f/255f, 166f/255f, 200f/255f, 1);

			CGColor[] colors = { c1, c2, c3 };
			NSNumber[] locations = { new NSNumber(0f), new NSNumber(.48f), new NSNumber(1.0f) };
			CAGradientLayer layer = new CAGradientLayer();
			layer.Colors = colors;
			layer.Locations = locations;
			layer.Frame = View.Frame;

			View.Layer.InsertSublayer(layer, 0);
		}

		private void CreateHeader()
		{
			float height = 60f;

			HeaderView = new UIView(new RectangleF(70f, 0f, 250f, height));
			View.Add(HeaderView);

			PlayPauseButton = new UIButton(UIButtonType.RoundedRect);
			PlayPauseButton.SetTitle(">/||", UIControlState.Normal);
			PlayPauseButton.Frame = new RectangleF(0f, 0f, height, height);
			PlayPauseButton.TouchUpInside += delegate(object sender, EventArgs e) {
				playQueueViewModel.PlayPauseToggle();
			};
			HeaderView.Add(PlayPauseButton);

			SongNameLabel = new UILabel(new RectangleF(height, 0f, HeaderView.Frame.Width - (height * 2), height / 2f));
			SongNameLabel.BackgroundColor = UIColor.Clear;
			SongNameLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 12f);
			SongNameLabel.TextColor = UIColor.FromRGB(107, 107, 107);
			HeaderView.Add(SongNameLabel);

			ArtistNameLabel = new UILabel(new RectangleF(height, height / 2f, HeaderView.Frame.Width - (height * 2), height / 2f));
			ArtistNameLabel.BackgroundColor = UIColor.Clear;
			ArtistNameLabel.Font = UIFont.FromName("HelveticaNeue", 11f);
			ArtistNameLabel.TextColor = UIColor.FromRGB(107, 107, 107);
			HeaderView.Add(ArtistNameLabel);
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "PlayQueueTableCell";

			private readonly IPlayQueueViewModel playQueueViewModel;

			public TableSource(IPlayQueueViewModel playQueueViewModel)
			{
				this.playQueueViewModel = playQueueViewModel;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return playQueueViewModel.MediaItems.Count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
					cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
					cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
					cell.TextLabel.BackgroundColor = UIColor.Clear;
					cell.BackgroundView = new UIView();
					cell.BackgroundView.BackgroundColor = UIColor.Clear;
				}

				IMediaItem mediaItem = playQueueViewModel.MediaItems[indexPath.Row];

				if (mediaItem is Song)
				{
					Song song = mediaItem as Song;
					cell.TextLabel.Text = song.SongName;
				}
				else if (mediaItem is Video)
				{
					Video video = mediaItem as Video;
					cell.TextLabel.Text = video.FileName;
				}

				if (indexPath.Row == playQueueViewModel.CurrentIndex)
				{
					cell.BackgroundView.BackgroundColor = UIColor.FromRGB(207, 207, 207);
				}
				else
				{
					cell.BackgroundView.BackgroundColor = UIColor.Clear;
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				playQueueViewModel.PlayItemAtIndex(indexPath.Row);
			}
		}
	}
}

