using System;
using System.Collections.Generic;
using System.Linq;
using Architexor.Core;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Models;
using ClashSolver.Request;
using ClashSolver.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Utils;

namespace ClashSolver.Forms.Controllers
{
	public class ExQuickDetectionUIController : QuickDetectionUIController
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

		public ExQuickDetectionUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			QuickDetectionController ins = m_Handler.Instance as QuickDetectionController;
			
			IsValid = ins.Initialize();

			if (IsValid)
			{
				ins.GetElementsInSectionBox();

				Project = Application.thisApp.Project;

				LinkedModels = [.. ins.Models];
			}

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

		public override string GetProjectId()
		{
			QuickDetectionController ins = m_Handler.Instance as QuickDetectionController;

			return ins.GetProjectId();
		}

		public override void UpdateIssues()
		{
			MakeRequest((int)ClashSolverRequestId.UpdateIssues);
		}

		public override int GetTotalCount()
		{
			QuickDetectionController ins = m_Handler.Instance as QuickDetectionController;
			Document doc = ins.GetDocument();

			List<TargetElement> temp	= new List<TargetElement>();
			var linkModelIds = LinkedModels.Where(x => x.IsSelected).Select(x => x.InstanceId).ToList();

			foreach (var item in ins.TargetElements)
			{
				if (linkModelIds.Contains(item.LinkModelId.Value))
				{
					temp.Add(item);
				}
			}

			ins.TargetElements = temp;

			return ins.TargetElements.Count;
		}

		public override List<Issue> FindClash(int index)
		{
			List<Issue> res = new List<Issue>();

			QuickDetectionController ins = m_Handler.Instance as QuickDetectionController;
			Document doc = ins.GetDocument();
			var models = ins.Models;
			double tolerance = 1e-3;

			var targetElement = ins.TargetElements[index];

			if (targetElement.IsLinkedElement)
			{
				var linkInstanceA = doc.GetElement(targetElement.LinkModelId) as RevitLinkInstance;
				var linkDocA = linkInstanceA.GetLinkDocument();
				var transformA = linkInstanceA.GetTransform();
				var elementA = linkDocA.GetElement(targetElement.Id);

				var solidA = RevitHelper.GetSolidFromElement(elementA);
				if (solidA == null) return null;

				var outlineA = RevitHelper.GetOutlineFromElement(elementA);
				if(outlineA == null) return null;

				// Transform solidA to the host model's coordinate system
				var transformedSolidA = SolidUtils.CreateTransformed(solidA, transformA);
				if (transformedSolidA == null) return null;

				foreach (var item in ins.TargetElements)
				{
					if (item.Id == targetElement.Id) continue;

					var linkInstanceB = doc.GetElement(item.LinkModelId) as RevitLinkInstance;
					var linkDocB = linkInstanceB.GetLinkDocument();
					var elementB = linkDocB.GetElement(item.Id);

					var outlineB = RevitHelper.GetOutlineFromElement(elementB);

					// Quick bounding box check
					if (outlineB == null || !outlineA.Intersects(outlineB, tolerance)) continue;

					var transformB = linkInstanceB.GetTransform();
					var solidB = RevitHelper.GetSolidFromElement(elementB);
					if (solidB == null) continue;

					var transformedSolidB = SolidUtils.CreateTransformed(solidB, transformB);
					if (transformedSolidB == null) continue;

					try
					{
						// Check for intersection
						Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(transformedSolidA, transformedSolidB, BooleanOperationsType.Intersect);

						if (intersection != null && intersection.Volume > 0)
						{
							long projectId = Application.thisApp.Project.Id;
							var linkedModelA = LinkModelTableAdapter.Instance.GetByInstanceId(projectId, linkInstanceA.Id.Value) as LinkedModel;
							var linkedModelB = LinkModelTableAdapter.Instance.GetByInstanceId(projectId, linkInstanceB.Id.Value) as LinkedModel;

							string disciplineA = linkedModelA.Discipline != LinkDiscipline.None ? linkedModelA.Discipline.ToString() : "";
							string disciplineB = linkedModelB.Discipline != LinkDiscipline.None ? linkedModelB.Discipline.ToString() : "";

							Issue issue = new Issue()
							{
								ProjectId = Project.Id,
#if REVIT2024 || REVIT2025
								ElementIdA = targetElement.Id.Value,
								ElementIdB = item.Id.Value,
#else
							ElementIdA = targeElement.Id.IntegerValue
							ElementIdB = item.Id.IntegerValue
#endif
								ElementA = $"{disciplineA} {elementA.Category.Name}",
								ElementB = $"{disciplineB} {elementB.Category.Name}",

								LinkModelA = linkedModelA ,
								LinkModelB = linkedModelB,
								Severity = "High",
								CategoryA = CategoryTableAdapter.Instance.GetByElementId(elementA.Category.Id.Value) as CSCategory,
								CategoryB = CategoryTableAdapter.Instance.GetByElementId(elementB.Category.Id.Value) as CSCategory,
								Description = $"{elementA.Category.Name} (ElementId : {elementA.Id.Value}) in {linkedModelA.Name} conflicts with {elementB.Category.Name}(ElementId: {elementB.Id.Value}) in {linkedModelB.Name}"
							};

							res.Add(issue);


						}
					}
					catch (Exception ex)
					{
						TraceLogger.Instance.ExceptionLog("ExQuickDetectionUIController::FindClash => ", ex);
						continue;
					}
					
				}
			}

			return res;
		}

		#endregion
	}
}
