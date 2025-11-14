using System.Collections.ObjectModel;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;

namespace ClashSolver.Forms.Controllers
{
	public class ExLicenseUIController : LicenseUIController
	{
		#region Fields

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		#endregion

		#region Properties

		public ClashSolverRequestHandler Handler { get => m_Handler; }
		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		#endregion

		#region Constructors

		public ExLicenseUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);
			
			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			LicenseController ins = m_Handler.Instance as LicenseController;

			ins.Initialize();

			WakeUp();
		}

		#endregion

		#region IExternal implementation
		public override void MakeRequest(int request)
		{
			LastRequestId = (ClashSolverRequestId)request;

			m_Handler.Request.Make(LastRequestId);
			m_ExEvent.Raise();
		}

		public override void WakeUp(bool bFinish = false)
		{
		}
		
		public override int GetRequestId()
		{
			if (m_Handler == null)
			{
				return (int)ClashSolverRequestId.None;
			}
			return (int)m_Handler.RequestId;
		}
		#endregion

		#region Controller implementation
		public override void OnOK()
		{
			// Validate SurName and Email Address
			if (string.IsNullOrEmpty(SurName) || string.IsNullOrEmpty(EmailAddress))
			{
				string message = "Please enter both your surname and email address.";
				MessageBox.Show(message);
				return;
			}

			if(!IsValidEmail(EmailAddress))
			{
				string message = "Please enter a valid email address.";
				MessageBox.Show(message);
				return;
			}

			base.OnOK();
		}

		private bool IsValidEmail(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return false;
			}

			string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
		}

		public override void RequestSent()
		{
			//TaskDialog.Show("Success", "Request was sent successfully.\n");
			string message = "Request was sent successfully.\n";
			MessageBox.Show(message);
		}
		#endregion
	}
}
