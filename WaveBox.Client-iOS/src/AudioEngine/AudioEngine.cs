using System;
using System.Timers;
using WaveBox.Core.Model;

namespace WaveBox.Client.AudioEngine
{
	public class AudioEngine : IAudioEngine
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IBassGaplessPlayer player;
		private readonly IPlayQueue playQueue;
		private readonly IBassWrapper bassWrapper;
		private readonly IClientSettings clientSettings;
		//private bool shouldResumeFromInterruption;

		public AudioEngine(IBassGaplessPlayer player, IPlayQueue playQueue, IBassWrapper bassWrapper, IClientSettings clientSettings)
		{
			if (player == null)
				throw new ArgumentNullException("player");
			if (playQueue == null)
				throw new ArgumentNullException("playQueue");
			if (bassWrapper == null)
				throw new ArgumentNullException("bassWrapper");
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			player.AudioEngine = (IAudioEngine)this;
			this.player = player;
			this.playQueue = playQueue;
			this.bassWrapper = bassWrapper;
			this.clientSettings = clientSettings;
		}

		/*+ (void)setup
		{
			AudioSessionInitialize(NULL, NULL, interruptionListenerCallback, NULL);

			// Add the callbacks for headphone removal and other audio takeover
			AudioSessionAddPropertyListener(kAudioSessionProperty_AudioRouteChange, audioRouteChangeListenerCallback, NULL);

			// AudioEngineingleton
			[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(updateLockScreenInfo) name:Notification_SongPlaybackStarted object:nil];
			[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(updateLockScreenInfo) name:Notification_CurrentPlaylistIndexChanged object:nil];
		}

		void interruptionListenerCallback(void *inUserData, UInt32 interruptionState)
		{
			if (interruptionState == kAudioSessionBeginInterruption) 
			{
				DDLogCVerbose(@"[AudioEngine] audio session begin interruption");
				if (_player.isPlaying)
				{
					_shouldResumeFromInterruption = YES;
					[_player pause];
				}
				else
				{
					_shouldResumeFromInterruption = NO;
				}
			} 
			else if (interruptionState == kAudioSessionEndInterruption) 
			{
				DDLogCVerbose(@"[AudioEngine] audio session interruption ended, isPlaying: %@   isMainThread: %@", NSStringFromBOOL(_player.isPlaying), NSStringFromBOOL([NSThread isMainThread]));
				if (_shouldResumeFromInterruption)
				{
					[_player playPause];

					// Reset the shouldResumeFromInterruption value
					_shouldResumeFromInterruption = NO;
				}
			}
		}

		void audioRouteChangeListenerCallback(void *inUserData, AudioSessionPropertyID inPropertyID, UInt32 inPropertyValueSize, const void *inPropertyValue) 
		{			
			DDLogCInfo(@"[AudioEngine] audioRouteChangeListenerCallback called, propertyId: %lu  isMainThread: %@", inPropertyID, NSStringFromBOOL([NSThread isMainThread]));

			// ensure that this callback was invoked for a route change
			if (inPropertyID != kAudioSessionProperty_AudioRouteChange) 
				return;

			if (_player.isPlaying)
			{
				// Determines the reason for the route change, to ensure that it is not
				// because of a category change.
				CFDictionaryRef routeChangeDictionary = inPropertyValue;
				CFNumberRef routeChangeReasonRef = CFDictionaryGetValue (routeChangeDictionary, CFSTR (kAudioSession_AudioRouteChangeKey_Reason));
				SInt32 routeChangeReason;
				CFNumberGetValue (routeChangeReasonRef, kCFNumberSInt32Type, &routeChangeReason);

				DDLogCInfo(@"[AudioEngine] route change reason: %li", routeChangeReason);

				// "Old device unavailable" indicates that a headset was unplugged, or that the
				// device was removed from a dock connector that supports audio output. This is
				// the recommended test for when to pause audio.
				if (routeChangeReason == kAudioSessionRouteChangeReason_OldDeviceUnavailable) 
				{
					[_player playPause];

					DDLogCInfo(@"[AudioEngine] Output device removed, so application audio was paused.");
				}
				else 
				{
					DDLogCInfo(@"[AudioEngine] A route change occurred that does not require pausing of application audio.");
				}
			}
			else 
			{	
				DDLogCInfo(@"[AudioEngine] Audio route change while application audio is stopped.");
				return;
			}
		}*/

		public void StartWithOffsetInBytesOrSeconds(long? byteOffset, double? seconds)
		{
			player.Stop();
			player.StartWithOffsetInBytesOrSeconds(byteOffset, seconds);
		}

