using System;

namespace WaveBox.Client.ViewModel
{
	public delegate void ViewModelEventHandler(object sender, ViewModelEventArgs e);

	public class ViewModelEventArgs : EventArgs
	{
		public bool Success { get; set; }
		public string Error { get; set; }

		public ViewModelEventArgs(bool success, string error) 
		{
			this.Success = success;
			this.Error = error;
		}

		public ViewModelEventArgs()
		{

		}
	}
}

