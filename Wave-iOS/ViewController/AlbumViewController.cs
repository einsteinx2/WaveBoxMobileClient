using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using Wave.iOS.ViewController.Extensions;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using System.Drawing;
using WaveBox.Client.ServerInteraction;
using SDWebImage;

namespace Wave.iOS.ViewController
{
	public class AlbumViewController : ListViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumViewModel albumViewModel;

		public AlbumViewController(IAlbumViewModel albumViewModel) : base(albumViewModel)
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
			TableView.RowHeight = 50.0f;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		public override void ViewWillAppear(bool animated)
		{
//			this.GetSidePanelController().StatusBarStyle = UIStatusBarStyle.LightContent;
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
				return albumViewModel.FilteredSongs.Count;
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

				cell.BackgroundColor = indexPath.Row % 2 == 0 ? UIColor.FromRGB(233f/255f, 233f/255f, 233f/255f) : UIColor.FromRGB(255f, 255f, 255f);

				Song song = albumViewModel.FilteredSongs[indexPath.Row];
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

