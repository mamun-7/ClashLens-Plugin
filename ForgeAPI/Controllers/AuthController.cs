using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForgeAPI.Models;

namespace ForgeAPI.Controllers
{
	public class AuthController
	{
		private readonly Auth auth;

		public AuthController(Auth auth)
		{
			this.auth = auth;
		}
	}
}
