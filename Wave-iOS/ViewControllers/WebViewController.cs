using System;
using MonoTouch.UIKit;
using WaveBox.Client;
using MonoTouch.Foundation;

namespace Wave.iOS.ViewControllers
{
	public partial class WebViewController : UIViewController, IWebViewController
	{
		private UIWebView webView;

		private readonly IClientSettings clientSettings;

		public WebViewController(IClientSettings clientSettings)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("clientSettings");

			this.clientSettings = clientSettings;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Create the web view
			webView = new UIWebView(View.Bounds);
			View.Add(webView);
			webView.LoadRequest(new NSUrlRequest(new NSUrl(clientSettings.ServerUrl)));
		}
	}
}

