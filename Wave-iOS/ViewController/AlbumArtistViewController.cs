using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using WaveBox.Core.Model;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using WaveBox.Core;
using Ninject;
using WaveBox.Client.ServerInteraction;
using SDWebImage;
using System.Drawing;
using Wave.iOS.ViewController.Extensions;

namespace Wave.iOS.ViewController
{
	public partial class AlbumArtistViewController : ListViewController
	{
		private TableSource Source { get; set; }

		private UIView TableHeaderOverlay { get; set; }
		private UIButton BackButton { get; set; }
		private UIButton MenuButton { get; set; }
		private UIButton PlayQueueButton { get; set; }
		private bool TableHeaderOverlayHidden
		{
			get
			{
				return TableHeaderOverlay.Alpha == 0f;
			}
		}

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

			UIImageView headerImageView = new UIImageView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 320.0f));
			headerImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
			headerImageView.ClipsToBounds = true;
			if (albumArtistViewModel.AlbumArtist.MusicBrainzId != null)
			{
				string coverUrlString = albumArtistViewModel.AlbumArtist.ArtUrlString(false);
				if (coverUrlString != null)
					headerImageView.SetImageWithURL(new NSUrl(coverUrlString), new UIImage("BlankAlbumCell.png"), SDWebImageOptions.RetryFailed);
			}
			TableView.TableHeaderView = headerImageView;
			AddTableHeaderOverlay();

			TableView.ContentInset = new UIEdgeInsets(-20f, 0, 0, 0);
			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			this.GetSidePanelController().StatusBarStyle = UIStatusBarStyle.LightContent;
			if (NavigationController != null)
				NavigationController.NavigationBarHidden = true;
			var b = View.AlignmentRectInsets;
//			TableView.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
//			var a = this.NavigationController.View;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			NavigationController.NavigationBarHidden = false;
			this.GetSidePanelController().StatusBarStyle = UIStatusBarStyle.Default;
		}

		public void AddTableHeaderOverlay()
		{
			TableHeaderOverlay = new UIView();
			TableHeaderOverlay.Frame = TableView.TableHeaderView.Bounds;
			TableHeaderOverlay.ClipsToBounds = true;
			TableHeaderOverlay.BackgroundColor = UIColor.FromRGBA(0f, 0f, 0f, 0.75f);
			TableHeaderOverlay.UserInteractionEnabled = true;
			TableView.TableHeaderView.AddSubview(TableHeaderOverlay);

			BackButton = new UIButton(UIButtonType.System);
			BackButton.SetTitle("Back", UIControlState.Normal);
			BackButton.TitleLabel.TextColor = UIColor.White;
			BackButton.Frame = new RectangleF(0f, 0f, 50f, 50f);
			TableHeaderOverlay.AddSubview(BackButton);



			UITapGestureRecognizer tap = new UITapGestureRecognizer(ToggleHeaderOverlay);

			tap.Delegate = new TapDelegate(TableHeaderOverlay);
			tap.NumberOfTapsRequired = 1;
			tap.CancelsTouchesInView = false;
			TableView.AddGestureRecognizer(tap);

			UILabel artistName = new UILabel();
			artistName.Text = this.albumArtistViewModel.AlbumArtist.AlbumArtistName;
			artistName.Font = UIFont.FromName("HelveticaNeue-Bold", 17.0f);
			artistName.TextColor = UIColor.FromRGB(245f/255f, 245f/255f, 245f/255f);
			artistName.Frame = new RectangleF(25f, TableHeaderOverlay.Frame.Height - 80.0f, TableHeaderOverlay.Frame.Width - 50f, 30f);
			TableHeaderOverlay.AddSubview(artistName);

			UIView separator = new UIView();
			separator.BackgroundColor = UIColor.FromRGBA(255f, 255f, 255f, 0.5f);
			separator.Frame = new RectangleF(25f, TableHeaderOverlay.Frame.Height - 47.0f, TableHeaderOverlay.Frame.Width - 50f, 1f);
			TableHeaderOverlay.AddSubview(separator);
		}

		private void ToggleHeaderOverlay(UITapGestureRecognizer tap)
		{
			PointF coord = tap.LocationInView(TableView);
			if (BackButton.PointInside(coord, null) && !TableHeaderOverlayHidden)
			{
				NavigationController.PopViewControllerAnimated(true);
				return;
			}

			if (TableHeaderOverlay.PointInside(coord, null))
			{
				UIView.Animate(0.2, () => {
					if (TableHeaderOverlay.Alpha == 1f)
					{
						TableHeaderOverlay.Alpha = 0f;
					}
					else
					{
						TableHeaderOverlay.Alpha = 1f;
					}
				});
			}
//			else
//			{
//				TableView.Source.RowSelected(TableView, row);
//			}
		}
		 
		private class TapDelegate : UIGestureRecognizerDelegate
		{
			UIView TableHeaderOverlay { set; get; }
			public TapDelegate(UIView tableHeaderOverlay)
			{
				TableHeaderOverlay = tableHeaderOverlay;
			}

			public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
			{
				return true;
			}

			public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}


//
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
						cell = new BrowsableTableCell(albumCellIdentifier);
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

