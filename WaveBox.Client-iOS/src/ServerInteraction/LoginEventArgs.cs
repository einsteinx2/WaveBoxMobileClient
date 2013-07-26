using System;

namespace WaveBox.Client.ServerInteraction
{
	public delegate void LoginEventHandler(object sender, LoginEventArgs e);

	public class LoginEventArgs : EventArgs
	{
		public string Error;
		public string SessionId;

		public LoginEventArgs(string error, string sessionId)
		{
			this.Error = error;
			this.SessionId = sessionId;
		}
	}
}

