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
using WaveBox.Client;

namespace Wave.iOS.ViewController
{
	public class MenuViewController : UIViewController
	{
		private static IKernel kernel = Injection.Kernel;

		private UITableView TableView;

		private TableSource Source;

		public MenuViewController(JASidePanelController sidePanelController)
		{
			Source = new TableSource(sidePanelController, kernel.Get<IMenuItemListViewModel>(), kernel.Get<IPlaylistListViewModel>());
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromRGB(233, 233, 233);

			TableView = new UITableView(new RectangleF(0.0f, 0.0f, 250f, View.Frame.Size.Height));
			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			TableView.TableHeaderView = new UIView(new RectangleF(0.0f, 0.0f, TableView.Frame.Size.Width, 60.0f));
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			View.Add(TableView);

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);

			TableView.Source = Source;
			TableView.ReloadData();

			Source.RowSelected(TableView, NSIndexPath.FromRowSection(0, 0));
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
							case MenuItemType.Logout:
								IClientSettings clientSettings = kernel.Get<IClientSettings>();
								clientSettings.ServerUrl = null;
								clientSettings.UserName = null;
								clientSettings.Password = null;
								clientSettings.SaveSettings();
								UIWindow window = UIApplication.SharedApplication.KeyWindow;
								window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>());
								return;
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
