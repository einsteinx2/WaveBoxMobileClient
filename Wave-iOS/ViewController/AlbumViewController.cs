using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using System.Drawing;
using WaveBox.Client.ServerInteraction;
using SDWebImage;

namespace Wave.iOS.ViewController
{
	public class AlbumViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumViewModel albumViewModel;

		public AlbumViewController(IAlbumViewModel albumViewModel)
		{
			if (albumViewModel == null)
				throw new ArgumentNullException("albumViewModel");

			this.albumViewModel = albumViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.albumViewModel);

			if (albumViewModel.Album != null)
			{
				Title = albumViewModel.Album.AlbumName;
			}

			if (albumViewModel.Album.ArtId != null)
			{
				UIImageView headerImageView = new UIImageView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 320.0f));
				string coverUrlString = albumViewModel.Album.ArtUrlString();
				if (coverUrlString != null)
					headerImageView.SetImageWithURL(new NSUrl(coverUrlString), new UIImage("BlankAlbumCell.png"), SDWebImageOptions.RetryFailed);
				TableView.TableHeaderView = headerImageView;
			}

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "AlbumViewTableCell";

			private readonly IAlbumViewModel albumViewModel;

			public TableSource(IAlbumViewModel albumViewModel)
			{
				this.albumViewModel = albumViewModel;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return albumViewModel.Songs.Count;
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
				}

				Song song = albumViewModel.Songs[indexPath.Row];
				cell.Song = song;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				albumViewModel.PlaySongAtIndex(indexPath.Row);
			}
		}
	}
}

