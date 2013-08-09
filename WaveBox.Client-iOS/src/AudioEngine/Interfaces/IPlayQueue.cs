using System;
using WaveBox.Core.Model;
using System.Collections.Generic;

namespace WaveBox.Client.AudioEngine
{
	public enum RepeatMode
	{
		None,
		One,
		All
	}
	
	public interface IPlayQueue
	{
		event PlayQueueEventHandler IndexChanged;
		event PlayQueueEventHandler ShuffleToggled;
		event PlayQueueEventHandler OrderChanged;

		IList<IMediaItem> PlayQueueList { get; }
		IList<IMediaItem> ShufflePlayQueueList { get; }
		IList<IMediaItem> CurrentPlayQueueList { get; }

		bool IsShuffle { get; set; }
		RepeatMode RepeatMode { get; set; }

		void SavePlayQueue();
		void SaveShufflePlayQueue();
		void SaveCurrentPlayQueue();
		void LoadPlayQueues();

		void ResetPlayQueue();
		void ResetShufflePlayQueue();
		void ResetBothPlayQueues();

		void RemoveItemsAtIndexes(IList<uint> indexes);
		void RemoveItemAtIndex(uint index);
		IMediaItem ItemForIndex(uint index);
		int IndexForItem(IMediaItem item);
		IMediaItem CurrentItem { get; }
		IMediaItem NextItem { get; }
		uint Index { get; set; }
		uint ShuffleIndex { get; set; }
		uint CurrentIndex { get; set; }
		uint NextIndex { get; }
		uint PrevIndex { get; }
		uint IndexForOffsetFromCurrentIndex(int offset);
		uint Count { get; }
		uint Duration { get; }
		uint decrementIndex();
		uint IncrementIndex();
		bool IsAnySongCached { get; }
		void ShuffleToggle(bool keepCurrentPlayingSong);
		void MoveSong(uint fromIndex, uint toIndex);
		void AddItems(IList<IMediaItem> items);
		void AddItem(IMediaItem item);
		void AddItemsNext(IList<IMediaItem> items);
		void AddItemNext(IMediaItem item);
		void FillDownloadQueue();
	}
}

