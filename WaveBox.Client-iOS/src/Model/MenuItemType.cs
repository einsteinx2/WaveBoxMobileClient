using System;

namespace WaveBox.Client.Model
{
	public enum MenuItemType
	{
		Artists,
		AlbumArtists,
		Albums,
		Folders,
		Genres
	}

	public static class MenuItemExtension
	{
		public static string TitleForMenuItemType(this MenuItemType type)
		{
			switch (type)
			{
				case MenuItemType.Artists:
					return "Artists";
				case MenuItemType.AlbumArtists:
					return "Artists";
				case MenuItemType.Albums:
					return "Albums";
				case MenuItemType.Folders:
					return "Folders";
				case MenuItemType.Genres:
					return "Genres";
				default:
					return "";
			}
		}
	}
}

