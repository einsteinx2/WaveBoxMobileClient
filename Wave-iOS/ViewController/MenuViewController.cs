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
using System.Collections;
using Wave.iOS.Extensions;

namespace Wave.iOS.ViewController
{
	public class MenuViewController : UIViewController
	{
		private static IKernel kernel = Injection.Kernel;

		private UIView HeaderView;
		private UIView FooterView;

		private UITableView TableView;
		private TableSource Source;

		private readonly IDictionary<string, object> styleDictionary;

		public MenuViewController(JASidePanelController sidePanelController, IDictionary<string, object> styleDictionary)
		{
			if (styleDictionary == null)
				throw new ArgumentNullException("styleDictionary");

			this.styleDictionary = styleDictionary;

			Source = new TableSource(sidePanelController, kernel.Get<IMenuItemListViewModel>(), kernel.Get<IPlaylistListViewModel>(), styleDictionary);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			HeaderView = new UIView(new RectangleF(0f, 0f, View.Frame.Width, float.Parse((string)styleDictionary["menu.staticHeader.height"])));
			HeaderView.BackgroundColor = UIColorExtension.FromHex((string)styleDictionary["menu.staticHeader.backgroundColor"], float.Parse((string)styleDictionary["menu.staticHeader.backgroundColorAlpha"]));

			UIView headerBottomBorder = new UIView(new RectangleF(HeaderView.Frame.Height - 1f, 0f, HeaderView.Frame.Width, 1f));
			headerBottomBorder.BackgroundColor = UIColorExtension.FromHex((string)styleDictionary["menu.staticHeader.bottomBorderColor"], float.Parse((string)styleDictionary["menu.staticHeader.bottomBorderColorAlpha"]));
			HeaderView.Add(headerBottomBorder);
			View.Add(HeaderView);

			UIImageView backgroundImageView = new UIImageView(new UIImage((string)styleDictionary["menu.backgroundImage"]));
			View.Add(backgroundImageView);

			TableView = new UITableView(new RectangleF(0.0f, 0.0f, 250f, View.Frame.Size.Height));
			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			TableView.TableHeaderView = new UIView(new RectangleF(0.0f, 0.0f, TableView.Frame.Size.Width, 60.0f));
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.RowHeight = float.Parse((string)styleDictionary["menu.row.height"]);
			TableView.BackgroundColor = UIColor.Clear;
			View.Add(TableView);

			TableView.Source = Source;
			TableView.ReloadData();

			Source.RowSelected(TableView, NSIndexPath.FromRowSection(0, 0));
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "MenuTableCell";

			private const int MENU_SECTION = 0;
			private const int PLAYLIST_SECTION = 1;

			private NSIndexPath SelectedIndexPath { get; set; }

			private IDictionary<string, UIViewController> ViewControllers { get; set; }

			private readonly JASidePanelController sidePanelController;
			private readonly IMenuItemListViewModel menuItemListViewModel;
			private readonly IPlaylistListViewModel playlistListViewModel;
			private readonly IDictionary<string, object> styleDictionary;

			public TableSource(JASidePanelController sidePanelController, IMenuItemListViewModel menuItemListViewModel, IPlaylistListViewModel playlistListViewModel, IDictionary<string, object> styleDictionary)
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
				this.styleDictionary = styleDictionary;

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
				/*
			StyleDictionary["menu.rowHeader.height"] = 20f;
			StyleDictionary["menu.rowHeader.fontColor"] = "FFFFFF";
			StyleDictionary["menu.rowHeader.fontColorAlpha"] = 1.0f;
			StyleDictionary["menu.rowHeader.fontName"] = "HelveticaNeue";
			StyleDictionary["menu.rowHeader.fontSize"] = 22.72f;*/

				UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null)
				{
					cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;
					cell.BackgroundView = new UIView();
				}

				if (indexPath.Equals(SelectedIndexPath))
				{
					cell.BackgroundView.BackgroundColor = UIColorExtension.FromHex((string)styleDictionary["menu.rowSelected.backgroundColor"], float.Parse((string)styleDictionary["menu.rowSelected.backgroundColorAlpha"]));
					cell.TextLabel.Font = UIFont.FromName((string)styleDictionary["menu.rowSelected.fontName"], float.Parse((string)styleDictionary["menu.rowSelected.fontSize"]));
					cell.TextLabel.TextColor = UIColorExtension.FromHex((string)styleDictionary["menu.rowSelected.fontColor"], float.Parse((string)styleDictionary["menu.rowSelected.fontColorAlpha"]));
				}
				else
				{
					cell.BackgroundView.BackgroundColor = UIColorExtension.FromHex((string)styleDictionary["menu.row.backgroundColor"], float.Parse((string)styleDictionary["menu.row.backgroundColorAlpha"]));
					cell.TextLabel.Font = UIFont.FromName((string)styleDictionary["menu.row.fontName"], float.Parse((string)styleDictionary["menu.row.fontSize"]));
					cell.TextLabel.TextColor = UIColorExtension.FromHex((string)styleDictionary["menu.row.fontColor"], float.Parse((string)styleDictionary["menu.row.fontColorAlpha"]));
				}

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
								window.RootViewController = new LoginViewController(window, kernel.Get<IPlayQueueViewModel>(), kernel.Get<ILoginViewModel>(), clientSettings.StyleDictionary);
								return;
						}

						ViewControllers[key] = controller;
					}

					sidePanelController.CenterPanel = controller;
				}
				else if (indexPath.Section == PLAYLIST_SECTION)
				{

				}

				SelectedIndexPath = indexPath;
				tableView.ReloadData();
			}
		}
	}
}
