using System;
using WaveBox.Core.Model;

namespace WaveBox.Client.AudioEngine
{
	public delegate void PlayerEventHandler(object sender, PlayerEventArgs e);

	public class PlayerEventArgs : EventArgs
	{
		public Song AffectedSong { get; set; }

		public PlayerEventArgs(Song affectedSong) 
		{
			this.AffectedSong = affectedSong;
		}
	}
}

