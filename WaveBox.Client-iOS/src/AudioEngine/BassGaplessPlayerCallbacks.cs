using System;
using MonoTouch;
using WaveBox.Core;
using Ninject;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using WaveBox.Core.Model;
using Un4seen.Bass.AddOn.Mix;
using System.IO;

namespace WaveBox.Client.AudioEngine
{
	public partial class BassGaplessPlayer
	{
		// Xamarin.iOS doesn't support instance methods as C callbacks. So have to use Ninject.
		public class Procs
		{
			private static IBassGaplessPlayer player = Injection.Kernel.Get<IBassGaplessPlayer>();

			delegate void FILECLOSEPROC(IntPtr user);

			delegate void FILELENPROC(IntPtr user);

			delegate void FILEREADPROC(IntPtr buffer, int length, IntPtr user);

			delegate void FILESEEKPROC(long offset, IntPtr user);

			delegate void SYNCPROC(int handle, int channel, int data, IntPtr user);

			[MonoPInvokeCallback(typeof (SYNCPROC))]
			public static int StreamProc(int handle, IntPtr buffer, int length, IntPtr user)
			{
				return player.StreamProc(handle, buffer, length, user);
			}

			[MonoPInvokeCallback(typeof (SYNCPROC))]
			public static void StreamCrossfadeCallback(int handle, int channel, int data, IntPtr user)
			{
				player.StreamCrossfadeCallback(handle, channel, data, user);
			}

			[MonoPInvokeCallback(typeof (SYNCPROC))]
			public static void StreamEndCallback(int handle, int channel, int data, IntPtr user)
			{
				player.StreamEndCallback(handle, channel, data, user);
			}

			[MonoPInvokeCallback(typeof (FILECLOSEPROC))]
			public static void FileCloseProc(IntPtr user)
			{
				player.FileCloseProc(user);
			}

			[MonoPInvokeCallback(typeof (FILELENPROC))]
			public static long FileLenProc(IntPtr user)
			{
				return player.FileLenProc(user);
			}

			[MonoPInvokeCallback(typeof (FILEREADPROC))]
			public static int FileReadProc(IntPtr buffer, int length, IntPtr user)
			{
				return player.FileReadProc(buffer, length, user);
			}

			[MonoPInvokeCallback(typeof (FILESEEKPROC))]
			public static bool FileSeekProc(long offset, IntPtr user)
			{
				return player.FileSeekProc(offset, user);
			}
		}

		// This callback is called at song length - crossfadeInterval to start fading out the song and start the next song stream
		public void StreamCrossfadeCallback(int handle, int channel, int data, IntPtr user)
		{
			BassStream userInfo = StreamForIdentifierPtr(user);
			if (userInfo != null)
			{
				userInfo.IsCrossfadeStarted = true;
				userInfo.CrossfadeInterval = crossfadeInterval;

				// Start fading out the song (uses milliseconds)
				Bass.BASS_ChannelSlideAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, 0f, (int)(crossfadeInterval * 1000));

				// Plug in and start fading in next song stream
				int index;
				lock(streamQueue)
				{
					index = streamQueue.IndexOf(userInfo);
				}

				if (index == 0)
				{
					// Retreive the next song userInfo object
					lock(streamQueue)
					{
						userInfo = streamQueue[1];
					}

					if (userInfo != null)
					{
						// Plug in the next song stream
						bool success = BassMix.BASS_Mixer_StreamAddChannel(mixerStream, userInfo.MyStream, 0);

						// Fade in the next stream
						success = success && Bass.BASS_ChannelSetAttribute(userInfo.MyStream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
						success = success && Bass.BASS_ChannelSlideAttribute(userInfo.MyStream, BASSAttribute.BASS_ATTRIB_VOL, 1f, (int)(crossfadeInterval * 1000));
						if (logger.IsDebugEnabled) logger.Debug(@"crossfade, plugged next stream in started fading in with success: " + success);
					}
				}
			}
		}

		public void StreamEndCallback(int handle, int channel, int data, IntPtr user)
		{
			BassStream userInfo = StreamForIdentifierPtr(user);
			if (userInfo != null)
			{
				int index;
				lock(streamQueue)
				{
					index = streamQueue.IndexOf(userInfo);
				}

				// If this is the current playing song, mark it as ended and record the remaining buffer space
				if (index == 0)
				{
					// Synchronize here in case the total duration sync and end sync call at the same time on different threads
					lock(userInfo)
					{
						if (userInfo.IsEnded)
							return;

						userInfo.IsEnded = true;
					}

					userInfo.BufferSpaceTilSongEnd = ringBuffer.FilledSpaceLength();

					// Plug in the next stream if we didn't crossfade
					if (!userInfo.IsCrossfadeStarted)
					{
						BassStream nextUserInfo = null;
						lock(streamQueue)
						{
							if (streamQueue.Count >= 2)
							{
								nextUserInfo = streamQueue[1];
							}
						}

						if (nextUserInfo != null)
						{
							// Plug in the next song stream
							bool success = BassMix.BASS_Mixer_StreamAddChannel(mixerStream, nextUserInfo.MyStream, 0);
							if (logger.IsDebugEnabled) logger.Debug("stream end, plugged next stream " + nextUserInfo.MyStream + " in started fading in with success: " + success);
						}
					}
				}
			}
			else
			{
				logger.Error("stream end: somehow this is not the stream at index 0");
			}
		}

