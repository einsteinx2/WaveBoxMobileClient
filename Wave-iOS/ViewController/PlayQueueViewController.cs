using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using System.Drawing;
using MonoTouch.Foundation;
using WaveBox.Core.Model;

namespace Wave.iOS.ViewController
{
	public class PlayQueueViewController : UIViewController
	{
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

			View.BackgroundColor = UIColor.FromRGB(233, 233, 233);

			CreateHeader();

			TableView = new UITableView(new RectangleF(70f, HeaderView.Frame.Height, 250f, View.Frame.Size.Height - HeaderView.Frame.Height));
			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			View.Add(TableView);

			Source = new TableSource(this.playQueueViewModel);

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60f;

			TableView.Source = Source;
			TableView.ReloadData();

			// Update the player info in case it happened before the view was loaded
			UpdatePlayerInfo(playQueueViewModel.CurrentItem as Song);

			playQueueViewModel.DataChanged += delegate(object sender, ViewModelEventArgs e) {
				BeginInvokeOnMainThread(()=> {
					// Reload the table
					TableView.ReloadData();

					// Update the player info
					UpdatePlayerInfo(playQueueViewModel.CurrentItem as Song);
				});
			};
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

		private void UpdatePlayerInfo(Song song)
		{
			if (song == null)
			{
				SongNameLabel.Text = null;
				ArtistNameLabel.Text = null;
			}
			else
			{
				SongNameLabel.Text = song.SongName;
				ArtistNameLabel.Text = song.ArtistName;
			}
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
				SongTableCell cell = tableView.DequeueReusableCell(cellIdentifier) as SongTableCell;
				if (cell == null)
				{
					cell = new SongTableCell(UITableViewCellStyle.Default, cellIdentifier);
					cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
					cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
					cell.TextLabel.BackgroundColor = UIColor.Clear;
					cell.BackgroundView = new UIView();
					cell.BackgroundView.BackgroundColor = UIColor.Clear;
					cell.TrackNumberLabel.Hidden = true;
				}

				IMediaItem mediaItem = playQueueViewModel.MediaItems[indexPath.Row];

				if (mediaItem is Song)
				{
					cell.Song = mediaItem as Song;
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

