using Architexor.Core;
using ClashSolver.UI.TableAdapters;
using ForgeAPI.Helpers;
using ForgeAPI.Models;
using System;
using System.Threading.Tasks;

namespace ClashSolver.UI.Controllers
{
	public class ConfigurationUIController : BaseUIController
	{
		#region Fields

		private string _clientId = "";
		private string _clientSecret = "";
		private string _port = "";
		private string _redirectURI = "";
		private string _statusText = "Not yet Login";
		private bool _isLogin = false;
		private string _buttonText = "Login";
		private bool _isLoading = false;

		#endregion

		#region Properties

		public string ClientId
		{
			get { return _clientId; }
			set
			{
				_clientId = value;
				OnPropertyChanged(nameof(ClientId));
			}
		}
		public string ClientSecret
		{
			get { return _clientSecret; }
			set
			{
				_clientSecret = value;
				OnPropertyChanged(nameof(ClientSecret));
			}
		}
		public string Port
		{
			get { return _port; }
			set
			{
				_port = value;
				OnPropertyChanged(nameof(Port));
			}
		}
		public string RedirectURI
		{
			get { return _redirectURI; }
			set
			{
				_redirectURI = value;
				OnPropertyChanged(nameof(RedirectURI));
			}
		}

		public string StatusText
		{
			get { return _statusText; }
			set
			{
				_statusText = value;
				OnPropertyChanged(nameof(StatusText));
			}
		}

		public bool IsLoggedin
		{
			get { return _isLogin; }
			set
			{
				_isLogin = value;
				OnPropertyChanged(nameof(IsLoggedin));
				OnPropertyChanged(nameof(IsEnabled));
			}
		}
		public bool IsEnabled
		{
			get { return !_isLogin; }
		}

		public string ButtonText
		{
			get { return _buttonText; }
			set
			{
				_buttonText = value;
				OnPropertyChanged(nameof(ButtonText));
			}
		}

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				_isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		public Auth Auth { get; set; }

		#endregion

		#region Constructors

		public ConfigurationUIController()
		{
			var auth = ACCUserTableAdapter.Instance.GetLoginUser() as Auth;

			if(auth != null)
			{
				StatusText = "Login Succeeded";
				ButtonText = "Logout";
				IsLoggedin = true;
				Auth = auth;
			}
		}

		#endregion

		#region Methods

		public virtual async Task<bool> OnLogin()
		{
			bool res = true;

			try
			{
				if (!IsLoggedin)
				{
					IsLoading = true;

					var token = await APSHelper.Instance.GetAccessTokenAsync(_clientId, _clientSecret);

					IsLoading = false;

					if (token == null)
					{
						res = false;
						StatusText = "Login Failed";
					}
					else
					{
						StatusText = "Login Succeeded";
						ButtonText = "Logout";
						IsLoggedin = true;

						var auth = ACCUserTableAdapter.Instance.GetByClientId(ClientId) as Auth;

						if (auth == null)
						{
							Auth = new Auth()
							{
								ClientId = ClientId,
								ClientSecret = ClientSecret,
								AccessToken = token.AccessToken,
								ExpiresAt = token.ExpiresAt,
								IsLogin = true
							};

							ACCUserTableAdapter.Instance.Insert(Auth);
						}
						else
						{
							auth.AccessToken = token.AccessToken;
							auth.ExpiresAt = token.ExpiresAt;
							auth.IsLogin = true;

							Auth = auth;

							long nRes = ACCUserTableAdapter.Instance.Update(Auth);

							if(nRes < 0)
							{
								res = false;
							}
						}
					}
				}
				
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("ForgeAPI Login => ", ex);
				res = false;
			}

			return res;
		}

		public virtual void OnLogout()
		{
			IsLoggedin = false;
			StatusText = "Not yet Login";
			ButtonText = "Login";

			Auth.AccessToken = "";
			Auth.IsLogin = false;

			ACCUserTableAdapter.Instance.Update(Auth);
		}

		#endregion
	}
}
