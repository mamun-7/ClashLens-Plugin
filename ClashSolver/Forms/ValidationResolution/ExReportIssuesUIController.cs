using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Architexor.Core;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Utils;

namespace ClashSolver.Forms.ValidationResolution
{
	public class ExReportIssuesUIController : ReportIssuesUIController
	{
		#region Fields

		protected ClashSolverRequestHandler m_Handler;
		protected ExternalEvent m_ExEvent;

		#endregion

		#region Properties

		public ClashSolverRequestHandler Handler { get => m_Handler; }
		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		#endregion

		#region Constructors

		public ExReportIssuesUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);
			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);
			Initialize();

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

			WakeUp();
		}

		private void Initialize()
		{

			//	Initialize Data Context
			IssueReportController ins = m_Handler.Instance as IssueReportController;

			Project = Application.thisApp.Project;

			//List<string> reportContentNames = ["Summary", "Clash Point", "Date Found", "Assigned To", "Date Approved", "Approved By", "Layer Name", "Item Path", "Item ID", "Status", "Distance", "Description", "Comments", "Quick Properties", "Image", "Simulation Dates", "Simulation Event", "Clash Groups", "Grid Location"];

			List<string> reportContentNames = ["Id", "ElementA", "ElementB", "Status", "Severity", "Description", "AnalyzedAt"];

			//foreach (var contentName in reportContentNames)
			//{
			//	SelectableItem reportContent = new SelectableItem()
			//	{
			//		IsSelected = true,
			//		Name = contentName
			//	};

			//	ReportContents.Add(reportContent);

			//	//ReportContentTableAdapter.Instance.Insert(reportContent);
			//}

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
			try
			{
				switch(SelectedReportFormat)
				{
					case ReportFormat.Text:
						ExportToText();
						break;
					case ReportFormat.Xlsx:
						ExportToExcel();
						break;
					default:
						break;
				}
			}
			catch(Exception ex)
			{
				TraceLogger.Instance.ExceptionLog($"ExReportIsueUIController::OnOK => ", ex);
			}
		}

		private void ExportToText()
		{
			IssueReportController ins = m_Handler.Instance as IssueReportController;

			Project = ProjectTableAdapter.Instance.GetByUniqueId(RevitHelper.GetProjectId(ins.GetDocument())) as Project;

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Issue Reports");
			sb.AppendLine($"Project Name: {Project.Name}");
			sb.AppendLine($"Date: {DateTime.Now.ToString("yyyy-MM-dd")}");

			// Get issues from database
			foreach (var obj in IssueTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is Issue issue)
				{
					sb.AppendLine($"Issue Number: {issue.Id}");
					sb.AppendLine($"ElementA: {issue.CategoryA.Name}({issue.ElementIdA}), ElementB: {issue.CategoryB.Name}({issue.ElementB})");
					sb.AppendLine($"Resolve: {issue.ResolveStatus}");
				}
			}

			string filePath = "Report.txt";
		}

		private void ExportToExcel()
		{
			// Open Save File Dialog
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Excel Files (*.xlsx)|*.xlsx",
				Title = "Save Excel File",
				DefaultExt = "xlsx",
				FileName = $"Clash_Report_{DateTime.Now.ToString("yyyy-MM-dd")}"
			};

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = saveFileDialog.FileName;

				List<string> headers = ReportContents.Where(x => x.IsSelected).Select(x => x.Name).ToList();

				var temp = Statuses.Where(x => x.IsSelected).Select(x => Statuses.IndexOf(x) + 1).ToArray();

				string statusStr = string.Join(",", temp);

				List<Issue> issues = new List<Issue>();
				foreach (var obj in IssueTableAdapter.Instance.GetByStatus(Project.Id, statusStr))
				{
					if (obj != null && obj is Issue issue)
					{
						issues.Add(issue);
					}
				}

				ExcelAdapter.ExportToExcel(headers, issues, filePath);

				Process.Start(filePath);
			}
		}

		#endregion
	}
}
	
