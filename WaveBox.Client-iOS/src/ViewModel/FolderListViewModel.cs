using System;
using Cirrious.MvvmCross.ViewModels;
using WaveBox.Core.Model;
using System.Collections.Generic;
using WaveBox.Core.Model.Repository;
using System.Linq;

namespace WaveBox.Client.ViewModel
{
	public class FolderListViewModel : IFolderListViewModel
	{
		public IList<Folder> Folders { get; set; }

		public IList<Folder> FilteredFolders { get; set; }

		private readonly IFolderRepository folderRepository;

		public FolderListViewModel(IFolderRepository folderRepository)
		{
			if (folderRepository == null)
				throw new ArgumentNullException("folderRepository");

			this.folderRepository = folderRepository;

			ReloadData();
		}

		public void PerformSearch(string searchTerm)
		{
			FilteredFolders = Folders.Where(x => x.FolderName.Contains(searchTerm)).ToList();
		}

		public void ReloadData()
		{
			Folders = folderRepository.TopLevelFolders();
			FilteredFolders = new List<Folder>(Folders);
		}
	}
}