		public void FileCloseProc(IntPtr user)
		{	
			if (user == IntPtr.Zero)
				return;

			// Get the user info object
			BassStream userInfo = StreamForIdentifierPtr(user);

			// Tell the read wait loop to break in case it's waiting
			userInfo.ShouldBreakWaitLoop = true;
			userInfo.ShouldBreakWaitLoopForever = true;

			// Close the file handle
			if (userInfo.FileHandle != null)
				userInfo.FileHandle.Close();

			userInfo.ClearFileLengthQueryCount();
		}

		public long FileLenProc(IntPtr user)
		{
			if (user == IntPtr.Zero)
				return 0;

			BassStream userInfo = StreamForIdentifierPtr(user);
			if (userInfo == null || userInfo.FileHandle == null)
				return 0;

			long length = 0;
			Song theSong = userInfo.MySong;
			if (userInfo.ShouldBreakWaitLoopForever)
			{
				return 0;
			}
			else if (theSong.IsFullyCached() || userInfo.IsTempCached)
			{
				// Return actual file size on disk
				length = theSong.LocalFileSize();
			}
			else
			{
				// Return server reported file size
				length = (long)theSong.FileSize;
			}

			userInfo.IncrementFileLengthQueryCount();

			if (logger.IsDebugEnabled) logger.Debug("checking " + theSong + " length: " + length);
			return length;
		}

		public int FileReadProc(IntPtr buffer, int length, IntPtr user)
		{
			if (buffer == IntPtr.Zero || user == IntPtr.Zero)
				return 0;

			BassStream userInfo = StreamForIdentifierPtr(user);
			if (userInfo == null || userInfo.FileHandle == null)
				return 0;

			// If the buffer is not big enough, recreate it
			if (userInfo.ReadProcBuffer.Length < length)
			{
				userInfo.ReadProcBuffer = new byte[length];
			}

			// Read from the file
			int bytesRead = 0;
			try 
			{
				bytesRead = userInfo.FileHandle.Read(userInfo.ReadProcBuffer, 0, length);
			}
			catch (Exception e)
			{
				logger.Error("Exception in MyFileReadProc: " + e);
				bytesRead = 0;
			}

			if (bytesRead > 0)
			{
				if (bytesRead > 0)
				{
					// Copy the data to the buffer
					Marshal.Copy(userInfo.ReadProcBuffer, 0, buffer, bytesRead);
				}

				if (bytesRead < length && userInfo.IsSongStarted && !userInfo.WasFileJustUnderrun)
				{
					userInfo.IsFileUnderrun = true;
				}

				userInfo.WasFileJustUnderrun = false;

				userInfo.ClearFileLengthQueryCount();
			}

			return bytesRead;
		}

		public bool FileSeekProc(long offset, IntPtr user)
		{	
			if (user == IntPtr.Zero)
				return false;

			// Seek to the requested offset (returns false if data not downloaded that far)
			BassStream userInfo = StreamForIdentifierPtr(user);
			if (userInfo == null || userInfo.FileHandle == null)
				return false;

			bool success = true;

			try 
			{
				userInfo.FileHandle.Seek(offset, SeekOrigin.Begin);
			}
			catch (Exception e) 
			{
				logger.Error("Failed to seek: " + e);
				success = false;
			}

			userInfo.ClearFileLengthQueryCount();

			if (logger.IsDebugEnabled) logger.Debug(@"seeking to " + offset + "  success: " + success);

			return success;
		}

		private byte[] drainedBytes = new byte[32 * 1024];
		public int StreamProc(int handle, IntPtr buffer, int length, IntPtr user)
		{
			BassStream userInfo = CurrentStream;
			if (userInfo == null)
				return 0;

			// If the buffer is not big enough, recreate it
			if (drainedBytes.Length < length)
			{
				drainedBytes = new byte[length];
			}

			// Drain the bytes
			int bytesRead = ringBuffer.DrainBytes(drainedBytes, length);
			if (bytesRead > 0)
			{
				Marshal.Copy(drainedBytes, 0, buffer, bytesRead);
			}

			if (userInfo.IsEnded)
			{
				userInfo.BufferSpaceTilSongEnd -= bytesRead;
				if (userInfo.BufferSpaceTilSongEnd <= 0)
				{
					SongEnded(userInfo);
				}
			}

			Song currentSong = userInfo.MySong;
			if (bytesRead == 0 && Bass.BASS_ChannelIsActive(userInfo.MyStream) != BASSActive.BASS_ACTIVE_PLAYING && (currentSong.IsFullyCached() || currentSong.IsTempCached()))
			{
				this.IsPlaying = false;

				if (!userInfo.IsEndedCalled)
				{
					// Somehow songEnded: was never called
					SongEnded(userInfo);
				}

				// The stream should end, because there is no more music to play
				if (SongPlaybackEnded != null)
				{
					SongPlaybackEnded(this, new PlayerEventArgs(userInfo.MySong));
				}

				if (logger.IsDebugEnabled) logger.Debug(@"Stream not active, freeing BASS");

				// Clear state
				Cleanup();

				// Start the next song if for some reason this one isn't ready
				AudioEngine.StartSong();

				//return BASS_STREAMPROC_END;
				return 0;
			}

			return bytesRead;
		}
	}
}

