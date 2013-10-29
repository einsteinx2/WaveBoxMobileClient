using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace Wave.iOS
{
	public class BrowsableTableCell : UITableViewCell
	{
		public BrowsableTableCell(string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
		{
//			ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
//			ImageView.ClipsToBounds = true;
		}

		public BrowsableTableCell(UIImage displayImage, string labelText, string reuseIdentifier) : this(reuseIdentifier)
		{
			// add image
			ImageView.Image = displayImage;

			// add label
			TextLabel.Text = labelText;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			ImageView.Frame = new RectangleF(0f, 0f, 59f, 59f);
			TextLabel.Frame = new RectangleF(ImageView.Frame.Right + 5f, 0f, Frame.Width - ImageView.Frame.Width - 5f, Frame.Height);
		}
	}
}

