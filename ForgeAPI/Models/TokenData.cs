using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAPI.Models
{
	public class TokenData
	{
		public string AccessToken { get; private set; }

		public DateTime ExpiresAt { get; private set; }

		public TokenData(string accessToken, DateTime expiresAt) 
		{
			AccessToken = accessToken;
			ExpiresAt = expiresAt;
		}
	}
}
