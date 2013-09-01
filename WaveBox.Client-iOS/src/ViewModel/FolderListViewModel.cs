using System;
using Cirrious.MvvmCross.ViewModels;
using WaveBox.Core.Model;
using System.Collections.Generic;
using WaveBox.Core.Model.Repository;

namespace WaveBox.Client.ViewModel
{
	public class FolderListViewModel : IFolderListViewModel
	{
		public IList<Folder> Folders { get; set; }

		private readonly IFolderRepository folderRepository;

		public FolderListViewModel(IFolderRepository folderRepository)
		{
			if (folderRepository == null)
				throw new ArgumentNullException("folderRepository");

			this.folderRepository = folderRepository;

			ReloadData();
		}

		public void ReloadData()
		{
			Folders = folderRepository.TopLevelFolders();
		}
	}
}

