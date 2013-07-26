using System;
using System.Threading;
using System.Collections.Generic;
using WaveBox.Core.Model;
using WaveBox.Core;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using System.Runtime.InteropServices;
using System.IO;
using Ninject;
using System.Timers;
using MonoTouch;

namespace WaveBox.Client.AudioEngine
{
	public class BassGaplessPlayer : IBassGaplessPlayer
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

		/*
		 * Events
		 */

		public event PlayerEventHandler PositionStarted;
		public event PlayerEventHandler SeekToPositionStarted;
		public event PlayerEventHandler SeekToPositionSuccess;
		public event PlayerEventHandler Stopped;
		public event PlayerEventHandler FirstStreamStarted;
		public event PlayerEventHandler SongEndedCalled;
		public event PlayerEventHandler SongEndedPlaylistIncremented;
		public event PlayerEventHandler SongEndedFinishedIsPlaying;
		public event PlayerEventHandler SongFailedToPlay;
		public event PlayerEventHandler SongPlaybackStarted;
		public event PlayerEventHandler SongPlaybackEnded;
		public event PlayerEventHandler SongPlaybackPaused;
		public event PlayerEventHandler BassFreed;
		public event PlayerEventHandler BassStopped;
		public event PlayerEventHandler StartedBuffering;
		public event PlayerEventHandler StillBuffering;
		public event PlayerEventHandler StoppedBuffering;

		public IBassGaplessPlayerDataSource DataSource { get; set; }

		public IAudioEngine AudioEngine { get; set; }

		private readonly IPlayQueue playQueue;
		private readonly IBassWrapper bassWrapper;
		private readonly IClientSettings clientSettings;

		private const int ISMS_BASSBufferSize = 800;
		private const int ISMS_defaultSampleRate = 44100;

		// Stream create failure retry values
		private const double ISMS_BassStreamRetryDelay = 2.0;
		private const int ISMS_BassStreamMinFilesizeToFail = 1024 * 1024;

		private const int audioEngineBufferNumberOfSeconds = 5;

		private double crossfadeInterval = 0;

		// Ring Buffer
		private RingBuffer ringBuffer = new RingBuffer(300 * 1024);
		private Thread ringBufferFillThread;

		private List<BassStream> tempStreamQueue = new List<BassStream>();

		// The main stream userInfo queue
		private List<BassStream> streamQueue = new List<BassStream>();

		// Using string instead of just the int here because Xamarin.iOS doesn't work well with value type keys,
		// must be a reference: http://docs.xamarin.com/guides/ios/advanced_topics/limitations
		private Dictionary<string, BassStream> streamDictionary = new Dictionary<string, BassStream>();

		private Song previousSongForProgress;
		private int outStream;
		private int mixerStream;

		public bool IsPlaying { get; private set; }

		public BassStream waitLoopStream;
		public bool isWaiting;
		public bool IsInitialBuffering { get; set; }

		private long startByteOffset;
		private int startSecondsOffset;

		public int numberOfRetries;

		private System.Timers.Timer startSongRetryTimer;
		private System.Timers.Timer nextSongRetryTimer;

		//public BassEqualizer Equalizer { get; set; }
		//public BassVisualizer Visualizer { get; set; }

		private int incrementingStreamIdentifier = 0;
		private object streamIdentifierLock = new object();
		private int GetStreamIdentifier()
		{
			lock (streamIdentifierLock)
			{
				incrementingStreamIdentifier++;
				return incrementingStreamIdentifier;
			}
		}

		private BassStream StreamForIdentifierPtr(IntPtr identifierPtr)
		{
			int streamIdentifier = identifierPtr.ToInt32();
			BassStream stream = null;
			streamDictionary.TryGetValue(streamIdentifier.ToString(), out stream);
			return stream;
		}
			
		public BassGaplessPlayer(IPlayQueue playQueue, IBassWrapper bassWrapper, IClientSettings clientSettings)
		{
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (bassWrapper == null)
				throw new ArgumentNullException("bassWrapper");
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.playQueue = playQueue;
			this.bassWrapper = bassWrapper;
			this.clientSettings = clientSettings;

			Un4seen.Bass.BassNet.Registration("ben@einsteinx2.com", "2X11231418232922");

			//_equalizer = [[BassEqualizer alloc] init];
			//_visualizer = [[BassVisualizer alloc] init];

			//[NSNotificationCenter removeObserverOnMainThread:self];
			//[NSNotificationCenter addObserverOnMainThread:self selector:@selector(prepareNextSongStream) name:Notification_RepeatModeChanged object:nil];
			//[NSNotificationCenter addObserverOnMainThread:self selector:@selector(prepareNextSongStream) name:Notification_CurrentPlaylistOrderChanged object:nil];
			//[NSNotificationCenter addObserverOnMainThread:self selector:@selector(prepareNextSongStream) name:Notification_CurrentPlaylistShuffleToggled object:nil];
			//[NSNotificationCenter addObserverOnMainThread:self selector:@selector(crossfadeIntervalChanged) name:Notification_BassCrossfadeIntervalChanged object:nil];
		}

