using System;
using MonoTouch.UIKit;
using JASidePanels;
using WaveBox.Client.ViewModel;
using System.Drawing;
using WaveBox.Client;
using System.Collections.Generic;

namespace Wave.iOS.ViewController
{
	public class LoginViewController : UIViewController
	{
		public UITextField UrlField { get; set; }
		public UITextField Username { get; set; }
		public UITextField Password { get; set; }
		public UIButton LoginButton { get; set; }

		public UIActivityIndicatorView LoadingSpinner { get; set; }

		private readonly UIWindow window;
		private readonly IPlayQueueViewModel playQueueViewModel;
		private readonly ILoginViewModel loginViewModel;
		private IDictionary<string, object> styleDictionary;

		public LoginViewController(UIWindow window, IPlayQueueViewModel playQueueViewModel, ILoginViewModel loginViewModel)//, IDictionary<string, object> styleDictionary)
		{
			if (window == null)
				throw new ArgumentNullException("window");
			if (playQueueViewModel == null)
				throw new ArgumentNullException("playQueueViewModel");
			if (loginViewModel == null)
				throw new ArgumentNullException("loginViewModel");
			if (styleDictionary == null)
				this.styleDictionary = new Dictionary<string, object>();
//				throw new ArgumentNullException("styleDictionary");

			this.window = window;
			this.playQueueViewModel = playQueueViewModel;
			this.loginViewModel = loginViewModel;
			this.styleDictionary = styleDictionary;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.FromRGB(233, 233, 233);

			UrlField = new UITextField(new RectangleF(40f, 100f, 240f, 30f));
			UrlField.BackgroundColor = UIColor.FromRGB(207, 207, 207);
			UrlField.AutocapitalizationType = UITextAutocapitalizationType.None;
			UrlField.Placeholder = "Url";
			View.Add(UrlField);

			Username = new UITextField(new RectangleF(40f, 140f, 240f, 30f));
			Username.BackgroundColor = UIColor.FromRGB(207, 207, 207);
			Username.AutocapitalizationType = UITextAutocapitalizationType.None;
			Username.Placeholder = "Username";
			View.Add(Username);

			Password = new UITextField(new RectangleF(40f, 180f, 240f, 30f));
			Password.BackgroundColor = UIColor.FromRGB(207, 207, 207);
			Password.SecureTextEntry = true;
			Password.AutocapitalizationType = UITextAutocapitalizationType.None;
			Password.Placeholder = "Password";
			View.Add(Password);

			LoginButton = new UIButton(UIButtonType.RoundedRect);
			LoginButton.Frame = new RectangleF(150f, 220f, 100f, 44f);
			LoginButton.SetTitle("Login", UIControlState.Normal);
			LoginButton.TouchUpInside += delegate(object sender, EventArgs e) {
				Login();
			};
			View.Add(LoginButton);
		}
	
		private void Login()
		{
			LoginButton.Enabled = false;
			LoadingSpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
			LoadingSpinner.Center = new PointF(LoginButton.Frame.Width / 2f, LoginButton.Frame.Height / 2f);
			LoginButton.Add(LoadingSpinner);
			LoadingSpinner.StartAnimating();

			loginViewModel.UrlString = UrlField.Text;
			loginViewModel.Username = Username.Text;
			loginViewModel.Password = Password.Text;

			loginViewModel.StateChanged += delegate(object sender, ViewModelEventArgs e) {
				InvokeOnMainThread(delegate {
					if (e.Success)
					{
						WBSidePanelController sidePanelController = new WBSidePanelController();
						sidePanelController.PanningLimitedToTopViewController = false;
						sidePanelController.LeftPanel = new MenuViewController(sidePanelController);//, styleDictionary);
						sidePanelController.RightPanel = new PlayQueueViewController(playQueueViewModel);
						window.RootViewController = sidePanelController;

						// TODO: rewrite this (Forces menu to load so the center is populated)
						Console.WriteLine("Menu view: " + sidePanelController.LeftPanel.View);
					}
					else
					{
						UIAlertView alert = new UIAlertView("Error logging in", e.Error, null, "OK");
						alert.Show();

						LoadingSpinner.RemoveFromSuperview();
						LoginButton.Enabled = true;
					}
				});
			};

			loginViewModel.Login();
		}
	}
}

