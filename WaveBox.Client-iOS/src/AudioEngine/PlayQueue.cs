using System;
using WaveBox.Core.Model;
using System.Collections.Generic;
using WaveBox.Core.Extensions;
using WaveBox.Client.ServerInteraction;
using System.Linq;

namespace WaveBox.Client.AudioEngine
{
	public class PlayQueue : IPlayQueue
	{
		public event PlayQueueEventHandler IndexChanged;
		public event PlayQueueEventHandler ShuffleToggled;
		public event PlayQueueEventHandler OrderChanged;

		public IList<IMediaItem> PlayQueueList { get; private set; }
		public IList<IMediaItem> ShufflePlayQueueList { get; private set; }
		public IList<IMediaItem> CurrentPlayQueueList { get { return IsShuffle ? ShufflePlayQueueList : PlayQueueList; } }

		public bool IsShuffle { get; set; }

		private IDownloadQueue downloadQueue;
		private IClientSettings clientSettings;

		public PlayQueue(IDownloadQueue downloadQueue, IClientSettings clientSettings)
		{
			if (downloadQueue == null)
				throw new ArgumentNullException("downloadQueue");
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.downloadQueue = downloadQueue;
			this.clientSettings = clientSettings;

			// Load the play queues from disk
			LoadPlayQueues();
		}

		private RepeatMode repeatMode;
		public RepeatMode RepeatMode 
		{ 
			get
			{
				lock (PlayQueueList)
				{
					return repeatMode;
				}
			}

			set
			{
				lock (PlayQueueList)
				{
					repeatMode = value;
				}

				FillDownloadQueue();
			}
		}

		public void SavePlayQueue()
		{
		}

		public void SaveShufflePlayQueue()
		{
		}

		public void SaveCurrentPlayQueue()
		{

		}

		public void LoadPlayQueues()
		{
			PlayQueueList = new List<IMediaItem>();
			ShufflePlayQueueList = new List<IMediaItem>();
		}

		public void ResetPlayQueue()
		{
			lock (PlayQueueList)
			{
				PlayQueueList.Clear();
			}
		}

		public void ResetShufflePlayQueue()
		{
			lock (ShufflePlayQueueList)
			{
				ShufflePlayQueueList.Clear();
			}
		}

		public void ResetBothPlayQueues()
		{
			ResetPlayQueue();
			ResetShufflePlayQueue();
		}

		public void RemoveItemsAtIndexes(IList<uint> indexes)
		{
			lock (PlayQueueList)
			{		
				// Sort the indexes to make sure they're descending
				indexes = new List<uint>(from i in indexes orderby i descending select i);

				if (indexes.Count == Count)
				{
					ResetBothPlayQueues();
					IsShuffle = false;
				}
				else
				{
					foreach (uint index in indexes)
					{
						CurrentPlayQueueList.RemoveAt((int)index);
					}
				}
				SavePlayQueue();
				SaveShufflePlayQueue();

				// Find out how many songs were deleted before the current position to determine the new position
				uint numberBefore = 0;
				foreach (int index in indexes)
				{
					if (index <= CurrentIndex)
					{
						numberBefore += 1;
					}
				}
				CurrentIndex -= numberBefore;
				if (CurrentIndex < 0)
					CurrentIndex = 0;

				if (indexes.Contains(CurrentIndex))
				{
					IncrementIndex();
				}
			}

			FillDownloadQueue();
		}

		public void RemoveItemAtIndex(uint index)
		{
			RemoveItemsAtIndexes(new List<uint>() { index });
		}

		public IMediaItem ItemForIndex(uint index)
		{
			lock (PlayQueueList)
			{
				return index < Count ? CurrentPlayQueueList[(int)index] : null;
			}
		}

		public int IndexForItem(IMediaItem item)
		{
			if (item == null)
				return -1;

			lock (PlayQueueList)
			{
				return CurrentPlayQueueList.IndexOf(item);
			}
		}

		public IMediaItem CurrentItem
		{
			get
			{
				lock (PlayQueueList)
				{
					return CurrentIndex < Count ? CurrentPlayQueueList[(int)CurrentIndex] : null;
				}
			}
		}

		public IMediaItem NextItem
		{
			get
			{
				lock (PlayQueueList)
				{
					return NextIndex < Count ? CurrentPlayQueueList[(int)NextIndex] : null;
				}
			}
		}

		private uint index;
		public uint Index 
		{ 
			get
			{
				lock (PlayQueueList)
				{
					return index;
				}
			}

			set
			{
				lock (PlayQueueList)
				{
					index = value;
				}
			}
		}

		private uint shuffleIndex;
		public uint ShuffleIndex
		{ 
			get
			{
				lock (PlayQueueList)
				{
					return shuffleIndex;
				}
			}

			set
			{
				lock (PlayQueueList)
				{
					shuffleIndex = value;
				}
			}
		}

		public uint CurrentIndex
		{
			get
			{
				return IsShuffle ? ShuffleIndex : Index;
			}

			set
			{
				if (IsShuffle)
					ShuffleIndex = value;
				else
					Index = value;

				if (IndexChanged != null)
					IndexChanged(this, new PlayQueueEventArgs(null));
			}
		}

		public uint NextIndex
		{
			get
			{
				lock (PlayQueueList)
				{
					switch (RepeatMode) 
					{
						case RepeatMode.None:
							return CurrentIndex + 1;
						case RepeatMode.One:
							return CurrentIndex;
						case RepeatMode.All:
							return ItemForIndex(CurrentIndex + 1) == null ? 0 : CurrentIndex + 1;
						default:
							return 0;
					}
				}
			}
		}

