using ATXLicense;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architexor.Core
{
	public static class Utils
	{
		public static string GetDBPath()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string dbPath = Path.Combine(appDataPath, "IntelliBIM", "IntelliBIM.db");
			// Ensure the directory exists
			Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

			return dbPath;
		}

        public static void ParseLicenseResponse(string response)
        {
            //LicenseStatus _fromServerStatus = LicenseStatus.UNDEFINED;
            try
            {
                JArray jSubscriptions = JArray.Parse(response);
                for (int i = 0; i < jSubscriptions.Count; i++)
                {
                    JObject jSubscription = (JObject)jSubscriptions[i];

                    JObject jUser = JObject.Parse(jSubscription.GetValue("user").ToString());
                    Constants.thisUser.Id = jUser.GetValue("id").ToString();
                    //Constants.thisUser.Token = jUser.GetValue("token").ToString();
                    Constants.thisUser.FirstName = jUser.GetValue("fullname").ToString();
                    Constants.thisUser.LastName = "";
                    Constants.thisUser.Email = jUser.GetValue("email").ToString();

                    string startDate = jSubscription.GetValue("start_date").ToString();
                    string endDate= jSubscription.GetValue("end_date").ToString();

                    DateTime expireAt = new DateTime();
                    if(endDate != "")
                    {
                        DateTime.TryParse(endDate, out expireAt);
                    }

                    if (startDate == "")
                    {
                        //_fromServerStatus = LicenseStatus.INVALID;
                    }
                    else if ( endDate != "" && expireAt != null && expireAt < DateTime.Now)
                    {
                        //_fromServerStatus = LicenseStatus.EXPIRED;
                    }
                    else
                    {
                        //_fromServerStatus = LicenseStatus.VALID;

                        Constants.thisUser.Licenses = new List<License>();
                        License _lic = new License();
                        /*_lic = (License)LicenseHandler.ParseLicenseFromBASE64String(typeof(License), sRes, _certPublicKeyData, out _fromServerStatus, out _msg);
                        if (_lic.EndDate < DateTime.Now)
                        {
                            _fromServerStatus = LicenseStatus.EXPIRED;
                        }
                        else
                        {
                            _fromServerStatus = LicenseStatus.VALID;
                        }*/
                        _lic.EndDate = expireAt;
                        Constants.thisUser.Licenses.Add(_lic);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //sRes = ex.Message;
            }
        }
    }
}
