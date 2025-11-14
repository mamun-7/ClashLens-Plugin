#define IsUpdateCategory
#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashSolver.Request;
using ClashSolver.UI.Models;
using ClashSolver.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Models;
using System.Windows.Controls;
using System.Windows.Forms;
using ClashSolver.UI;


#endregion

namespace ClashSolver.Controllers
{
	public class QuickDetectionController : Controller
	{
		private List<LinkedModel> _models = new List<LinkedModel>();
		private List<Issue> _issues = new List<Issue>();
		private List<TargetElement> _targetElements = new List<TargetElement>();

		public List<LinkedModel> Models
		{
			get => _models;
			set => _models = value;
		}

		public List<Issue> Issues
		{
			get => _issues;
			set => _issues = value;
		}

		public List<TargetElement> TargetElements
		{
			get => _targetElements;
			set => _targetElements = value;
		}

		public BoundingBoxXYZ SectionBox { get; set; }

		public override bool Initialize()
		{
			bool bRes = true;
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			var linkedModels = RevitHelper.GetLinkedProjectsFromDB(Application.thisApp.Project.Id);

			if (linkedModels.Where(x => x.Discipline == LinkDiscipline.None).Count() > 0)
			{
				TaskDialog.Show("Warning", "You should set the discipline of linked models correctly.");

				bRes = false;
			}

			foreach(var item in linkedModels)
			{
				_models.Add(new LinkedModel()
				{
					No = item.No,
					ElementId = item.ElementId,
					Name = item.Name,
					Url = item.Url,
					Discipline = item.Discipline,
					InstanceId = item.InstanceId,

					IsSelected = true
				});
			}

			// Get the current 3D view
			View3D view3D = doc.ActiveView as View3D;

			if (view3D == null)
			{
				TaskDialog.Show("Error", "Please run this command in a 3D view.");

				return false;
			}

			//Check if the Section Box is enabled
			if (!view3D.IsSectionBoxActive)
			{
				TaskDialog.Show("Error", "The Section Box is not active in this view.");

				bRes = false;
			}

			// Get the Section Box
			SectionBox = view3D.GetSectionBox();

			if (SectionBox == null)
			{
				TaskDialog.Show("Error", "Could not retrieve the Section Box");
				bRes = false; 
			}

			return bRes;
		}


		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch (reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.QuickDetection:
					break;
				case ClashSolverRequestId.ReviewIssues:
					break;
				case ClashSolverRequestId.UpdateIssues:
					Application.thisApp.DoRequest(m_uiApp, reqId);
					break;
				default:
					break;
			}

			return bFinish;
		}

		public void GetElementsInSectionBox()
		{
			Document doc = GetDocument();

			//var hostElements = RevitHelper.GetHostElementsInSectionBox(doc, SectionBox);

			List<long> linkInstanceIdsForAnalyzing = _models.Where(x => x.IsSelected).Select(x => x.InstanceId).ToList();

			_targetElements = RevitHelper.GetElementsInBoundingBox(doc, SectionBox, linkInstanceIdsForAnalyzing);
		}

	}

}
