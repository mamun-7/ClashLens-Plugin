using System.Collections.Generic;
using System.Linq;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.TableAdapters;
using Autodesk.Revit.DB;
using ClashSolver.Utils;
using Issue = ClashSolver.UI.Models.Issue;
using ClashSolver.UI;
using System.Windows;
using System.Windows.Data;
using System.Runtime.ConstrainedExecution;
using DocumentFormat.OpenXml.Office.CustomXsn;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using System.Xml.Linq;

namespace ClashSolver.Forms.ValidationResolution
{
	public class ExRunValidationUIController : RunValidationUIController
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

		public ExRunValidationUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			RunValidationController ins = m_Handler.Instance as RunValidationController;

			Project = Application.thisApp.Project;

			ins.Initialize();

			Document doc = ins.GetDocument();

			// Get detection sets
			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					if(detectionSet.BlinkInstanceId > 0 && ins.Models.Where(x => x.InstanceId == detectionSet.BlinkInstanceId).Count() > 0)
					{
						detectionSet.BLinkedModel = ins.Models.Where(x => x.InstanceId == detectionSet.BlinkInstanceId).FirstOrDefault();
					}

					Sets.Add(detectionSet);
				}
			}

			if(Sets.Count > 0)
			{
				IsValid = true;
				Sets[0].IsSelected = true; // Check first element as a default
			}

			// Get scopes
			var scopes = RevitHelper.GetAllScopeBoxes(doc);
			if(scopes.Count > 0)
			{
				Scopes = [.. scopes];
				CanFilterByScopeBox = true;
				IsFilterByScopeBox = false;
			} 


			// Get levels
			var levels = RevitHelper.GetAllLevels(doc);
			if(levels.Count > 0)
			{
				levels[0].IsSelected = true;
			}
			Levels = [..levels];


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
			RunValidationController ins = m_Handler.Instance as RunValidationController;

			// Collect all clash issues

		}

		public override List<Issue> FindClash(CSElement element)
		{
			RunValidationController ins = m_Handler.Instance as RunValidationController;
			Document doc = ins.GetDocument();
			Element elementA = doc.GetElement(new ElementId(element.Id));

			ClashDetectionSet set = Sets.Where(x => x.Id == element.Set).FirstOrDefault();
			CSCategory categoryA = set.AElementCategories.Where(x => x.ElementId == element.CategoryId).FirstOrDefault();

			var categories = set.BElementCategories.Where(x => x.IsSelected).ToList();

			// Get linked document and transform
			RevitLinkInstance linkInstance = null;

			if (set.IsIncludeLink && set.BlinkInstanceId > 0)
			{
				linkInstance = doc.GetElement(new ElementId(set.BlinkInstanceId)) as RevitLinkInstance;
			}

			List<Element> clashes = [];
			List<Issue> issues = [];

			foreach (var categoryB in categories)
			{
				clashes = RevitHelper.FindClashes(elementA, new ElementId(categoryB.ElementId), linkInstance);

				string disciplineA = set.ALinkedModel.Discipline != LinkDiscipline.None ? set.ALinkedModel.Discipline.ToString() : "";
				string disciplineB = set.BLinkedModel.Discipline != LinkDiscipline.None ? set.BLinkedModel.Discipline.ToString() : "";

				foreach (var elementB in clashes)
				{
					// Continue if the element is the same with elementB
					if (element.Id == elementB.Id.Value)
						continue;

					Intersection intersection = RevitHelper.GetIntersection(elementA, elementB, linkInstance);

					if (intersection == null)
						continue;

					issues.Add(new Issue()
					{
						ProjectId = Project.Id,
						#if REVIT2024 || REVIT2025
						ElementIdA = element.Id,
						ElementIdB = elementB.Id.Value,
						#else
						ElementA = aElem.IntegerValue,
						ElementB = bElem.Id.IntegerValue,
						#endif
						ElementA = $"{disciplineA} {categoryA.Name}",
						ElementB = $"{disciplineB} {categoryB.Name}",

						LinkModelA = set.ALinkedModel,
						LinkModelB = set.BLinkedModel,
						ScopeBox = element.ScopeBox,

						Intersection = intersection,

						Severity = "High",
						ClashDetectionSet = set,
						CategoryA = CategoryTableAdapter.Instance.GetById(categoryA.Id) as CSCategory,
						CategoryB = CategoryTableAdapter.Instance.GetById(categoryB.Id) as CSCategory,
					});

				}
			}

			return issues;
		}

		/// <summary>
		/// Get all elements to be analyzed
		/// </summary>
		/// <returns></returns>
		public override bool RetrieveElementsToBeAnalyzed()
		{
			// Delete existing clash issues stored in the database
			DeleteIssues();

			RunValidationController ins = m_Handler.Instance as RunValidationController;
			Document doc = ins.GetDocument();

			// Get elements
			Elements = [];

			// Get selected detection sets
			var selSets = Sets.Where(x => x.IsSelected).ToList();

			if (IsFilterByParts)
			{
				if (IsFilterByScopeBox)
				{
					if (Scopes.Where(x => x.IsSelected).Count() == 0)
					{
						TaskDialog.Show("Warning", "No scope box has been selected.");
						return false;
					}
				}
			}

			for (int i = 0; i < selSets.Count; i++)
			{
				var set = selSets[i];
				var categories = set.AElementCategories.Where(x => x.IsSelected).ToList();

				for ( int j = 0; j < categories.Count; j++)
				{
					var category = categories[j];
					if (IsFilterByParts)
					{
						if(IsFilterByScopeBox)
						{
							// Filter by Scope Box
							foreach (var scope in Scopes.Where(x => x.IsSelected))
							{
								if(IsFilterByLevel)
								{
									// Filter by Level
									foreach (var level in Levels.Where(x => x.IsSelected))
									{
										Elements.AddRange(GetElements(set.Id, category.ElementId, scope.Id, level.Name));
									}
								}
								else
								{
									Elements.AddRange(GetElements(set.Id, category.ElementId, scope.Id));
								}
							}
						}
						else
						{
							if (IsFilterByLevel)
							{
								foreach (var level in Levels.Where(x => x.IsSelected))
								{
									Elements.AddRange(GetElements(set.Id, category.ElementId, -1, level.Name));
								}
							}
							else
							{
								Elements.AddRange(GetElements(set.Id, category.ElementId));
							}
						}
					}
					else
					{
						Elements.AddRange(GetElements(set.Id, category.ElementId));
					}
				}
			}

			if(Elements.Count == 0)
			{
				TaskDialog.Show("Warning", "There are no elements to be analyzed.");
				return false;
			}

			return Elements.Count > 0;
		}

		/// <summary>
		/// Get Elements from set, category and boundingbox
		/// </summary>
		/// <param name="setId">Id of set the element is contained</param>
		/// <param name="categoryId">Id of category the element is contained</param>
		/// <param name="bbox"> BoundingBoxXYZ the element is contained</param>
		/// <returns></returns>
		private List<CSElement> GetElements(long setId, long categoryId, long scopeId = -1, string levelName = "")
		{
			List<CSElement> res = [];

			RunValidationController ins = m_Handler.Instance as RunValidationController;
			Document doc = ins.GetDocument();
			var elements = new List<Element>();

			FilteredElementCollector collector = new FilteredElementCollector(doc)
						.OfCategoryId(new ElementId(categoryId))
						.WhereElementIsNotElementType();

			if(scopeId > 0)
			{
				// Get the bounding box of the Scope Box
				Element scopeBox = doc.GetElement(new ElementId(scopeId));
				BoundingBoxXYZ bbox = scopeBox.get_BoundingBox(null);

				if (bbox == null)
					return [];

				// Use BoundingBoxIsInsideFilter to efficiently filter elements inside the scope box
				var outline = new Outline(bbox.Min, bbox.Max);
				BoundingBoxIsInsideFilter filter = new BoundingBoxIsInsideFilter(outline);

				elements = [.. collector.WherePasses(filter)];

				if (!string.IsNullOrEmpty(levelName))
				{
					// Filter By level name
					elements = [.. elements.Where(x => RevitHelper.IsElementOnLevel(doc, x.Id, levelName))];
				}
			}
			else
			{
				elements = [.. collector];
			}

			var existingIds = Elements.Select(x => x.Id).ToList();

			foreach( var element in elements)
			{
				//if (existingIds.Contains(element.Id.Value))
				//	continue;

				res.Add(new CSElement()
				{
					Id =  element.Id.Value,
					Name = element.Name,
					Set = setId,
					CategoryId = categoryId,
					ScopeBox = scopeId
				});
			}

			return res;
		}

		public override void UpdateIssues(List<Issue> issues)
		{
			// The message box is not shown because this is invoked in other thread.
			// So we need to display message box in RunValidationController.
			//if (issues.Count == 0)
			//{
			//	TaskDialog.Show("Information", "There are no conflicting elements.");
			//	return;
			//}

			RunValidationController ins = m_Handler.Instance as RunValidationController;
			ins.Issues = issues;

			MakeRequest((int)ClashSolverRequestId.UpdateIssues);
		}

		#endregion
	}
}

