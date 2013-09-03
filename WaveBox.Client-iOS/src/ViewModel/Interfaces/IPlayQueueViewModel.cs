using System;
using System.Collections.Generic;
using WaveBox.Core.Model;

namespace WaveBox.Client.ViewModel
{
	public interface IPlayQueueViewModel
	{
		event ViewModelEventHandler DataChanged;

		IList<IMediaItem> MediaItems { get; }
		uint CurrentIndex { get; }
		IMediaItem CurrentItem { get; }

		void PlayPauseToggle();
		void PlayItemAtIndex(int index);
	}
}

