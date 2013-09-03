using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using MonoTouch.Foundation;
using WaveBox.Core.Model;
using WaveBox.Core;
using Ninject;
using SDWebImage;
using System.Drawing;
using WaveBox.Client.ServerInteraction;

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
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.folderViewModel, this.NavigationController);

			if (folderViewModel.Folder != null)
			{
				Title = folderViewModel.Folder.FolderName;
			}

			if (folderViewModel.Folder.ArtId != null)
			{
				UIImageView headerImageView = new UIImageView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 320.0f));
				string coverUrlString = folderViewModel.Folder.ArtUrlString();
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
			private const int FOLDER_SECTION = 0;
			private const int SONG_SECTION = 1;
			private const int VIDEO_SECTION = 2;

			private string folderCellIdentifier = "FolderTableCell";
			private string songCellIdentifier = "FolderSongTableCell";

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
				if (indexPath.Section == FOLDER_SECTION)
				{
					UITableViewCell cell = tableView.DequeueReusableCell(folderCellIdentifier);
					if (cell == null)
					{
						cell = new UITableViewCell(UITableViewCellStyle.Default, folderCellIdentifier);
						cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
						cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
						cell.TextLabel.BackgroundColor = UIColor.Clear;
						cell.ImageView.ContentMode = UIViewContentMode.ScaleToFill;
					}

					Folder folder = folderViewModel.SubFolders[indexPath.Row];
					cell.TextLabel.Text = folder.FolderName;
					string artUrlString = folder.ArtUrlString(120);
					if (artUrlString != null)
					{
						cell.ImageView.SetImageWithURL(new NSUrl(artUrlString), new UIImage("BlankAlbumCell.png"), SDWebImageOptions.RetryFailed);
					}
					else
					{
						cell.ImageView.Image = new UIImage("BlankAlbumCell.png");
					}
					return cell;
				}
				else if (indexPath.Section == SONG_SECTION)
				{
					SongTableCell cell = tableView.DequeueReusableCell(songCellIdentifier) as SongTableCell;
					if (cell == null)
					{
						cell = new SongTableCell(UITableViewCellStyle.Default, songCellIdentifier);
					}

					cell.Song = folderViewModel.Songs[indexPath.Row];
					return cell;
				}
				else if (indexPath.Section == VIDEO_SECTION)
				{
					//Video video = folderViewModel.Videos[indexPath.Row];
					//cell.TextLabel.Text = video.FileName;
				}

				return null;
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

