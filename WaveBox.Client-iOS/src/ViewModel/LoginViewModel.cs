using System;
using WaveBox.Client.ServerInteraction;
using MonoTouch.Foundation;

namespace WaveBox.Client.ViewModel
{
	public class LoginViewModel : ILoginViewModel
	{
		public event ViewModelEventHandler StateChanged;

		public string UrlString { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		private readonly IClientSettings clientSettings;
		private readonly IClientDatabase clientDatabase;

		private readonly ILoginLoader loginLoader;
		private readonly IDatabaseSyncLoader databaseLoader;

		public LoginViewModel(IClientSettings clientSettings, IClientDatabase clientDatabase, ILoginLoader loginLoader, IDatabaseSyncLoader databaseLoader)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");
			if (clientDatabase == null)
				throw new ArgumentNullException("clientDatabase");
			if (loginLoader == null)
				throw new ArgumentNullException("loginLoader");
			if (databaseLoader == null)
				throw new ArgumentNullException("databaseLoader");

			this.clientSettings = clientSettings;
			this.clientDatabase = clientDatabase;
			this.loginLoader = loginLoader;
			this.databaseLoader = databaseLoader;
		}

		public void Login()
		{
			clientSettings.ServerUrl = UrlString;
			clientSettings.UserName = Username;
			clientSettings.Password = Password;

			loginLoader.LoginCompleted += delegate(object sender, LoginEventArgs e) 
			{
				if (e.Error == null)
				{
					clientSettings.SessionId = e.SessionId;

					databaseLoader.DatabaseDownloaded += delegate(object sender2, DatabaseSyncEventArgs e2) 
					{
						// Swap out the database
						clientDatabase.ReplaceDatabaseWithDownloaded();

						// Success so save the settings
						clientSettings.SaveSettings();

						if (StateChanged != null)
						{
							StateChanged(this, new ViewModelEventArgs(true, null));
						}
					};
					databaseLoader.DownloadDatabase();
				}
				else
				{
					if (StateChanged != null)
					{
						StateChanged(this, new ViewModelEventArgs(false, e.Error));
					}
				}
			};
			loginLoader.Login();
		}
	}
}

