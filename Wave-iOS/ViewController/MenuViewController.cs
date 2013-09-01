using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using WaveBox.Client.ViewModel;
using WaveBox.Client.Model;
using JASidePanels;
using System.Collections.Generic;
using Ninject;
using WaveBox.Core;
using System.Drawing;

namespace Wave.iOS.ViewController
{
	public class MenuViewController : UITableViewController
	{
		private static IKernel kernel = Injection.Kernel;

		private TableSource Source;

		public MenuViewController(JASidePanelController sidePanelController)
		{
			Source = new TableSource(sidePanelController, kernel.Get<IMenuItemListViewModel>(), kernel.Get<IPlaylistListViewModel>());
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.ContentInset = new UIEdgeInsets(0f, 0f, 0f, 70f);

			TableView.TableHeaderView = new UIView(new RectangleF(0.0f, 0.0f, View.Frame.Size.Width, 60.0f));

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "MenuTableCell";

			private const int MENU_SECTION = 0;
			private const int PLAYLIST_SECTION = 1;

			private IDictionary<string, UIViewController> ViewControllers { get; set; }

			private readonly JASidePanelController sidePanelController;
			private readonly IMenuItemListViewModel menuItemListViewModel;
			private readonly IPlaylistListViewModel playlistListViewModel;

			public TableSource(JASidePanelController sidePanelController, IMenuItemListViewModel menuItemListViewModel, IPlaylistListViewModel playlistListViewModel)
			{
				if (sidePanelController == null)
					throw new ArgumentNullException ("sidePanelController");
				if (menuItemListViewModel == null)
					throw new ArgumentNullException ("menuItemListViewModel");
				if (playlistListViewModel == null)
					throw new ArgumentNullException ("playlistListViewModel");

				this.sidePanelController = sidePanelController;
				this.menuItemListViewModel = menuItemListViewModel;
				this.playlistListViewModel = playlistListViewModel;

				ViewControllers = new Dictionary<string, UIViewController>();
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 2;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				if (section == MENU_SECTION)
					return menuItemListViewModel.MenuItems.Count;
				else if (section == PLAYLIST_SECTION)
					return playlistListViewModel.Playlists.Count;
				return 0;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null) cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);

				if (indexPath.Section == MENU_SECTION)
				{
					cell.TextLabel.Text = menuItemListViewModel.MenuItems[indexPath.Row].Title;
				}
				else if (indexPath.Section == PLAYLIST_SECTION)
				{
					cell.TextLabel.Text = playlistListViewModel.Playlists[indexPath.Row].PlaylistName;
				}

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == MENU_SECTION)
				{
					MenuItem menuItem = menuItemListViewModel.MenuItems[indexPath.Row];
					string key = Enum.GetName(typeof(MenuItemType), menuItem.Type);
					UIViewController controller = null;
					UIViewController listController = null;
					if (ViewControllers.ContainsKey(key))
					{
						// Use the existing controller
						controller = ViewControllers[key];
					}
					else
					{
						// Load the controller
						switch (menuItem.Type)
						{
							case MenuItemType.AlbumArtists:
								listController = new AlbumArtistListViewController(kernel.Get<IAlbumArtistListViewModel>());
								controller = new UINavigationController(listController);
								break;
							case MenuItemType.Albums:
								listController = new AlbumListViewController(kernel.Get<IAlbumListViewModel>());
								controller = new UINavigationController(listController);
								break;
							case MenuItemType.Artists:
								break;
							case MenuItemType.Folders:
								listController = new FolderListViewController(kernel.Get<IFolderListViewModel>());
								controller = new UINavigationController(listController);
								break;
							case MenuItemType.Genres:
								listController = new GenreListViewController(kernel.Get<IGenreListViewModel>());
								controller = new UINavigationController(listController);
								break;
						}

						ViewControllers[key] = controller;
					}

					sidePanelController.CenterPanel = controller;
				}
				else if (indexPath.Section == PLAYLIST_SECTION)
				{

				}
			}
		}
	}
}
