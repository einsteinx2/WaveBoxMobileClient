using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using System.Drawing;

namespace Wave.iOS.ViewController
{
	public abstract class ListViewController : UITableViewController
	{
		public UISearchBar SearchBar { get; set; }

		private readonly IListViewModel listViewModel;

		public ListViewController(IListViewModel listViewModel)
		{
			if (listViewModel == null)
				throw new ArgumentNullException("listViewModel");

			this.listViewModel = listViewModel;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			CreateSearchHeader();
		}

		public void PerformSearch(string searchTerm)
		{
			listViewModel.PerformSearch(searchTerm);
			TableView.ReloadData();
		}

		public void CreateSearchHeader()
		{
			TableView.TableHeaderView = CreateSearchHeaderView();
		}

		public UIView CreateSearchHeaderView()
		{
			SearchBar = new UISearchBar(new RectangleF(0f, 0f, TableView.Frame.Width, 44f));
			SearchBar.TextChanged += delegate(object sender, UISearchBarTextChangedEventArgs e) {
				if (e.SearchText.Length == 0)
					((UISearchBar)sender).ResignFirstResponder();
				else
					PerformSearch(e.SearchText);
			};
			SearchBar.SearchButtonClicked += delegate(object sender, EventArgs e) {
				((UISearchBar)sender).ResignFirstResponder();
			};
			return SearchBar;
		}
	}
}

