using ClashSolver.Controllers;
using ClashSolver.Request;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;

namespace ClashSolver.Request
{
	//	A class with methods to execute requests made by the Dialog
	public class ClashSolverRequestHandler : IExternalEventHandler
	{
		//	The value of the latest request made by the form 
		public ClashSolverRequest Request { get; } = new ClashSolverRequest();

		public ClashSolverRequestId RequestId { get; set; }

		//	Controller Class Instance
		public Controller Instance { get; set; }

		//	A method to identify this External Event Handler
		public string GetName()
		{
			return "ClashSolverRequest";
		}

		public ClashSolverRequestHandler(ClashSolverRequestId reqId, UIApplication uiapp)
		{
			Initialize(reqId, uiapp);
		}

		protected void Initialize(ClashSolverRequestId reqId, UIApplication uiapp)
		{
			RequestId = reqId;
			switch (reqId)
			{
				case ClashSolverRequestId.CopyFromLinks:
					Instance = (Controller)Application.thisApp.GetClassInstance("CopyFromLinksController");
					break;
				case ClashSolverRequestId.QuickDetection:
					Instance = (Controller)Application.thisApp.GetClassInstance("QuickDetectionController");
					break;
				case ClashSolverRequestId.RunValidation:
					Instance = (Controller)Application.thisApp.GetClassInstance("RunValidationController");
					break;
				case ClashSolverRequestId.ReviewIssues:
					Instance = (Controller)Application.thisApp.GetClassInstance("ReviewIssuesController");
					break;
				case ClashSolverRequestId.IssueReport:
					Instance = (Controller)Application.thisApp.GetClassInstance("IssueReportController");
					break;
				case ClashSolverRequestId.ComplianceHealthReport:
					Instance = (Controller)Application.thisApp.GetClassInstance("ComplianceReportController");
					break;
				case ClashSolverRequestId.AIResolve:
					Instance = (Controller)Application.thisApp.GetClassInstance("AIResolveController");
					break;
				case ClashSolverRequestId.ManualResolution:
					Instance = (Controller)Application.thisApp.GetClassInstance("ResolveController");
					break;
				case ClashSolverRequestId.ManageLinks:
					Instance = (Controller)Application.thisApp.GetClassInstance("ManageLinksController");
					break;
				case ClashSolverRequestId.ClashSettings:
					Instance = (Controller)Application.thisApp.GetClassInstance("ClashSettingsController");
					break;
				case ClashSolverRequestId.ComplianceSettings:
					Instance = (Controller)Application.thisApp.GetClassInstance("ComplianceSettingsController");
					break;
				case ClashSolverRequestId.IssueMarkers:
					Instance = (Controller)Application.thisApp.GetClassInstance("IssueMarkersController");
					break;
				case ClashSolverRequestId.CostDatabase:
					Instance = (Controller)Application.thisApp.GetClassInstance("CostDatabaseController");
					break;
				case ClashSolverRequestId.ManageTeam:
					Instance = (Controller)Application.thisApp.GetClassInstance("ManageTeamController");
					break;
				case ClashSolverRequestId.Configuration:
					Instance = (Controller)Application.thisApp.GetClassInstance("ConfigurationController");
					break;
				case ClashSolverRequestId.LinkModel:
					Instance = (Controller)Application.thisApp.GetClassInstance("ConnectModelController");
					break;
				case ClashSolverRequestId.SyncModel:
					Instance = (Controller)Application.thisApp.GetClassInstance("SyncModelController");
					break;
				case ClashSolverRequestId.License:
					Instance = (Controller)Application.thisApp.GetClassInstance("LicenseController");
					break;
				default:
					break;
			}

			Instance.UIApp = uiapp;
		}

		//	The top method of the event handler.
		//	<remarks>
		//		This is called by Revit after the corresponding
		//		external event was raised (by the modeless form)
		//		and Revit reached the time at which it could call
		//		the event's handler (i.e. this object)
		//	</remarks>
		public void Execute(UIApplication uiapp)
		{
			bool bFinish = false;
			try
			{
				if (uiapp.ActiveUIDocument == null)
					return;

				Document doc = uiapp.ActiveUIDocument.Document;

				ClashSolverRequestId reqId = Request.Take();

				bFinish = Instance.ProcessRequest(reqId);
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.Message);
				Debug.WriteLine(ex.StackTrace);
			}
			finally
			{
				Application.thisApp.WakeRequestUp(RequestId, bFinish);
			}
		}
	}
}
