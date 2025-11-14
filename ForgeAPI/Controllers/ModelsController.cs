using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForgeAPI.Models;
using Autodesk.Oss;
using Autodesk.Oss.Model;
using ForgeAPI.Helpers;
using System.IO;

namespace ForgeAPI.Controllers
{
	public class ModelsController
	{
		private readonly Auth _auth;

		public ModelsController(Auth auth)
		{
			_auth = auth;
		}
	}
}
