using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using WaveBox.Core.Model;
using MonoTouch.Foundation;
using WaveBox.Core;
using Ninject;
using WaveBox.Client.ServerInteraction;
using SDWebImage;
using System.Drawing;

namespace Wave.iOS.ViewController
{
	public class AlbumArtistViewController : ListViewController
	{
		private TableSource Source { get; set; }

		private readonly IAlbumArtistViewModel albumArtistViewModel;

		public AlbumArtistViewController(IAlbumArtistViewModel albumArtistViewModel) : base(albumArtistViewModel)
		{
			if (albumArtistViewModel == null)
				throw new ArgumentNullException("albumArtistViewModel");

			this.albumArtistViewModel = albumArtistViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Source = new TableSource(this.albumArtistViewModel, this.NavigationController);

			if (albumArtistViewModel.AlbumArtist != null)
			{
				Title = albumArtistViewModel.AlbumArtist.AlbumArtistName;
			}

			if (albumArtistViewModel.AlbumArtist.ArtId != null)
			{
				UIImageView headerImageView = new UIImageView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 320.0f));
				string coverUrlString = albumArtistViewModel.AlbumArtist.ArtUrlString();
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
			private const int ALBUM_SECTION = 0;
			private const int SONG_SECTION = 1;

			private string albumCellIdentifier = "AlbumArtistViewAlbumTableCell";
			private string songCellIdentifier = "AlbumArtistViewSingleTableCell";

			private readonly IAlbumArtistViewModel albumArtistViewModel;
			private readonly UINavigationController navigationController;

			public TableSource(IAlbumArtistViewModel albumArtistViewModel, UINavigationController navigationController)
			{
				this.albumArtistViewModel = albumArtistViewModel;
				this.navigationController = navigationController;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 2;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return section == ALBUM_SECTION ? albumArtistViewModel.FilteredAlbums.Count : albumArtistViewModel.FilteredSingles.Count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == ALBUM_SECTION)
				{
					UITableViewCell cell = tableView.DequeueReusableCell(albumCellIdentifier);
					if (cell == null)
					{
						cell = new UITableViewCell(UITableViewCellStyle.Default, albumCellIdentifier);
						cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
						cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
						cell.TextLabel.BackgroundColor = UIColor.Clear;
					}

					Album album = albumArtistViewModel.FilteredAlbums[indexPath.Row];
					cell.TextLabel.Text = album.AlbumName;

					string artUrlString = album.ArtUrlString(120);
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
				else if (indexPath.Section == SONG_SECTION)
				{
					SongTableCell cell = tableView.DequeueReusableCell(songCellIdentifier) as SongTableCell;
					if (cell == null)
					{
						cell = new SongTableCell(UITableViewCellStyle.Default, songCellIdentifier);
						cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
						cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
						cell.TextLabel.BackgroundColor = UIColor.Clear;
					}

					Song song = albumArtistViewModel.FilteredSingles[indexPath.Row];
					cell.Song = song;

					return cell;
				}

				return null;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == ALBUM_SECTION)
				{
					IAlbumViewModel viewModel = Injection.Kernel.Get<IAlbumViewModel>();
					viewModel.Album = albumArtistViewModel.FilteredAlbums[indexPath.Row];
					AlbumViewController controller = new AlbumViewController(viewModel);
					navigationController.PushViewController(controller, true);
				}
				else if (indexPath.Section == SONG_SECTION)
				{
					albumArtistViewModel.PlaySongAtIndex(indexPath.Row);
				}
			}
		}
	}
}

