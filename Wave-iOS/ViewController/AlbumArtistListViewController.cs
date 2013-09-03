using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;
using SDWebImage;
using WaveBox.Client.ServerInteraction;

namespace Wave.iOS.ViewController
{
	public class AlbumArtistListViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumArtistListViewModel albumArtistListViewModel;

		public AlbumArtistListViewController(IAlbumArtistListViewModel albumArtistListViewModel)
		{
			if (albumArtistListViewModel == null)
				throw new ArgumentNullException("albumArtistListViewModel");

			this.albumArtistListViewModel = albumArtistListViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.albumArtistListViewModel, this.NavigationController);

			Title = "Artists";

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "AlbumArtistListTableCell";

			private readonly IAlbumArtistListViewModel albumArtistListViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IAlbumArtistListViewModel albumArtistListViewModel, UINavigationController navigationController)
			{
				this.albumArtistListViewModel = albumArtistListViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return albumArtistListViewModel.AlbumArtists.Count;
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

				AlbumArtist albumArtist = albumArtistListViewModel.AlbumArtists[indexPath.Row];
				cell.TextLabel.Text = albumArtist.AlbumArtistName;

				string artUrlString = albumArtist.ArtUrlString(120);
				if (artUrlString != null)
				{
					cell.ImageView.SetImageWithURL(new NSUrl(artUrlString), new UIImage("BlankAlbumCell.png"), delegate(UIImage image, NSError error, SDImageCacheType cacheType) { });
				}
				else
				{
					cell.ImageView.Image = new UIImage("BlankAlbumCell.png");
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				IAlbumArtistViewModel viewModel = Injection.Kernel.Get<IAlbumArtistViewModel>();
				viewModel.AlbumArtist = albumArtistListViewModel.AlbumArtists[indexPath.Row];
				AlbumArtistViewController controller = new AlbumArtistViewController(viewModel);
				navigationController.PushViewController(controller, true);
			}
		}
	}
}