		public void Start()
		{
			StartWithOffsetInBytesOrSeconds(0, null);
		}

		public void Stop()
		{
			player.Stop();

			// Stop any loading spinners
			//[NSNotificationCenter postNotificationToMainThreadWithName:Notification_BassPlayerStoppedBuffering];
		}

		Timer startSongDelayTimer = null;
		public void StartSongAtOffsetInBytesOrSeconds(long? bytes, double? seconds, bool delay)
		{
			Song currentSong = playQueue.CurrentItem as Song;
			if (currentSong == null)
				return;

			if (startSongDelayTimer != null)
			{
				startSongDelayTimer.Stop();
			}

			if (delay)
			{
				// Only start the caching process if it's been a half second after the last request
				// Prevents crash when skipping through playlist fast
				if (startSongDelayTimer != null)
				{
					startSongDelayTimer.Stop();
				}
				if (startSongDelayTimer == null)
				{
					startSongDelayTimer = new Timer();
				}
				startSongDelayTimer.Elapsed += new ElapsedEventHandler((Object sender, ElapsedEventArgs e) => {
					StartSongAtOffsetInBytesOrSecondsInternal(bytes, seconds);
					startSongDelayTimer.Stop();
				});
				startSongDelayTimer.Interval = 600.0;
				startSongDelayTimer.Start();
			}
			else
			{
				StartSongAtOffsetInBytesOrSecondsInternal(bytes, seconds);
			}
		}

		// TODO: put this method somewhere and name it properly
		public void StartSongAtOffsetInBytesOrSecondsInternal(long? bytes, double? seconds)
		{
			// Stop the player
			player.Stop();

			// Let the player controller know to start showing the loading spinner
			//[NSNotificationCenter postNotificationToMainThreadWithName:Notification_BassPlayerStartedBuffering];
			player.IsInitialBuffering = true;

			// If we're loading a song to start, then initialize BASS now or it won't start in the background
			bassWrapper.BassInit();

			// Always clear the temp cache
			//[CacheManager clearTempCache];

			/*Song *currentSong = playlistS.CurrentSong;

			//DDLogVerbose(@"[AudioEngineSingleton] startSongWithOffset for song %@", currentSong);

			// Check to see if the song is already cached
			if (currentSong.IsFullyCached())
			{
				//DDLogVerbose(@"[AudioEngineSingleton] song is fully cached %@", currentSong);

				// The song is fully cached, start streaming from the local copy
				StartWithOffsetInBytesOrSeconds(bytes, seconds);

				// Fill the stream queue
				//if (!clientSettings.IsOfflineMode)
				//	[streamManagerS fillStreamQueue:YES];
			}
			else if (!currentSong.IsFullyCached() && clientSettings.IsOfflineMode)
			{
				// Make sure we have downloaded songs in this playlist
				if (CacheManager.numberOfCachedSongs > 0)
				{
					if ([playlistS isAnySongDownloaded])
					{
						DDLogVerbose(@"[AudioEngineingleton] song is NOT fully cached and it's offline mode %@", currentSong);
						[self nextSong];
					}
					else
					{
						[AudioEngine stop];
					}
				}
				else
				{
					[AudioEngine stop];
				}
			}
			else
			{
				DDLogVerbose(@"[AudioEngineingleton] song is NOT fully cached and it's online mode %@", currentSong);
				if ([streamManagerS isSongDownloading:currentSong])
				{
					DDLogVerbose(@"[AudioEngineingleton] song is being downloaded by stream manager %@", currentSong);
					// The song is streaming, start streaming from the local copy
					StreamHandler *handler = [streamManagerS handlerForSong:currentSong];
					if (!AudioEngine.player.isPlaying && handler.isDelegateNotifiedToStartPlayback)
					{
						// Only start the player if the handler isn't going to do it itself
						[AudioEngine startWithOffsetInBytes:bytesAndSeconds[BytesKey] orSeconds:bytesAndSeconds[SecondsKey]];
					}
				}
				else if ([cacheQueueManagerS.currentQueuedSong isEqualToSong:currentSong])
				{
					DDLogVerbose(@"[AudioEngineingleton] song is being downloaded by cache queue manager %@", currentSong);
					// The song is caching, start streaming from the local copy
					StreamHandler *handler = [cacheQueueManagerS currentStreamHandler];
					if (!AudioEngine.player.isPlaying && handler.isDelegateNotifiedToStartPlayback)
					{
						// Only start the player if the handler isn't going to do it itself
						[AudioEngine startWithOffsetInBytes:bytesAndSeconds[BytesKey] orSeconds:bytesAndSeconds[SecondsKey]];
					}
				}
				else if ([streamManagerS isSongFirstInQueue:currentSong] && ![streamManagerS isQueueDownloading] && isResumeAllowed)
				{
					DDLogVerbose(@"[AudioEngineingleton] song is first in stream manager but is not downloading and resume is allowed %@", currentSong);
					// The song is first in queue, but the queue is not downloading. Probably the song was downloading
					// when the app quit. Resume the download and start the player
					[streamManagerS resumeQueue];

					// The song is caching, start streaming from the local copy
					StreamHandler *handler = [streamManagerS handlerForSong:currentSong];
					if (!AudioEngine.player.isPlaying && handler.isDelegateNotifiedToStartPlayback)
					{
						// Only start the player if the handler isn't going to do it itself
						[AudioEngine startWithOffsetInBytes:bytesAndSeconds[BytesKey] orSeconds:bytesAndSeconds[SecondsKey]];
					}
				}
				else if ([streamManagerS isSongFirstInQueue:currentSong] && ![streamManagerS isQueueDownloading] && !isResumeAllowed)
				{
					DDLogVerbose(@"[AudioEngineingleton] song is first in stream manager but is not downloading and resume is NOT allowed %@", currentSong);
					// Resume isn't allowed, so start the download over
					StreamHandler *handler = [streamManagerS handlerForSong:currentSong];
					[streamManagerS startHandler:handler resume:NO];
				}
				else
				{
					DDLogVerbose(@"[AudioEngineingleton] reached else, clearing stream manager and starting download for %@", currentSong);
					// Clear the stream manager
					[streamManagerS removeAllStreams];

					// Disabled resuming
					//BOOL isTempCache = NO;
					//if ([bytesAndSeconds[BytesKey] integerValue] > 0)
					//	isTempCache = YES;
					//else if (!settingsS.isSongCachingEnabled)
					//	isTempCache = YES;

					// Start downloading the current song from the correct offset
					[streamManagerS queueStreamForSong:currentSong
					 byteOffset:0//[bytesAndSeconds[BytesKey] unsignedLongLongValue]
					 secondsOffset:0//[bytesAndSeconds[SecondsKey] doubleValue]
					 atIndex:0
					 isTempCache:NO//isTempCache
					 isStartDownload:YES];

					// Fill the stream queue
					if (settingsS.isSongCachingEnabled)
						[streamManagerS fillStreamQueue:AudioEngine.player.isStarted];
				}
			}*/
		}

