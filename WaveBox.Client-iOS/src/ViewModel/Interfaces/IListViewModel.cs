using System;

namespace WaveBox.Client.ViewModel
{
	public interface IListViewModel
	{
		void PerformSearch(string searchTerm);
		void ReloadData();
	}
}

