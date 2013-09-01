using System;
using System.Collections.Generic;
using WaveBox.Client.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IMenuItemListViewModel : IListViewModel
	{
		IList<MenuItem> MenuItems { get; set; }
	}
}

