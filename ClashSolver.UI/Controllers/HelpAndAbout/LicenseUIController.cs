using Architexor.Core;
using ATXLicense;
using ClashSolver.UI.Models;
using Newtonsoft.Json.Linq;
using QLicense;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ClashSolver.UI.Controllers
{
	public class LicenseUIController : BaseUIController
	{
		#region Fields

		private string _surName = "";

		private string _emailAddress = "";

		private string _deviceId = "";

		private bool _isLicense = false;

		private bool _isLoading = false;

		private DateTime _endDate = new DateTime();

		private byte[] _certPublicKeyData;

		#endregion

		#region Properties

		public string SurName
		{
			get => _surName;
			set
			{
				_surName = value;
				OnPropertyChanged(nameof(SurName));
			}
		}

		public string EmailAddress
		{
			get => _emailAddress;
			set
			{
				_emailAddress = value;
				OnPropertyChanged(nameof(EmailAddress));
			}
		}

		public string DeviceId
		{
			get => _deviceId;
			set
			{
				_deviceId = value;
				OnPropertyChanged(nameof(DeviceId));
			}
		}

		public bool IsLicense
		{
			get => _isLicense;
			set
			{
				_isLicense = value;
				OnPropertyChanged(nameof(IsLicense));
			}
		}

		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		public DateTime EndDate
		{
			get => _endDate;
			set
			{
				_endDate = value;
				OnPropertyChanged(nameof(EndDateStr));
			}
		}

		public string EndDateStr
		{
			get
			{
				string expirationDate = _endDate.ToString("yyyy/M/d");
				return $"Your expiration date is {expirationDate}.";
			}
		}
		#endregion

		#region Constructors
		//	Test Constructor for UITest
		public LicenseUIController()
		{
			License _lic = new License();
			DeviceId = LicenseHandler.GenerateUID(_lic.AppName);

			//	Check License
			string _msg = string.Empty;

			//	Read public key from assembly
			string url = Assembly.GetExecutingAssembly().Location;
			url = url.Substring(0, url.LastIndexOf("\\")) + "\\" + "Architexor.Core.dll";
			Assembly _assembly = Assembly.LoadFrom(url);
			using (MemoryStream _mem = new())
			{
				_assembly.GetManifestResourceStream("Architexor.Core.LicenseVerify.cer").CopyTo(_mem);

				_certPublicKeyData = _mem.ToArray();
			}

			UpdateUI();

			//ApiService.PostAsync("https://www.architexor.com/core/license_check", "{\"deviceId\":\"" + _deviceId + "\"}").ContinueWith(task =>
			//ApiService.GetAsync(Constants.API_ENDPOINT + "core/subscription/license_check?deviceId=" + _deviceId).ContinueWith(task =>
			//{
			//	if (task.Exception == null)
			//	{
			//		Architexor.Core.Utils.ParseLicenseResponse(task.Result);
			//	}
			//	else
			//	{
			//		//	Exception
			//		//task.Exception.InnerException?.Message;
			//	}

			//	UpdateUI();
			//});
		}
		#endregion

		#region Event Handlers

		public override void OnOK()
		{
			IsLoading = true;
			//ApiService.PostAsync("https://www.architexor.com/api/core/license_request", "{\"deviceId\":\"" + _deviceId + "\", \"fullname\":\"" + _surName + "\", \"email\":\"" + _emailAddress + "\"}").ContinueWith(task =>
			ApiService.GetAsync(Constants.API_ENDPOINT + "core/subscription/license_request?deviceId=" + _deviceId + "&fullname=" + _surName + "&email=" + _emailAddress).ContinueWith(task =>
			{

				if (task.Exception == null)
				{
					Architexor.Core.Utils.ParseLicenseResponse(task.Result);

					IsLoading = false;
					RequestSent();
				}
				else
				{
				}

				UpdateUI();
			});
		}

		public virtual void RequestSent()
		{
			
		}
		#endregion

		private void UpdateUI()
		{
			//Constants.thisUser.Licenses.Find(x => x.StartDate == "") != null
			IsLicense = Constants.thisUser.IsLicensed;
			DateTime endDate = new DateTime();
			var licenses = Constants.thisUser.Licenses;
			if(licenses != null && licenses.Count > 0)
			{
				foreach (License lic in licenses)
				{
					if (lic != null && endDate < lic.EndDate)
					{
						endDate = lic.EndDate;
					}
				}

				EndDate = endDate;
			}

			SurName = Constants.thisUser.FirstName + " " + Constants.thisUser.LastName;
			EmailAddress = Constants.thisUser.Email;
		}
	}
}
