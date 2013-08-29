using System;
using System.Threading.Tasks;
using System.Net.Http;
using WaveBox.Core;
using Newtonsoft.Json;
using System.Text;
using WaveBox.Core.Extensions;

namespace WaveBox.Client.ServerInteraction
{
	public class LoginLoader : ILoginLoader
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event LoginEventHandler LoginCompleted;

		private readonly IClientSettings clientSettings;

		public LoginLoader(IClientSettings clientSettings)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.clientSettings = clientSettings;
		}

		public async Task Login()
		{
			await Login(clientSettings.ServerUrl, clientSettings.UserName, clientSettings.Password);
		}

		public async Task Login(string serverUrl, string userName, string password)
		{
			if (serverUrl == null)
			{
				throw new MissingServerUrlException();
			}

			if (userName == null)
			{
				throw new MissingApiParameterException("u");
			}

			if (password == null)
			{
				throw new MissingApiParameterException("p");
			}

			// Initiate the Api call
			HttpClient client = new HttpClient();
			client.Timeout = new TimeSpan(10 * TimeSpan.TicksPerMillisecond);
			HttpResponseMessage response = null;
			try
			{
				response = await client.GetAsync(clientSettings.ServerUrl + "/api/login?u=" + clientSettings.UserName + "&p=" + clientSettings.Password);
			}
			catch (Exception e)
			{
				logger.Debug("login failed: " + e);
				if (LoginCompleted != null) LoginCompleted(this, new LoginEventArgs("Failed to open connection", null));
			}

			logger.Debug("response status code: " + response.StatusCode);

			// Check that response was successful or throw exception
			try
			{
				response.EnsureSuccessStatusCode();
			}
			catch (Exception e)
			{
				logger.Debug("login failed: " + e);
				if (LoginCompleted != null) LoginCompleted(this, new LoginEventArgs("Status code not success: " + e, null));
			}

			// Parse the response
			string responseString = await response.Content.ReadAsStringAsync();
			responseString = responseString.RemoveByteOrderMark();
			try
			{
				LoginResponse loginResponse = (LoginResponse)JsonConvert.DeserializeObject<LoginResponse>(responseString);

				if (LoginCompleted != null)  LoginCompleted(this, new LoginEventArgs(loginResponse.Error, loginResponse.SessionId));
			}
			catch (Exception e)
			{
				logger.Error("Exception parsing login response: " + e);
				if (LoginCompleted != null)  LoginCompleted(this, new LoginEventArgs("Exception parsing login response: " + e, null));
			}
		}
	}
}

