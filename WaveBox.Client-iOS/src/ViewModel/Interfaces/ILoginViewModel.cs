using System;

namespace WaveBox.Client.ViewModel
{
	public interface ILoginViewModel
	{
		event ViewModelEventHandler StateChanged;

		string UrlString { get; set; }
		string Username { get; set; }
		string Password { get; set; }

		void Login();
	}
}