		private void CrossfadeIntervalChanged()
		{    
			if (crossfadeInterval == 0.0)
			{
				// Remove any existing crossfades if the crossfade hasn't started yet
				foreach (BassStream userInfo in streamQueue)
				{
					if (userInfo.CrossfadeSync != 0 && !userInfo.IsCrossfadeStarted)
					{
						Bass.BASS_ChannelRemoveSync(userInfo.MyStream, userInfo.CrossfadeSync);
						userInfo.CrossfadeSync = 0;
					}
				}
			}
			else
			{
				// Add the crossfades or adjust the existing ones, if the crossfades haven't started yet
				foreach (BassStream userInfo in streamQueue)
				{
					if (!userInfo.IsCrossfadeStarted)
					{
						// Remove any existing sync
						if (userInfo.CrossfadeSync != 0)
						{
							Bass.BASS_ChannelRemoveSync(userInfo.MyStream, userInfo.CrossfadeSync);
						}

						// Add the new sync

						long position = Bass.BASS_ChannelSeconds2Bytes(userInfo.MyStream, (float)userInfo.MySong.Duration - crossfadeInterval);
						userInfo.PointerHandle = GCHandle.Alloc(userInfo, GCHandleType.Pinned);
						userInfo.CrossfadeSync = Bass.BASS_ChannelSetSync(userInfo.MyStream, BASSSync.BASS_SYNC_POS, position, new SYNCPROC(Procs.StreamCrossfadeCallback), userInfo.PointerHandle.AddrOfPinnedObject());
					}
				}
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
						if (logger.IsDebugEnabled) logger.Debug(@"crossfade, plugged next stream in started fading in with success: {0}", success);
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
							if (logger.IsDebugEnabled) logger.Debug("stream end, plugged next stream {0} in started fading in with success: {1}", nextUserInfo.MyStream, success);
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

			if (logger.IsDebugEnabled) logger.Debug("checking {0} length: {1}", theSong, length);
			return length;
		}

		public int FileReadProc(IntPtr buffer, int length, IntPtr user)
		{
			if (logger.IsDebugEnabled) logger.Debug(@"Ring Buffer - Starting MyFileReadProc");

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
				if (logger.IsDebugEnabled) logger.Debug(@"Ring Buffer - MyFileReadProc 1");

				if (bytesRead > 0)
				{
					// Copy the data to the buffer
					Marshal.Copy(userInfo.ReadProcBuffer, 0, buffer, bytesRead);
				}

				if (bytesRead < length && userInfo.IsSongStarted && !userInfo.WasFileJustUnderrun)
				{
					userInfo.IsFileUnderrun = true;
				}

				if (logger.IsDebugEnabled) logger.Debug(@"Ring Buffer - MyFileReadProc 2");

				userInfo.WasFileJustUnderrun = false;

				userInfo.ClearFileLengthQueryCount();

				if (logger.IsDebugEnabled) logger.Debug(@"Ring Buffer - Finished MyFileReadProc");
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

			if (logger.IsDebugEnabled) logger.Debug(@"seeking to {0}  success: {1}", offset, success);

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

		private void MoveToNextSong()
		{
			if (DataSource.BassPlaylistNextSong != null)
			{
				AudioEngine.PlaySongAtPosition(playQueue.NextIndex);
			}
			else
			{
				Cleanup();
			}
		}

		// songEnded: is called AFTER MyStreamEndCallback, so the next song is already actually decoding into the ring buffer
		private void SongEnded(BassStream userInfo)
		{
			userInfo.IsEndedCalled = true;

			if (SongEndedCalled != null)
			{
				SongEndedCalled(this, new PlayerEventArgs(null));
			}

			previousSongForProgress = userInfo.MySong;
			ringBuffer.TotalBytesDrained = 0;

			// Remove the stream from the queue
			if (userInfo != null)
			{
				Bass.BASS_StreamFree(userInfo.MyStream);
			}
			lock(streamQueue)
			{
				streamQueue.RemoveAt(0);
				streamDictionary.Remove(userInfo.Identifier.ToString());
			}

			// Increment current playlist index
			if (userInfo.ShouldIncrementIndex)
			{
				playQueue.IncrementIndex();
			}

			// Get the next song in the queue
			PrepareNextSongStream(playQueue.NextSong, false);

			Song endedSong = userInfo.MySong;

			if (SongEndedPlaylistIncremented != null)
			{
				SongEndedPlaylistIncremented(this, new PlayerEventArgs(endedSong));
			}

			// Send song end notification
			if (SongPlaybackEnded != null)
			{
				SongPlaybackEnded(this, new PlayerEventArgs(endedSong));
			}

			if (CurrentStream != null && CurrentStream.MySong != null && !CurrentStream.MySong.Equals(DataSource.BassPlaylistCurrentSong))
			{
				// Check to see if songs were added after the song finished decoding but before the song finished playing
				// The songs don't match, so we're playing the wrong song, instead restart the player
				if (logger.IsDebugEnabled) logger.Debug("songEnded but the current stream doesn't match the current play queue song, so restarting the player");
				AudioEngine.PlaySongAtPosition(playQueue.CurrentIndex);
			}
			else if (IsPlaying && CurrentStream != null)
			{
				if (logger.IsDebugEnabled) logger.Debug("songEnded: self.isPlaying = true");
				startSecondsOffset = 0;
				startByteOffset = 0;
				if (userInfo.IsCrossfadeStarted)
					ringBuffer.TotalBytesDrained = Bass.BASS_ChannelSeconds2Bytes(CurrentStream.MyStream, userInfo.CrossfadeInterval) / CurrentStream.ChannelCount;

				// Send song start notification
				if (SongPlaybackStarted != null)
				{
					SongPlaybackStarted(this, new PlayerEventArgs(null));
				}

				// Mark the last played time in the database for cache cleanup
				//CurrentStream.MySong.PlayedDate = [NSDate date];

				if (SongEndedFinishedIsPlaying != null)
				{
					SongEndedFinishedIsPlaying(this, new PlayerEventArgs(endedSong));
				}
			}
			else
			{
				if (logger.IsDebugEnabled) logger.Debug("songEnded: self.isPlaying = false");
				AudioEngine.StartSong();
			}
		}

		private void KeepRingBufferFilled()
		{
			// Cancel the existing thread if needed
			if (ringBufferFillThread != null)
			{
				ringBufferFillThread.Abort();
			}

			// Create and start a new thread to fill the buffer
			ringBufferFillThread = new Thread(new ThreadStart(KeepRingBufferFilledInternal));
			ringBufferFillThread.Start();
		}

		private int BytesForSecondsAtBitrate(int seconds, int bitrate) 
		{
			return (bitrate / 8) * 1024 * seconds;
		}

		private byte[] tempBuffer = new byte[64 * 1024];
		private void KeepRingBufferFilledInternal()
		{
			// Grab the mixerStream and ringBuffer as local references, so that if cleanup is run, and we're still inside this loop
			// it won't start filling the new buffer
			RingBuffer localRingBuffer = ringBuffer;
			int localMixerStream = mixerStream;

			while (true)
			{            
				// Fill the buffer if there is empty space
				if (localRingBuffer.FreeSpaceLength() > tempBuffer.Length)
				{
					// Read data to fill the buffer
					BassStream userInfo = CurrentStream;


					int tempLength = Bass.BASS_ChannelGetData(localMixerStream, tempBuffer, tempBuffer.Length);
					if (logger.IsDebugEnabled) logger.Debug(@"Ring Buffer - After BASS_ChannelGetData");
					if (tempLength > 0)
					{
						userInfo.IsSongStarted = true;

						localRingBuffer.FillWithBytes(tempBuffer, 0, tempLength);
					}

					RingBufferCheckForUnderrun(userInfo);
				}

				// Sleep for 1/6th of a second to prevent a tight loop
				Thread.Sleep(150);
			}
		}

		private void RingBufferCheckForUnderrun(BassStream userInfo)
		{
			if (userInfo == null)
				return;

			// Handle pausing to wait for more data
			if (userInfo.IsFileUnderrun && Bass.BASS_ChannelIsActive(userInfo.MyStream) != BASSActive.BASS_ACTIVE_STOPPED)
			{
				// Get a strong reference to the current song's userInfo object, so that
				// if the stream is freed while the wait loop is sleeping, the object will
				// still be around to respond to shouldBreakWaitLoop
				waitLoopStream = userInfo;

				// Mark the stream as waiting
				userInfo.IsWaiting = true;
				userInfo.IsFileUnderrun = false;
				userInfo.WasFileJustUnderrun = true;

				// Handle waiting for additional data
				Song theSong = userInfo.MySong;
				if (logger.IsDebugEnabled) logger.Debug("We had a file underrun for song: {0}  at path: {1}", theSong, theSong.CurrentPath());
				if (logger.IsDebugEnabled) logger.Debug("theSong.isFullyCached: {0}  theSong.localFileSize: {1}", theSong.IsFullyCached(), theSong.LocalFileSize());

				if (!theSong.IsFullyCached())
				{
					if (clientSettings.IsOfflineMode)
					{
						// This is offline mode and the song can not continue to play
						MoveToNextSong();
					}
					else
					{
						// Calculate the needed size:
						// Choose either the current player bitrate, or if for some reason it is not detected properly,
						// use the best estimated bitrate. Then use that to determine how much data to let download to continue.

						long size = theSong.LocalFileSize();
						int bitrate = bassWrapper.EstimateBitrate(userInfo);

						long bytesToWait = BytesForSecondsAtBitrate(audioEngineBufferNumberOfSeconds, bitrate);
						userInfo.NeededSize = size + bytesToWait;

						// Make sure that the needed size is never larger than the size of the song
						long serverSize = (long)userInfo.MySong.FileSize;
						if (serverSize < userInfo.NeededSize)
						{
							// We can never reach the needed size, so change it to the server reported size
							userInfo.NeededSize = serverSize;
						}

						if (logger.IsDebugEnabled) logger.Debug("audioEngineBufferNumberOfSeconds: {0}", audioEngineBufferNumberOfSeconds);
						if (logger.IsDebugEnabled) logger.Debug("AUDIO ENGINE - waiting for {0}   neededSize: {1} for song: {2}  at path: {3}", bytesToWait, userInfo.NeededSize, userInfo.MySong, userInfo.MySong.CurrentPath());

						// Sleep for 1/100th of a second
						int sleepTime = 10;
						// Check file size every second
						int fileSizeCheckWait = 1000;
						int totalSleepTime = 0;
						if (StartedBuffering != null)
						{
							StartedBuffering(this, new PlayerEventArgs(userInfo.MySong));
						}
						isWaiting = true;
						while (true)
						{
							// Check if we should break every 100th of a second
							Thread.Sleep(sleepTime);
							totalSleepTime += sleepTime;
							if (userInfo.ShouldBreakWaitLoop || userInfo.ShouldBreakWaitLoopForever)
							{
								isWaiting = false;
								if (StoppedBuffering != null)
								{
									StoppedBuffering(this, new PlayerEventArgs(userInfo.MySong));
								}
								break;
							}

							// Only check the file size every second
							if (totalSleepTime >= fileSizeCheckWait)
							{
								totalSleepTime = 0;

								if (logger.IsDebugEnabled) logger.Debug("Checking file size again, userInfo: {0}  song: {1}  isFullyCached: {2}  path: {3}  localFileSize: {4}  neededSize: {5}", userInfo, userInfo.MySong, userInfo.MySong.IsFullyCached(), userInfo.MySong.CurrentPath(), userInfo.MySong.LocalFileSize(), userInfo.NeededSize);

								// If enough of the file has downloaded, break the loop
								if (userInfo.LocalFileSize() >= userInfo.NeededSize ||
								    // Handle temp cached songs ending. When they end, they are set as the last temp cached song, so we know it's done and can stop waiting for data.
								    //(theSong.IsTempCached() && theSong.Equals(streamManagerS.lastTempCachedSong) ||
								    // If the song has finished caching, we can stop waiting
								    theSong.IsFullyCached() ||
								    // If we're not in online mode so there's no way of getting more bytes, stop waiting and try next song
								    clientSettings.IsOfflineMode)
								{
									if (logger.IsDebugEnabled) logger.Debug("breaking the wait loop, userInfo: {0}  song: {1}  isFullyCached: {2}  path: {3}  localFileSize: {4}  neededSize: {5}", userInfo, userInfo.MySong, userInfo.MySong.IsFullyCached(), userInfo.MySong.CurrentPath(), userInfo.MySong.LocalFileSize(), userInfo.NeededSize);

									isWaiting = false;
									if (StoppedBuffering != null)
									{
										StoppedBuffering(this, new PlayerEventArgs(userInfo.MySong));
									}

									if (clientSettings.IsOfflineMode)
									{
										MoveToNextSong();
									}
									break;
								}

								if (StillBuffering != null)
								{
									StillBuffering(this, new PlayerEventArgs(userInfo.MySong));
								}
							}
						}
						if (logger.IsDebugEnabled) logger.Debug(@"done waiting");
					}
				}

				userInfo.IsWaiting = false;
				userInfo.ShouldBreakWaitLoop = false;
				waitLoopStream = null;
			}
		}

		private void Cleanup()
		{
			if (startSongRetryTimer != null)
			{
				startSongRetryTimer.Stop();
				startSongRetryTimer = null;
			}
			if (nextSongRetryTimer != null)
			{
				nextSongRetryTimer.Stop();
				nextSongRetryTimer = null;
			}

			if (ringBufferFillThread != null)
			{
				ringBufferFillThread.Abort();
			}

			lock(streamQueue)
			{
				foreach (BassStream userInfo in streamQueue)
				{
					userInfo.ShouldBreakWaitLoopForever = true;
					Bass.BASS_StreamFree(userInfo.MyStream);
				}
			}

			Bass.BASS_StreamFree(mixerStream);
			Bass.BASS_StreamFree(outStream);

			IsPlaying = false;
			isWaiting = false;

			ringBuffer.Reset();

			lock(streamQueue)
			{
				streamQueue.Clear();
				tempStreamQueue.Clear();
				streamDictionary.Clear();
			}

			if (BassFreed != null)
			{
				BassFreed(this, new PlayerEventArgs(null));
			}
		}

		private BassStream CreateUserInfoForSong(Song aSong)
		{
			// Create the user info object for the stream
			BassStream userInfo = new BassStream();
			userInfo.MySong = aSong;
			userInfo.WritePath = aSong.CurrentPath();
			userInfo.IsTempCached = aSong.IsTempCached();
			userInfo.FileHandle = new FileStream("./Broken.mp3", FileMode.Open, FileAccess.Read); //new FileStream(userInfo.WritePath, FileMode.Open, FileAccess.Read);
			userInfo.Identifier = GetStreamIdentifier();
			userInfo.Player = this;
			if (userInfo.FileHandle == null)
			{
				// File failed to open
				logger.Error("File failed to open");
				return null;
			}

			streamDictionary[userInfo.Identifier.ToString()] = userInfo;
			return userInfo;
		}

		private BassStream PrepareStreamForSong(Song aSong)
		{
			if (logger.IsDebugEnabled) logger.Debug(@"preparing stream for {0}  file: {1}", aSong, aSong.CurrentPath());
			if (aSong.FileExists())
			{
				// Check if we're offline and the song is not downloaded or not fully cached
				if (clientSettings.IsOfflineMode && (!aSong.IsDownloaded() || !aSong.IsFullyCached()))
					return null;

				// Create the user info object for the stream
				BassStream userInfo = CreateUserInfoForSong(aSong);
				if (userInfo == null)
					return null;

				IntPtr streamIdentifier = new IntPtr(userInfo.Identifier);

				BASS_FILEPROCS procs = new BASS_FILEPROCS(new FILECLOSEPROC(Procs.FileCloseProc), 
				                                          new FILELENPROC(Procs.FileLenProc), 
				                                          new FILEREADPROC(Procs.FileReadProc), 
				                                          new FILESEEKPROC(Procs.FileSeekProc));

				// Create the stream
				int fileStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_NOBUFFER, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT, procs, streamIdentifier);

				// First check if the stream failed because of a BASS_Init error
				if (fileStream == 0 && Bass.BASS_ErrorGetCode() == BASSError.BASS_ERROR_INIT)
				{
					// Retry the regular hardware sampling stream
					logger.Error("Failed to create stream for {0} with hardware sampling because BASS is not initialized somehow, initializing BASS and trying again with hardware sampling", aSong);

					bassWrapper.BassInit();
					fileStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_NOBUFFER, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT, procs, streamIdentifier);
				}

				// Stream failed to start, try different parameters
				if(fileStream == 0)
				{
					logger.Error("Failed to create stream for {0} with hardware sampling, trying again with software sampling", aSong);
					bassWrapper.LogError();

					// Create the user info object for the stream
					userInfo = CreateUserInfoForSong(aSong);
					if (userInfo == null)
						return null;

					// Create the stream
					fileStream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_NOBUFFER, BASSFlag.BASS_SAMPLE_SOFTWARE | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT, procs, streamIdentifier);
				}

				if (fileStream != 0)
				{
					// Add the stream free callback
					Bass.BASS_ChannelSetSync(fileStream, BASSSync.BASS_SYNC_END, 0, Procs.StreamEndCallback, streamIdentifier);

					// Also set a sync on the total duration because it can take a few seconds after playback finishes before the END sync is called
					long totalDurationPosition = Bass.BASS_ChannelSeconds2Bytes(fileStream, (double)aSong.Duration);
					Bass.BASS_ChannelSetSync(fileStream, BASSSync.BASS_SYNC_POS, totalDurationPosition, Procs.StreamEndCallback, streamIdentifier);

					// Stream successfully created
					userInfo.MyStream = fileStream;
					userInfo.Player = this;

					// Ask BASS how many channels are on this stream
					BASS_CHANNELINFO info = new BASS_CHANNELINFO();
					Bass.BASS_ChannelGetInfo(fileStream, info);
					userInfo.ChannelCount = info.chans;
					userInfo.SampleRate = info.freq;

					// Add the fade out callback
					if (crossfadeInterval > 0.0)
					{
						long position = Bass.BASS_ChannelSeconds2Bytes(fileStream, (double)(aSong.Duration - crossfadeInterval));
						userInfo.CrossfadeSync = Bass.BASS_ChannelSetSync(fileStream, BASSSync.BASS_SYNC_POS, position, new SYNCPROC(Procs.StreamCrossfadeCallback), streamIdentifier);
					}

					// Store the userInfo object here temporarily until we add it to the regular stream queue to try and prevent an EXC_BAD_ACCESS that happens sometimes
					lock(streamQueue)
					{
						tempStreamQueue.Add(userInfo);
					}

					// Successfully created a stream, so reset the retry counter
					numberOfRetries = 0;

					return userInfo;
				}

				logger.Error("Failed to create stream for {0} with software sampling", aSong);
				bassWrapper.LogError();
				if (userInfo.FileHandle != null)
				{
					userInfo.FileHandle.Close();
					userInfo.FileHandle = null;
				}
				userInfo.Player = null;

				return null;
			}

