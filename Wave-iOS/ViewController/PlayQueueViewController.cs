using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using System.Drawing;
using MonoTouch.Foundation;
using WaveBox.Core.Model;

namespace Wave.iOS.ViewController
{
	public class PlayQueueViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IPlayQueueViewModel playQueueViewModel;

		public PlayQueueViewController(IPlayQueueViewModel playQueueViewModel)
		{
			if (playQueueViewModel == null)
				throw new ArgumentNullException("playQueueViewModel");

			this.playQueueViewModel = playQueueViewModel;

			playQueueViewModel.DataChanged += delegate(object sender, ViewModelEventArgs e) {
				TableView.ReloadData();
			};
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.ContentInset = new UIEdgeInsets(0f, 70f, 0f, 0f);

			Source = new TableSource(this.playQueueViewModel);

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.TableHeaderView = new UIView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 60.0f));

			TableView.Source = Source;
			TableView.ReloadData();
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

