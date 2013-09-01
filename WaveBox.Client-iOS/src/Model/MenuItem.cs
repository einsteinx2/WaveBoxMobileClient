using System;

namespace WaveBox.Client.Model
{
	public class MenuItem
	{
		public MenuItemType Type { get; set; }

		public string Title { get; set; }

		public MenuItem(MenuItemType type)
		{
			Type = type;
			Title = type.TitleForMenuItemType();
		}
	}
}

