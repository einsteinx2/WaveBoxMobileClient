using System;
using WaveBox.Model;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace WaveBox.Client
{
	public class BassStream
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public BassGaplessPlayer Player { get; set; }

		public int MyStream { get; set; }
		public int CrossfadeSync { get; set; }
		public bool IsCrossfadeStarted { get; set; }
		public double CrossfadeInterval { get; set; }
		public Song MySong { get; set; }

		public int channelCount { get; set; }
		public int sampleRate { get; set; }

		//public NSFileHandle *fileHandle { get; set; }
		public bool ShouldBreakWaitLoop { get; set; }
		public bool ShouldBreakWaitLoopForever { get; set; }
		public long NeededSize { get; set; }
		public bool IsWaiting { get; set; }
		public string WritePath { get; set; }
		public bool IsTempCached { get; set; }
		public bool IsSongStarted { get; set; }
		public bool IsFileUnderrun { get; set; }
		public bool WasFileJustUnderrun { get; set; }

		public bool IsEnded { get; set; }
		public bool IsEndedCalled { get; set; }
		public int BufferSpaceTilSongEnd { get; set; }

		public int FileLengthQueryCount { get; set; }

		public bool ShouldIncrementIndex { get; set; }

		public BassStream ()
		{
			NeededSize = long.MaxValue;
			ShouldIncrementIndex = true;
		}

		public long localFileSize()
		{
			FileInfo f = new FileInfo (this.WritePath);
			return f.Length;
		}

		public override int GetHashCode()
		{
			return this.MyStream;
		}

		public bool Equals(BassStream otherStream) 
		{
			if (ReferenceEquals(this, otherStream))
				return true;

			if (ReferenceEquals(this.MySong, null) || ReferenceEquals(otherStream.MySong, null))
				return false;

			if (this.MySong.Equals(otherStream.MySong) && this.MyStream == otherStream.MyStream)
				return true;

			return false;
		}

		public override bool Equals(System.Object obj) 
		{
			if (ReferenceEquals(this, obj))
				return true;

			// If parameter cannot be cast to Point return false.
			BassStream otherStream = obj as BassStream;
			if ((System.Object)otherStream == null)
			{
				return false;
			}

			if (ReferenceEquals(this.MySong, null) || ReferenceEquals(otherStream.MySong, null))
				return false;

			if (this.MySong.Equals(otherStream.MySong) && this.MyStream == otherStream.MyStream)
				return true;

			return false;
		}

		public void IncrementFileLengthQueryCount()
		{
			this.FileLengthQueryCount++;

			if (this.FileLengthQueryCount == 5)
			{
				logger.Error(@"[BassStream] File length check happened > 5 times for %@, restarting the song", this.MySong);

				// This is a failure, notify the delegate
				if (this.Player.Delegate != null)
				{
					this.Player.Delegate.BassSongFailedToPlay (this.MySong);
				}

			}
		}

		public void ClearFileLengthQueryCount()
		{
			this.FileLengthQueryCount = 0;
		}

		public bool IsPluggedIntoMixer()
		{
			// GetMixer returns 0 if not plugged into a mixer, otherwise it returns the mixer handle
			return BassMix.BASS_Mixer_ChannelGetMixer(this.MyStream) != 0;
		}


		public override string ToString()
		{
			return base.ToString() + " - " + this.MyStream + " - " + this.MySong;
		}
	}
}
