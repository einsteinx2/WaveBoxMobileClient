using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;

namespace Wave.iOS.ViewController
{
	public class AlbumListViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumListViewModel albumListViewModel;

		public AlbumListViewController(IAlbumListViewModel albumListViewModel)
		{
			if (albumListViewModel == null)
				throw new ArgumentNullException("albumListViewModel");

			this.albumListViewModel = albumListViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.albumListViewModel, this.NavigationController);

			Title = "Albums";

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "AlbumListTableCell";

			private readonly IAlbumListViewModel albumListViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IAlbumListViewModel albumListViewModel, UINavigationController navigationController)
			{
				this.albumListViewModel = albumListViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return albumListViewModel.Albums.Count;
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

				Album album = albumListViewModel.Albums[indexPath.Row];
				cell.TextLabel.Text = album.AlbumName;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				IAlbumViewModel viewModel = Injection.Kernel.Get<IAlbumViewModel>();
				viewModel.Album = albumListViewModel.Albums[indexPath.Row];
				AlbumViewController controller = new AlbumViewController(viewModel);
				navigationController.PushViewController(controller, true);
			}
		}
	}
}

