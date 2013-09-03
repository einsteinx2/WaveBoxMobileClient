using System;
using MonoTouch.UIKit;
using WaveBox.Core.Model;
using WaveBox.Core.Extensions;
using System.Drawing;

namespace Wave.iOS.ViewController
{
	public class SongTableCell : UITableViewCell
	{
		public UILabel TrackNumberLabel { get; set; }

		public UILabel SongNameLabel { get; set; }

		public UILabel DurationLabel { get; set; }

		private Song song;
		public Song Song 
		{ 
			get
			{
				return song;
			}
			set
			{
				song = value;

				if (song == null)
				{
					TrackNumberLabel.Text = null;
					SongNameLabel.Text = null;
					DurationLabel.Text = null;
				}
				else
				{
					TrackNumberLabel.Text = song.TrackNumber == null ? null : song.TrackNumber.ToString();
					SongNameLabel.Text = song.SongName;
					DurationLabel.Text = song.Duration == null ? null : ((int)song.Duration).ToTimeString();
				}
			}
		}

		public SongTableCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
		{
			TrackNumberLabel = new UILabel();
			TrackNumberLabel.BackgroundColor = UIColor.Clear;
			TrackNumberLabel.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin;
			TrackNumberLabel.TextColor = UIColor.FromRGB(102, 102, 102);
			TrackNumberLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
			TrackNumberLabel.TextAlignment = UITextAlignment.Center;
			Add(TrackNumberLabel);

			SongNameLabel = new UILabel();
			SongNameLabel.BackgroundColor = UIColor.Clear;
			SongNameLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			SongNameLabel.TextColor = UIColor.FromRGB(102, 102, 102);
			SongNameLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
			Add(SongNameLabel);

			DurationLabel = new UILabel();
			DurationLabel.BackgroundColor = UIColor.Clear;
			DurationLabel.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
			DurationLabel.TextColor = UIColor.FromRGB(102, 102, 102);
			DurationLabel.Font = UIFont.FromName("HelveticaNeue-Bold", 14.5f);
			DurationLabel.TextAlignment = UITextAlignment.Center;
			Add(DurationLabel);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			float height = Frame.Height;
			TrackNumberLabel.Frame = new RectangleF(0f, 0f, height, height);
			SongNameLabel.Frame = new RectangleF(height, 0f, Frame.Width - (height * 2), height);
			DurationLabel.Frame = new RectangleF(Frame.Width - height, 0f, height, height);
		}
	}
}

