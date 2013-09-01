using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;

namespace Wave.iOS.ViewController
{
	public class AlbumViewController : UIViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumViewModel albumViewModel;

		public AlbumViewController(IAlbumViewModel albumViewModel)
		{
			if (albumViewModel == null)
				throw new ArgumentNullException("albumViewModel");

			this.albumViewModel = albumViewModel;
			albumViewModel.ReloadData();

			Source = new TableSource(this.albumViewModel, this.NavigationController);
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "AlbumViewTableCell";

			private readonly IAlbumViewModel albumViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IAlbumViewModel albumViewModel, UINavigationController navigationController)
			{
				this.albumViewModel = albumViewModel;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 3;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return albumViewModel.Songs.Count;
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
				}

				Song song = albumViewModel.Songs[indexPath.Row];
				cell.TextLabel.Text = song.SongName;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				albumViewModel.PlaySongAtIndex(indexPath.Row);
			}
		}
	}
}

