using System;
using WaveBox.Client.Model;
using System.Collections.Generic;

namespace WaveBox.Client.ViewModel
{
	public class MenuItemListViewModel : IMenuItemListViewModel
	{
		public IList<MenuItem> MenuItems { get; set; }

		public MenuItemListViewModel()
		{
			MenuItems = new List<MenuItem>();
			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			throw new NotImplementedException();
		}

		public void ReloadData()
		{
			// For now show everything except Artists, as we'll use AlbumArtists instead, and put them in a particular order
			MenuItems.Clear();
			MenuItems.Add(new MenuItem(MenuItemType.AlbumArtists));
			MenuItems.Add(new MenuItem(MenuItemType.Albums));
			MenuItems.Add(new MenuItem(MenuItemType.Folders));
			MenuItems.Add(new MenuItem(MenuItemType.Genres));
			MenuItems.Add(new MenuItem(MenuItemType.Logout));
		}
	}
}

