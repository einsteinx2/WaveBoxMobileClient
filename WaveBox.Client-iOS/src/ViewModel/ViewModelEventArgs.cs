using System;

namespace WaveBox.Client.ViewModel
{
	public delegate void ViewModelEventHandler(object sender, ViewModelEventArgs e);

	public class ViewModelEventArgs : EventArgs
	{
		public ViewModelEventArgs() 
		{
		}
	}
}

