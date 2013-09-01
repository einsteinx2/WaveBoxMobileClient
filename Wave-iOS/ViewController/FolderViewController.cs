using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;
using SDWebImage;
using System.Drawing;

namespace Wave.iOS.ViewController
{
	public class FolderViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IFolderViewModel folderViewModel;

		public FolderViewController(IFolderViewModel folderViewModel)
		{
			if (folderViewModel == null)
				throw new ArgumentNullException("folderViewModel");

			this.folderViewModel = folderViewModel;
			folderViewModel.ReloadData();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.folderViewModel, this.NavigationController);

			if (folderViewModel.Folder != null)
			{
				Title = folderViewModel.Folder.FolderName;
			}

			UIImageView headerImageView = new UIImageView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 320.0f));
			string coverUrlString = folderViewModel.CoverUrl;
			Console.WriteLine("coverUrlString: " + coverUrlString);
			headerImageView.SetImageWithURL(new NSUrl(coverUrlString));
			TableView.TableHeaderView = headerImageView;

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private const int FOLDER_SECTION = 0;
			private const int SONG_SECTION = 1;
			private const int VIDEO_SECTION = 2;

			private string cellIdentifier = "FolderTableCell";

			private readonly IFolderViewModel folderViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IFolderViewModel folderViewModel, UINavigationController navigationController)
			{
				this.folderViewModel = folderViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 3;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				switch (section)
				{
					case FOLDER_SECTION:
						return folderViewModel.SubFolders.Count;
					case SONG_SECTION:
						return folderViewModel.Songs.Count;
					case VIDEO_SECTION:
						return folderViewModel.Videos.Count;
					default:
						return 0;
				}
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

				switch (indexPath.Section)
				{
					case FOLDER_SECTION:
						Folder folder = folderViewModel.SubFolders[indexPath.Row];
						cell.TextLabel.Text = folder.FolderName;
						break;
					case SONG_SECTION:
						Song song = folderViewModel.Songs[indexPath.Row];
						cell.TextLabel.Text = song.SongName;
						break;
					case VIDEO_SECTION:
						Video video = folderViewModel.Videos[indexPath.Row];
						cell.TextLabel.Text = video.FileName;
						break;
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				switch (indexPath.Section)
				{
					case FOLDER_SECTION:
						IFolderViewModel viewModel = Injection.Kernel.Get<IFolderViewModel>();
						viewModel.Folder = folderViewModel.SubFolders[indexPath.Row];
						FolderViewController controller = new FolderViewController(viewModel);
						navigationController.PushViewController(controller, true);
						break;
					case SONG_SECTION:
						folderViewModel.PlaySongAtIndex(indexPath.Row);
						break;
					case VIDEO_SECTION:
						break;
				}
			}
		}
	}
}

