using System.Collections.Generic;
using Autodesk.Revit.UI;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;

namespace ClashSolver.Forms.Controllers
{
	public class ExComplianceReportUIController : ComplianceReportUIController
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

		public ExComplianceReportUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			// Get Report Contents From Database
			//foreach (var obj in ReportContentTableAdapter.Instance.GetByType())
			//{
			//	if (obj is ReportContent reportContent)
			//	{
			//		ReportContents.Add(new ReportContent()
			//		{
			//			Id = reportContent.Id,
			//			Name = reportContent.Name
			//		});
			//	}
			//}

			Initialize();

			WakeUp();
		}

		private void Initialize()
		{

			List<string> complianceContentNames = ["Total Clashes Detected", "Clash-Free Areas", "Resolved Clashes", "Remaining Clashes", "Compliance Rate", "High-Risk Areas", "Total Cost of Resolution", "Cost by Clash Type", "Cost by Severity", "Cost per Discipline", "Budget Compliance", "Risk Assessment", "Impacke on Construction", "Clash Heatmap", "Compliance Trends", "Coordination Compliance"];

			foreach (var contentName in complianceContentNames)
			{
				ReportContent reportContent = new ReportContent()
				{
					Name = contentName,
					Type = "Compliance"
				};

				ReportContents.Add(reportContent);

				//ReportContentTableAdapter.Instance.Insert(reportContent);
			}
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

		#region Event Handlers
		public override void OnOK()
		{
			
		}
		#endregion
	}
}
	
