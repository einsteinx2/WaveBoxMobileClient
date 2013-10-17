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
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using Wave.iOS.ViewController.Extensions;

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

			AddBackgroundLayer();

			AddSearchBox();



			TableView = new UITableView(new RectangleF(0.0f, 64.0f, 250f, View.Frame.Size.Height - 70.0f));
			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			View.Add(TableView);

			TableView.BackgroundColor = UIColor.Clear;

			TableView.Source = Source;
			TableView.ReloadData();

			Source.RowSelected(TableView, NSIndexPath.FromRowSection(0, 0));
		}

		public void AddBackgroundLayer()
		{
			CGColor c1 = new CGColor(219f/255f, 147f/255f, 197f/255f, 1f);
			CGColor c2 = new CGColor(169f/255f, 164f/255f, 205f/255f, 1);
			CGColor c3 = new CGColor(148f/255f, 166f/255f, 200f/255f, 1);

			CGColor[] colors = { c1, c2, c3 };
			NSNumber[] locations = { new NSNumber(0f), new NSNumber(.48f), new NSNumber(1.0f) };
			CAGradientLayer layer = new CAGradientLayer();
			layer.Colors = colors;
			layer.Locations = locations;
			layer.Frame = View.Bounds;

			View.Layer.InsertSublayer(layer, 0);
		}

		public void AddSearchBox()
		{
			UIView searchBox = new UIView(new RectangleF(0, 0, this.View.Bounds.Width, 64.0f));
			searchBox.BackgroundColor = new UIColor(1.0f, 1.0f, 1.0f, .4f);
			this.View.AddSubview(searchBox);
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "MenuTableCell";

			private const int MENU_SECTION = 0;
			private const int PLAYLIST_SECTION = 1;
			private const int SETTINGS_SECTION = 2;

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
				return 3;
			}

			public override float GetHeightForFooter(UITableView tableView, int section)
			{
				return 20.0f;
			}

			public override float EstimatedHeightForFooter(UITableView tableView, int section)
			{
				return 20.0f;
			}

			public override UIView GetViewForFooter(UITableView tableView, int section)
			{
				return new UIView(new RectangleF(0, 0, tableView.Bounds.Width, 20.0f));
			}

			public override UIView GetViewForHeader(UITableView tableView, int section)
			{
				UIView headerView = new UIView(new RectangleF(0f, 0f, tableView.Frame.Width, 15.0f));
				headerView.BackgroundColor = UIColor.Clear;

				UILabel label = new UILabel(new RectangleF(15.0f, 5.0f, tableView.Frame.Width - 15.0f, 23.0f));
				label.TextColor = UIColor.White;
				label.Font = UIFont.FromName("HelveticaNeue-Bold", 11.0f);

				headerView.AddSubview(label);

				switch (section)
				{
					case 0:
						label.Text = "BROWSE";
						break;
					case 1:
						label.Text = "PLAYLISTS";
						break;
					case 2:
						label.Text = "SETTINGS";
						break;
				}


				return headerView;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				if (section == MENU_SECTION)
					return menuItemListViewModel.MenuItems.Count;
				else if (section == PLAYLIST_SECTION)
					return playlistListViewModel.Playlists.Count;
				else if (section == SETTINGS_SECTION)
					return 2;
				return 0;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null) cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
				cell.TextLabel.TextColor = UIColor.White;
				cell.BackgroundColor = UIColor.Clear;

				if (indexPath.Section == MENU_SECTION)
				{
					cell.TextLabel.Text = menuItemListViewModel.MenuItems[indexPath.Row].Title;
				}
				else if (indexPath.Section == PLAYLIST_SECTION)
				{
					cell.TextLabel.Text = playlistListViewModel.Playlists[indexPath.Row].PlaylistName;
				}
				else if (indexPath.Section == SETTINGS_SECTION)
				{
					switch (indexPath.Row)
					{
						case 0:
							cell.TextLabel.Text = "Settings";
							break;
						case 1:
							cell.TextLabel.Text = "Log out";
							break;
					}
				}

				return cell;
			}

//			public override string[] SectionIndexTitles(UITableView tableView)
//			{
//				return new string[] { "Browse", "Playlists" };
//			}

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
						UINavigationController c = controller as UINavigationController;
						c.PopToRootViewController(false);
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
				else if (indexPath.Section == SETTINGS_SECTION)
				{
					if (indexPath.Row == 1)
					{
						IClientSettings clientSettings = kernel.Get<IClientSettings>();
						clientSettings.ServerUrl = null;
						clientSettings.UserName = null;
						clientSettings.Password = null;
						clientSettings.SaveSettings();
						UIWindow window = UIApplication.SharedApplication.KeyWindow;
						window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>());
						return;
					}
				}
			}
		}
	}
}
