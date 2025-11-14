using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using ClashSolver.Utils;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.Controllers
{
	public class ClashSettingsController : Controller
	{
		#region Fields

		private List<LinkedModel> _linkModels = [];
		private List<long> _usedCategories = [];

		#endregion

		#region Properties

		public List<LinkedModel> LinkModels { get => _linkModels; set => _linkModels = value; }

		public List<long> UsedCategories { get => _usedCategories; set => _usedCategories = value; }

		#endregion

		#region Initialization

		public override bool Initialize()
		{
			_usedCategories.Clear();

			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			_linkModels = RevitHelper.GetLinkedProjectsFromDB(Application.thisApp.Project.Id);

			UsedCategories = RevitHelper.GetUsedCategoryElementIds(doc);

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
				case ClashSolverRequestId.ClashSettings:
					break;
				default:
					break;
			}

			return bFinish;
		}

		#endregion
	}
}
