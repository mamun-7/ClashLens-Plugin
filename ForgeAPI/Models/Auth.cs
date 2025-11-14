using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAPI.Models
{
	public class Auth
	{
		private string _clientId = "";
		private string _clientSecret = "";
		private string _accessToken = "";
		private DateTime _expiresIn;

		public string ClientId
		{
			get => _clientId;
			set => _clientId = value;
		}

		public string ClientSecret
		{
			get => _clientSecret;
			set => _clientSecret = value;
		}

		public string AccessToken
		{
			get => _accessToken;
			set => _accessToken = value;
		}

		public DateTime ExpiresAt
		{
			get => _expiresIn;
			set => _expiresIn = value;
		}

		public bool IsLogin { get; set; }	

		public long Id { get; set; }
	}
}
