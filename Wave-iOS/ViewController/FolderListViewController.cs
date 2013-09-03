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
	public class FolderListViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IFolderListViewModel folderListViewModel;

		public FolderListViewController(IFolderListViewModel folderListViewModel)
		{
			if (folderListViewModel == null)
				throw new ArgumentNullException("folderListViewModel");

			this.folderListViewModel = folderListViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.folderListViewModel, this.NavigationController);

			Title = "Folders";

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "FolderListTableCell";

			private readonly IFolderListViewModel folderListViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IFolderListViewModel folderListViewModel, UINavigationController navigationController)
			{
				this.folderListViewModel = folderListViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return folderListViewModel.Folders.Count;
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

				Folder folder = folderListViewModel.Folders[indexPath.Row];
				cell.TextLabel.Text = folder.FolderName;

				string artUrlString = folder.ArtUrlString(120);
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
				IFolderViewModel viewModel = Injection.Kernel.Get<IFolderViewModel>();
				viewModel.Folder = folderListViewModel.Folders[indexPath.Row];
				FolderViewController controller = new FolderViewController(viewModel);
				navigationController.PushViewController(controller, true);
			}
		}
	}
}

