using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IFolderListViewModel : IListViewModel
	{
		IList<Folder> Folders { get; set; }
	}
}