		public void StartSong()
		{
			StartSongAtOffsetInBytesOrSeconds(0, 0, false);
		}

		public void PlaySongAtPosition(uint position)
		{
			PlaySongAtPosition(position, false);
		}

		public void PlaySongAtPosition(uint position, bool delay)
		{
			//DLog(@"before stop called, progress: %f   duration: %f", AudioEngine.player.progress, AudioEngine.player.currentStream.song.duration.floatValue);

			if (playQueue.CurrentIndex == position)
			{
				// Send out the playlist index changed notification, otherwise it won't go out
				//[NSNotificationCenter postNotificationToMainThreadWithName:Notification_CurrentPlaylistIndexChanged];
			}

			playQueue.CurrentIndex = position;

			playQueue.RepeatMode = RepeatMode.None;

			//[streamManagerS removeAllStreamsExceptForSong:playlistS.currentSong];

			StartSongAtOffsetInBytesOrSeconds(0, 0, delay);
		}

		public void PrevSong()
		{
			if (player.Progress > 10.0)
			{
				// Past 10 seconds in the song, so restart playback instead of changing songs
				PlaySongAtPosition(playQueue.CurrentIndex);
			}
			else
			{
				playQueue.RepeatMode = RepeatMode.None;

				// Within first 10 seconds, go to previous song
				PlaySongAtPosition(playQueue.PrevIndex, true);
			}
		}

		public void NextSong()
		{
			playQueue.RepeatMode = RepeatMode.None;

			PlaySongAtPosition(playQueue.NextIndex, true);
		}

		public void ResumeSong()
		{
			if (logger.IsDebugEnabled) logger.Debug("isRecover: %@  currentItem: %@", clientSettings.IsRecover, playQueue.CurrentItem);

			if (playQueue.CurrentItem != null && clientSettings.IsRecover)
			{
				StartSongAtOffsetInBytesOrSeconds(clientSettings.RecoverByteOffset, clientSettings.RecoverSeekTime, false);
			}
			else
			{

			}
		}
	}
}

