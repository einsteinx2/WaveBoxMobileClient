using System;
using MonoTouch.UIKit;
using WaveBox.Client.ViewModel;
using WaveBox.Core.Model;
using MonoTouch.Foundation;

namespace Wave.iOS.ViewController
{
	public class GenreListViewController : UITableViewController
	{
		private TableSource Source { get; set; }

		private readonly IGenreListViewModel genreListViewModel;

		public GenreListViewController(IGenreListViewModel genreListViewModel)
		{
			if (genreListViewModel == null)
				throw new ArgumentNullException("genreListViewModel");

			this.genreListViewModel = genreListViewModel;

			Source = new TableSource(this.genreListViewModel);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = "Genres";

			TableView.BackgroundColor = UIColor.FromRGB(233, 233, 233);
			TableView.SeparatorColor = UIColor.FromRGB(207, 207, 207);
			TableView.RowHeight = 60.0f;

			TableView.Source = Source;
			TableView.ReloadData();
		}

		private class TableSource : UITableViewSource
		{
			private string cellIdentifier = "GenreListTableCell";

			private readonly IGenreListViewModel genreListViewModel;

			public TableSource(IGenreListViewModel genreListViewModel)
			{
				this.genreListViewModel = genreListViewModel;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				return genreListViewModel.Genres.Count;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
				if (cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
					cell.TextLabel.TextColor = UIColor.FromRGB(102, 102, 102);
					cell.TextLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
					cell.TextLabel.BackgroundColor = UIColor.Clear;
				}

				Genre genre = genreListViewModel.Genres[indexPath.Row];
				cell.TextLabel.Text = genre.GenreName;

				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{

			}
		}
	}
}
