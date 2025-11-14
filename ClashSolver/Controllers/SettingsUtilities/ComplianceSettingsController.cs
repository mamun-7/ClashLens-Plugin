using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using ClashSolver.Utils;

namespace ClashSolver.Controllers
{
	public class ComplianceSettingsController : Controller
	{
		#region Fields

		private List<LinkedModel> _models = new List<LinkedModel>();
		public List<LinkedModel> Models
		{
			get => _models;
			set => _models = value;
		}

		#endregion

		#region Initialization

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();
			
			_models = RevitHelper.GetLinkedProjects(doc);

			return true;
		}

		#endregion

		#region Request Handler

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.ComplianceSettings:
					break;
				default:
					break;
			}

			return bFinish;
		}

		#endregion

	}
}
