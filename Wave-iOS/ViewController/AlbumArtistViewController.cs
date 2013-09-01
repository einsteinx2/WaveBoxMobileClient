using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using WaveBox.Core.Model;
using MonoTouch.Foundation;
using WaveBox.Core;
using Ninject;

namespace Wave.iOS.ViewController
{
	public class AlbumArtistViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumArtistViewModel albumArtistViewModel;

		public AlbumArtistViewController(IAlbumArtistViewModel albumArtistViewModel)
		{
			if (albumArtistViewModel == null)
				throw new ArgumentNullException("albumArtistViewModel");

			this.albumArtistViewModel = albumArtistViewModel;
			albumArtistViewModel.ReloadData();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.albumArtistViewModel, this.NavigationController);

			if (albumArtistViewModel.AlbumArtist != null)
			{
				Title = albumArtistViewModel.AlbumArtist.AlbumArtistName;
			}

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "AlbumArtistViewAlbumTableCell";

			private readonly IAlbumArtistViewModel albumArtistViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IAlbumArtistViewModel albumArtistViewModel, UINavigationController navigationController)
			{
				this.albumArtistViewModel = albumArtistViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return albumArtistViewModel.Albums.Count;
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

				Album album = albumArtistViewModel.Albums[indexPath.Row];
				cell.TextLabel.Text = album.AlbumName;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				IAlbumViewModel viewModel = Injection.Kernel.Get<IAlbumViewModel>();
				viewModel.Album = albumArtistViewModel.Albums[indexPath.Row];
				AlbumViewController controller = new AlbumViewController(viewModel);
				navigationController.PushViewController(controller, true);
			}
		}
	}
}