		public uint PrevIndex
		{
			get
			{
				lock (PlayQueueList)
				{
					switch (RepeatMode) 
					{
						case RepeatMode.None:
							return CurrentIndex == 0 ? 0 : CurrentIndex - 1;
						case RepeatMode.One:
							return CurrentIndex;
						case RepeatMode.All:
							return CurrentIndex == 0 ? Count - 1 : CurrentIndex - 1;
						default:
							return 0;
					}
				}
			}
		}

		public uint IndexForOffsetFromCurrentIndex(int offset)
		{
			lock (PlayQueueList)
			{
				int index = (int)CurrentIndex;
				switch (RepeatMode) 
				{
					case RepeatMode.None:
						index += offset;
						break;
					case RepeatMode.One:
						break;
					case RepeatMode.All:	
						for (int i = 0; i < offset; i++)
						{
							index = ItemForIndex((uint)index + 1) == null ? 0 : index + 1;
						}
						break;
				}
				return (uint)index;
			}
		}

		public uint Count
		{ 
			get
			{ 
				lock (PlayQueueList)
				{
					return (uint)CurrentPlayQueueList.Count;
				}
			}
		}

		public uint Duration 
		{ 
			get
			{
				lock (PlayQueueList)
				{
					uint duration = 0;
					foreach (IMediaItem item in CurrentPlayQueueList)
					{
						duration += (uint)item.Duration;
					}
					return duration;
				}
			}
		}

		public uint decrementIndex()
		{
			lock (PlayQueueList)
			{
				CurrentIndex = PrevIndex;
				return CurrentIndex;
			}
		}

		public uint IncrementIndex()
		{
			lock (PlayQueueList)
			{
				CurrentIndex = NextIndex;
			}

			FillDownloadQueue();

			return CurrentIndex;
		}

		public bool IsAnySongCached
		{ 
			get
			{
				return true;
			}
		}

		public void ShuffleToggle(bool keepCurrentPlayingSong)
		{
			IMediaItem currentItem = CurrentItem;
			if (IsShuffle)
			{
				IsShuffle = false;

				// Find the track position in the regular playlist
				lock (PlayQueueList)
				{
					CurrentIndex = (uint)PlayQueueList.IndexOf(currentItem);
				}
			}
			else
			{
				uint currentIndex = CurrentIndex;
				IsShuffle = true;
				ShuffleIndex = 0;

				ResetShufflePlayQueue();

				lock (PlayQueueList)
				{
					int i = 0;
					foreach (IMediaItem mediaItem in PlayQueueList)
					{
						if (i != currentIndex)
						{
							ShufflePlayQueueList.Add(mediaItem);
						}
						i++;
					}

					ShufflePlayQueueList.Shuffle();
					ShufflePlayQueueList.Insert(0, currentItem);
				}
			}  

			FillDownloadQueue();

			if (ShuffleToggled != null)
				ShuffleToggled(this, new PlayQueueEventArgs(null));
		}

		public void MoveSong(uint fromIndex, uint toIndex)
		{
			lock (PlayQueueList)
			{
				if (fromIndex >= Count || toIndex >= Count || fromIndex == toIndex)
					return;

				IMediaItem fromItem = CurrentPlayQueueList[(int)fromIndex];
				CurrentPlayQueueList.RemoveAt((int)fromIndex);
				CurrentPlayQueueList.Insert((int)toIndex, fromItem);

				// Correct the value of currentPlaylistPosition
				if (fromIndex == CurrentIndex)
				{
					CurrentIndex = toIndex;
				}
				else
				{
					if (fromIndex < CurrentIndex && toIndex >= CurrentIndex)
					{
						CurrentIndex -= 1;
					}
					else if (fromIndex > CurrentIndex && toIndex <= CurrentIndex)
					{
						CurrentIndex += 1;
					}
				}
			}

			FillDownloadQueue();

			SaveCurrentPlayQueue();

			if (OrderChanged != null)
				OrderChanged(this, new PlayQueueEventArgs(null));
		}

		public void AddItems(IList<IMediaItem> items)
		{
			lock (PlayQueueList)
			{
				CurrentPlayQueueList.AddRange(items);
				if (IsShuffle)
					ShufflePlayQueueList.AddRange(items);
			}

			FillDownloadQueue();

			SaveCurrentPlayQueue();
			if (IsShuffle)
				SaveShufflePlayQueue();
		}

		public void AddItem(IMediaItem item)
		{
			AddItems(new List<IMediaItem>() { item }); 
		}

		public void AddItemsNext(IList<IMediaItem> items)
		{
			lock (PlayQueueList)
			{
				CurrentPlayQueueList.InsertRange((int)NextIndex, items);

				if (IsShuffle)
					ShufflePlayQueueList.InsertRange((int)NextIndex, items);
			}

			FillDownloadQueue();

			SaveCurrentPlayQueue();
			if (IsShuffle)
				SaveShufflePlayQueue();
		}

		public void AddItemNext(IMediaItem item)
		{
			AddItemsNext(new List<IMediaItem>() { item }); 
		}

		public void FillDownloadQueue()
		{
			uint numberOfItems = clientSettings.StreamQueueLength;
			IList<IMediaItem> itemsToQueue = new List<IMediaItem>();

			lock (PlayQueueList)
			{
				for (int i = 0; i < numberOfItems; i++)
				{
					IMediaItem item = ItemForIndex(IndexForOffsetFromCurrentIndex(i));
					if (item != null && !itemsToQueue.Contains(item))
					{
						itemsToQueue.Add(item);
					}
				}
			}

			downloadQueue.FillQueueWithMediaItems(itemsToQueue);
		}
	}
}

