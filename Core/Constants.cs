using ATXLicense;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Architexor.Core
{
	public struct ATXUser
	{
		public string Id;
		public string FirstName;
		public string LastName;
		public string Email;
		public string Token;
		public string DeviceId;

		public List<License> Licenses;

		public bool IsLicensed 
		{
			get
			{
				return Licenses != null && Licenses.Count > 0;
            }
		}
	}

	public static class Constants
	{
		public const string BRAND = "IntelliBIM";
		public const string CONTACT_PRODUCT_MANAGER = "chebanovandrii@gmail.com";
		public const string CONTACT_DEVELOPER = "chebanovandrii@gmail.com";
		public const string FRONTEND = "http://193.203.202.249";
		public const string BACKEND = "http://193.203.202.249:81";
		public const string API_ENDPOINT = "http://193.203.202.249/";
		public const string AI_ENGINE_ENDPOINT = "http://193.203.202.249:82/";
		public const int RevitVersion = 2024;
		public const string FontFamily = "Calibri";
		public static ATXUser thisUser = new ATXUser();

		// IntelliBIM
		public const string MARKER_FAMILY_NAME = "Clash Marker 3D";
		public const string COPY_MODEL_SUCCESS = "Successfully copied the model from the link.";
		public const string COPY_MODEL_ERROR = "An error occurred while copying the model from the link.";

		// Messages
		public const string SUCCESS = "Success";
		public const string ERROR = "Error";
		public const string WARNING = "Warning";

		public const int ISSUE_COUNTER_PER_PAGE = 25;

		// Error Messages
		public const string DATABASE_ERROR = "There is an error in database. Please restart the application.";
		public const string REQUIRE_SAVE = "Please save your project.";
		public const string FAIL_ADD_PROJECT = "Unable to insert a project to a database.";
		public const string FAIL_ADD_LINKEDMODEL = "Unable to insert a linked model to a database.";
		public const string INVALID_DOCUMENT = "This tool is not available in family documents.";

		public const string INVALID_LICENSE = "License verification failed. Please check your license or contact support to obtain a valid one.";
	}
}
