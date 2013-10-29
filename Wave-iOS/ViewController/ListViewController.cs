using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using Wave.iOS.ViewController.Extensions;
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

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add);
			NavigationItem.RightBarButtonItem.Clicked += (object sender, EventArgs e) => {
				this.GetSidePanelController().ShowRightPanelAnimated(true);
			};

			CreateSearchHeader();
			TableView.ContentOffset = new PointF(0, SearchBar.Frame.Height);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			BeginInvokeOnMainThread(() => {
				SearchBar.ResignFirstResponder();
			});
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
			SearchBar.TextChanged += (object sender, UISearchBarTextChangedEventArgs e) => {
				if (e.SearchText.Length == 0)
				{
					SearchBar.ResignFirstResponder();
				}

				PerformSearch(e.SearchText);
			};

			SearchBar.OnEditingStarted += (object sender, EventArgs e) => {
				SearchBar.ShowsCancelButton = true;
			};

			SearchBar.OnEditingStopped += (object sender, EventArgs e) => {
				SearchBar.ShowsCancelButton = false;
				SearchBar.Text = "";
				PerformSearch(SearchBar.Text);
			};

			SearchBar.CancelButtonClicked += (object sender, EventArgs e) => {
				SearchBar.ResignFirstResponder();
				SearchBar.ShowsCancelButton = false;
			};

			SearchBar.SearchButtonClicked += (object sender, EventArgs e) => {
				((UISearchBar)sender).ResignFirstResponder();
			};
			return SearchBar;
		}
	}
}