			// File doesn't exist
			return null;
		}


		public void StartWithOffsetInBytesOrSeconds(long? byteOffset, double? seconds)
		{
 			IsInitialBuffering = true;

			int count = DataSource.BassPlaylistCount;
			if (DataSource.BassPlaylistCurrentIndex >= count) DataSource.BassPlaylistCurrentIndex = count - 1;

			Song currentSong = DataSource.BassPlaylistCurrentSong;
			if (currentSong == null)
				return;


			/*// Keep disabled for now
			if (self.isPlaying && settingsS.crossfadeInterval > 0.)
			{
				// Crossfade into the song
				PrepareNextSongStream(currentSong, true);
				return;
			}*/

			startByteOffset = 0;
			startSecondsOffset = 0;

			Cleanup(); 

			if (currentSong.FileExists())
			{
				BassStream userInfo = PrepareStreamForSong(currentSong);
				if (userInfo != null)
				{
					mixerStream = BassMix.BASS_Mixer_StreamCreate(ISMS_defaultSampleRate, 2, BASSFlag.BASS_STREAM_DECODE);
					BassMix.BASS_Mixer_StreamAddChannel(mixerStream, userInfo.MyStream, 0);
					outStream = Bass.BASS_StreamCreate(ISMS_defaultSampleRate, 2, 0, new STREAMPROC(Procs.StreamProc), IntPtr.Zero);

					ringBuffer.TotalBytesDrained = 0;

					// Make sure BASS is started so music plays
					Bass.BASS_Start();

					/*self.visualizer.channel = self.outStream;

					self.equalizer.channel = self.outStream;

					// Enable the equalizer if it's turned on
					if (settingsS.isEqualizerOn)
					{
						BassEffectDAO *effectDAO = [[BassEffectDAO alloc] initWithType:BassEffectType_ParametricEQ];
						[effectDAO selectPresetId:effectDAO.selectedPresetId];
						[self.equalizer applyEqualizerValues];
					}

					// Add gain amplification
					[self.equalizer createVolumeFx];*/

					// Add the stream to the queue
					lock(streamQueue)
					{
						streamQueue.Add(userInfo);
						tempStreamQueue.Remove(userInfo);
					}

					// Skip to the byte offset
					if (byteOffset != null)
					{
						startByteOffset = (int)byteOffset;
						ringBuffer.TotalBytesDrained = (long)byteOffset;

						if (seconds != null)
						{
							SeekToPositionInSeconds((double)seconds);
						}
						else
						{
							if (startByteOffset > 0)
								SeekToPositionInBytes(startByteOffset);
						}
					}
					else if (seconds != null)
					{
						startSecondsOffset = (int)seconds;
						if (startSecondsOffset > 0.0)
							SeekToPositionInSeconds(startSecondsOffset);
					}

					// Start filling the ring buffer
					KeepRingBufferFilled();

					if (logger.IsDebugEnabled) logger.Debug(@"[BassGaplessPlayer] start song called, created new ring buffer thread: {0}", ringBufferFillThread);

					// Start playback
					Bass.BASS_ChannelPlay(outStream, false);
					IsPlaying = true;

					if (FirstStreamStarted != null)
					{
						FirstStreamStarted(this, new PlayerEventArgs(userInfo.MySong));
					}

					// Prepare the next song
					PrepareNextSongStream();

					IsInitialBuffering = false;

					// Notify listeners that playback has started
					if (SongPlaybackStarted != null)
					{
						SongPlaybackStarted(this, new PlayerEventArgs(userInfo.MySong));
					}

					//currentSong.playedDate = [NSDate date];

					// If we're on a phone call, pause the audio
					/*if ([[UIDevice currentDevice] isOnPhoneCall])
					{
						[self pause];
					}*/
				}
				else if (userInfo != null && !currentSong.IsFullyCached() && currentSong.LocalFileSize() < ISMS_BassStreamMinFilesizeToFail)
				{
					if (DataSource.BassIsOfflineMode)
					{
						MoveToNextSong();
					}
					else if (!currentSong.FileExists())
					{
						logger.Error("Stream for song {0} failed, file is not on disk", currentSong);
						// File was removed, most likely because the decryption failed, so start again normally
						if (SongFailedToPlay != null)
						{
							SongFailedToPlay(this, new PlayerEventArgs(currentSong));
						}
					}
					else
					{
						// Failed to create the stream, retrying
						logger.Error("------failed to create stream, retrying in 2 seconds------");

						if (startSongRetryTimer != null)
						{
							startSongRetryTimer.Stop();
						}

						startSongRetryTimer = new System.Timers.Timer();
						startSongRetryTimer.Elapsed += new ElapsedEventHandler((Object sender, ElapsedEventArgs e) => {
							StartWithOffsetInBytesOrSeconds(byteOffset, seconds);
							startSongRetryTimer.Stop();
						});
						startSongRetryTimer.Interval = 2000.0;
						startSongRetryTimer.Start();
					}
				}
				else
				{
					logger.Error("Could not prepare stream for song {0}, so failing", currentSong);

					// Song failed to play so inform the delegate to handle necessary actions
					if (SongFailedToPlay != null)
					{
						SongFailedToPlay(this, new PlayerEventArgs(currentSong));
					}
				}
			}
			else
			{
				logger.Error(@"Could not prepare stream for song {0}, because the file does not exist, so failing", currentSong);

				// Song failed to play so inform the delegate to handle necessary actions
				if (SongFailedToPlay != null)
				{
					SongFailedToPlay(this, new PlayerEventArgs(currentSong));
				}
			}
		}

		private void RemoveNextStreams()
		{
			lock(streamQueue)
			{
				int count = streamQueue.Count;
				while (count > 1)
				{
					BassStream userInfo = streamQueue[streamQueue.Count - 1];
					if (userInfo.IsPluggedIntoMixer())
						BassMix.BASS_Mixer_ChannelRemove(userInfo.MyStream);
					Bass.BASS_StreamFree(userInfo.MyStream);
					streamQueue.RemoveAt(streamQueue.Count - 1);
					streamDictionary.Remove(userInfo.Identifier.ToString());
					count = streamQueue.Count;
				}
			}
		}

		private void PrepareNextSongStream()
		{
			PrepareNextSongStream(null, false);
		}

		private void PrepareNextSongStream(Song nextSong, bool isCrossfadeImmediately)
		{
			// Remove any additional streams
			bool isNextStreamAlreadyPluggedIn = false;
			lock(streamQueue)
			{
				//NSLog(@"streamQueue: %@", self.streamQueue);
				int count = streamQueue.Count;
				while (count > 1)
				{
					BassStream userInfo = streamQueue[streamQueue.Count - 1];
					isNextStreamAlreadyPluggedIn = isNextStreamAlreadyPluggedIn || userInfo.IsPluggedIntoMixer();
					Bass.BASS_StreamFree(userInfo.MyStream);
					streamQueue.RemoveAt(streamQueue.Count - 1);
					streamDictionary.Remove(userInfo.Identifier.ToString());
					count = streamQueue.Count; 
				}
			}

			Song theSong = nextSong != null ? nextSong : DataSource.BassPlaylistNextSong;

			if (theSong != null)
				if (logger.IsDebugEnabled) logger.Debug(@"nextSong.localFileSize: {0}", theSong.LocalFileSize());
			if (theSong == null || theSong.LocalFileSize() == 0)
				return;

			if (logger.IsDebugEnabled) logger.Debug(@"preparing next song stream for {0}  file: {1}", theSong, theSong.CurrentPath());

			bool success = false;
			if (theSong.FileExists())
			{
				// Check if we're offline and the song is not downloaded or not fully cached
				if (DataSource.BassIsOfflineMode && (!theSong.IsDownloaded() || !theSong.IsFullyCached()))
					return;

				BassStream userInfo = PrepareStreamForSong(theSong);
				if (userInfo != null)
				{
					if (logger.IsDebugEnabled) logger.Debug(@"nextSong: {0}\n   ", userInfo.MyStream);

					lock(streamQueue)
					{
						streamQueue.Add(userInfo);
						tempStreamQueue.Remove(userInfo);
					}
					success = true;

					// If the next stream was already plugged in, we're going to need to plug this one in, becuase we freed it
					if (isNextStreamAlreadyPluggedIn)
					{
						BassMix.BASS_Mixer_StreamAddChannel(mixerStream, userInfo.MyStream, 0);
					}
				}
			}

			if (success && isCrossfadeImmediately)
			{
				IsInitialBuffering = false;

				//NSLog(@"streamQueue2: %@", self.streamQueue);
				BassStream currentStream = CurrentStream;
				currentStream.ShouldIncrementIndex = false;
				//NSLog(@"currentStream: %@", currentStream);

				currentStream.PointerHandle = GCHandle.Alloc(currentStream, GCHandleType.Pinned);

				Procs.StreamCrossfadeCallback(0, currentStream.MyStream, 0, currentStream.PointerHandle.AddrOfPinnedObject());

				// Set a new end sync to happen after the fade
				Bass.BASS_ChannelSetSync(currentStream.MyStream, BASSSync.BASS_SYNC_SLIDE, 0, new SYNCPROC(Procs.StreamEndCallback), currentStream.PointerHandle.AddrOfPinnedObject());
			}
			else if (!success)
			{
				logger.Error("nextSong stream error: {0} - {1}", (int)Bass.BASS_ErrorGetCode(), bassWrapper.StringFromErrorCode(Bass.BASS_ErrorGetCode()));

				// If the stream is currently stuck in the wait loop for partial precaching
				// tell the stream manager to download a few more seconds of data
				//[streamManagerS downloadMoreOfPrecacheStream];

				if (nextSongRetryTimer != null)
				{
					nextSongRetryTimer.Stop();
				}

				nextSongRetryTimer = new System.Timers.Timer();
				nextSongRetryTimer.Elapsed += new ElapsedEventHandler((Object sender, ElapsedEventArgs e) => {
					PrepareNextSongStream(theSong, isCrossfadeImmediately);
					nextSongRetryTimer.Stop();
				});
				nextSongRetryTimer.Interval = 2000.0;
				nextSongRetryTimer.Start();
			}
		}

		//#pragma mark - Audio Engine Properties
		public bool IsStarted
		{
			get
			{
				return CurrentStream.MyStream != 0;
			}
		}

		public long CurrentByteOffset
		{
			get
			{
				return Bass.BASS_StreamGetFilePosition(CurrentStream.MyStream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT) + startByteOffset;
			}
		}

		public double Progress
		{
			get
			{
				if (CurrentStream == null)
					return 0;

				int chanCount = CurrentStream.ChannelCount;
				double sampleRateRatio = CurrentStream.SampleRate / (double)ISMS_defaultSampleRate;
				double seconds = Bass.BASS_ChannelBytes2Seconds(CurrentStream.MyStream, (long)(ringBuffer.TotalBytesDrained * sampleRateRatio * chanCount));

				if (seconds < 0)
				{
					return (double)previousSongForProgress.Duration + seconds;
				}

				return seconds + startSecondsOffset;
			}

		}

		public BassStream CurrentStream 
		{
			get 
			{
				lock (streamQueue)
				{
					if (streamQueue.Count > 0)
					{
						return streamQueue[0];
					}
					return null;
				}
			}
		}

		public int BitRate
		{
			get
			{
				return new BassWrapper().EstimateBitrate(CurrentStream);
			}
		}

		//#pragma mark - Playback methods

		public void Start()
		{
			StartWithOffsetInBytesOrSeconds(0, null);
		}

		public void Stop()
		{
			if (BassStopped != null)
			{
				BassStopped(this, new PlayerEventArgs(null));
			}
			
			if (IsPlaying)
			{
				Bass.BASS_Pause();
				IsPlaying = false;

				if (SongPlaybackEnded != null)
				{
					SongPlaybackEnded(this, new PlayerEventArgs(null));
				}
			}

			Cleanup();
		}

		public void Pause()
		{
			if (IsPlaying) PlayPause();
		}

		public void PlayPause()
		{	
			if (IsPlaying) 
			{
				Bass.BASS_Pause();
				IsPlaying = false;
				if (SongPlaybackPaused != null)
				{
					SongPlaybackPaused(this, new PlayerEventArgs(CurrentStream.MySong));
				}
			} 
			else 
			{
				if (CurrentStream == null)
				{
					int count = DataSource.BassPlaylistCount;
					if (DataSource.BassPlaylistCurrentIndex >= count) 
					{
						// The playlist finished
						DataSource.BassPlaylistCurrentIndex = count - 1;
						startByteOffset = 0;
						startSecondsOffset = 0;
					}
					AudioEngine.StartSongAtOffsetInBytesOrSeconds(startByteOffset, startSecondsOffset, false);
				}
				else
				{
					if (!ringBufferFillThread.IsAlive)
					{
						// Start the ring buffer filling thread if it's not running
						KeepRingBufferFilled();
					}

					Bass.BASS_Start();
					IsPlaying = true;
					if (SongPlaybackStarted != null)
					{
						SongPlaybackStarted(this, new PlayerEventArgs(CurrentStream.MySong));
					}
				}
			}

			//[AudioEngine updateLockScreenInfo];
		}

		public void SeekToPositionInBytes(long bytes)
		{
			BassStream userInfo = CurrentStream;
			if (userInfo == null)
				return;

			if (SeekToPositionStarted != null)
			{
				SeekToPositionStarted(this, new PlayerEventArgs(userInfo.MySong));
			}

			if (userInfo.IsEnded)
			{
				userInfo.IsEnded = false;
				Cleanup();
				StartWithOffsetInBytesOrSeconds(bytes, null);
			}
			else
			{
				if (BassMix.BASS_Mixer_ChannelSetPosition(userInfo.MyStream, bytes, BASSMode.BASS_POS_BYTES))
				{
					startByteOffset = bytes;

					userInfo.NeededSize = long.MaxValue;
					if (userInfo.IsWaiting)
					{
						userInfo.ShouldBreakWaitLoop = true;
					}

					ringBuffer.Reset();

					ringBuffer.TotalBytesDrained = (long)(bytes / CurrentStream.ChannelCount / (CurrentStream.SampleRate / (double)ISMS_defaultSampleRate));

					// Handle seeking during a crossfade
					if (userInfo.IsCrossfadeStarted)
					{
						// Reset the current stream attributes
						userInfo.IsCrossfadeStarted = false;
						Bass.BASS_ChannelSlideAttribute(userInfo.MyStream, BASSAttribute.BASS_ATTRIB_VOL, 1.0f, 0);

						// Retreive the next song userInfo object
						BassStream nextUserInfo = null;
						lock(streamQueue)
						{
							nextUserInfo = streamQueue[1];
						}

						if (nextUserInfo != null)
						{
							// We've plugged this stream in already, so restart it and unplug it
							BassMix.BASS_Mixer_ChannelRemove(nextUserInfo.MyStream);
							Bass.BASS_ChannelSetPosition(nextUserInfo.MyStream, 0, BASSMode.BASS_POS_BYTES);
							Bass.BASS_ChannelSetAttribute(nextUserInfo.MyStream, BASSAttribute.BASS_ATTRIB_VOL, 1.0f);
						}
					}

					if (SeekToPositionSuccess != null)
					{
						SeekToPositionSuccess(this, new PlayerEventArgs(userInfo.MySong));
					}
				}
				else
				{
					new BassWrapper().LogError();
				}
			}
		}

		public void SeekToPositionInSeconds(double seconds)
		{
			int stream = CurrentStream == null ? 0 : CurrentStream.MyStream;
			long bytes = Bass.BASS_ChannelSeconds2Bytes(stream, seconds);
			SeekToPositionInBytes(bytes);
		}
		
		private void CancelBuffering()
		{
			// Pause the audio engine
			Pause();

			// Unset the initial buffering flag
			IsInitialBuffering = false;
		}

		public void RaiseSongFailedToPlayEvent(Song song)
		{
			if (SongFailedToPlay != null)
			{
				SongFailedToPlay(this, new PlayerEventArgs(song));
			}
		}
	}
}

