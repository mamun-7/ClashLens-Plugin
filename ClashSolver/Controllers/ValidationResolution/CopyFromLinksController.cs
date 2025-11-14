#define IsUpdateCategory

#region Namespaces
using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using ClashSolver.Utils;
using ClashSolver.UI.Controllers;
using System.Linq;
using System.Windows.Media;
using Components;
using ClashSolver.UI.TableAdapters;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls;
using Autodesk.Revit.DB.Mechanical;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Architexor.Core;
using ClashSolver.UI;


#endregion

namespace ClashSolver.Controllers
{
	public class CopyFromLinksController : Controller
	{
		public List<LinkedModel> Models { get; set; }

		/// <summary>
		/// Linked model where copy grids and levels
		/// </summary>
		public LinkedModel GridCopyModel { get; set; } = null;

		public List<LinkedModelCopySetting> LinkedModelCopySettings { get; set; }

		public override bool Initialize()
		{
			//	Get Linked Models From Application
			Models = RevitHelper.GetLinkedProjectsFromDB(Application.thisApp.Project.Id);
			LinkedModelCopySettings = new List<LinkedModelCopySetting>();

			return true;
		}

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.CopyFromLinks:
					break;
				case ClashSolverRequestId.CreateLinkInstance:
					using(Transaction trans = new Transaction(doc, "Link Revit Project"))
					{
						try
						{
							trans.Start();

							// Load the Revit model as a link type

							foreach (var setting in LinkedModelCopySettings)
							{
								RevitLinkType linkType = RevitHelper.GetLinkedType(doc, setting.LinkedModel.Name);

								if (linkType == null)
									continue;

								RevitLinkInstance instance = RevitLinkInstance.Create(doc, linkType.Id);

								if (instance == null)
								{
									continue;
								}

								trans.Commit();
							}
						}
						catch (Exception ex) 
						{
							trans.RollBack();
							TraceLogger.Instance.ExceptionLog($"CopyFromLinksController::ProcessRequest => ", ex);
						}
						finally
						{
							TaskDialog.Show("Notice", Constants.COPY_MODEL_SUCCESS);
						}
					}
					break;
				case ClashSolverRequestId.CopyElements:
					foreach (var setting in LinkedModelCopySettings)
					{
						try
						{
							// Get linked model instance
							var linkInstanceId = new ElementId(setting.LinkedModel.InstanceId);
							RevitLinkInstance linkInstance = doc.GetElement(linkInstanceId) as RevitLinkInstance;

							if (linkInstance == null || linkInstance.GetLinkDocument() == null)
							{
								continue;
							}
							Document linkDoc = linkInstance.GetLinkDocument();
							// Copy specified elements from Linked Model
							Autodesk.Revit.DB.Transform transform = linkInstance.GetTransform();

							// Copy Levels and Grids from Linked Model
							var levels = RevitHelper.GetAllLevels(linkDoc);
							var levelIds = levels.Select(level => new ElementId(level.Id)).ToList();
							RevitHelper.CopyElements(doc, linkDoc, levelIds, transform);
							
							// Copy Families from Linked Model
							if (setting.UsedFamilies != null && setting.UsedFamilies.Count > 0)
							{
								if (setting.DuplicatedFamilies.Count > 0)
								{
									var overwriteFamilies = setting.DuplicatedFamilies.Where(f => f.IsSelected).ToList();
									RevitHelper.HandleDuplicateFamilies(doc, linkDoc, overwriteFamilies);
								}

								List<ElementId> familyIdsToCopy = setting.GetFamilyIdsToCopy().Select(x => new ElementId(x)).ToList();
								RevitHelper.CopyFamilies(doc, linkDoc, familyIdsToCopy);
							}

							// Get elements uniquely in specified categories from linked model
							HashSet<ElementId> uniqueElementIds = new HashSet<ElementId>();

							foreach (var category in setting.SelectedElementCategories)
							{
								var linkCollector = new FilteredElementCollector(linkDoc).OfCategoryId(new ElementId(category.ElementId)).WhereElementIsNotElementType().ToElementIds();

								foreach (var elemId in linkCollector)
								{
									uniqueElementIds.Add(elemId);
								}
							}

							if(uniqueElementIds.Count == 0)
								continue;

							HashSet<ElementId> elemsToCopy = new HashSet<ElementId>(uniqueElementIds);

							if (setting.LinkedModel.Discipline == LinkDiscipline.Mechanical ||
								setting.LinkedModel.Discipline == LinkDiscipline.Plumbing)
							{
								// Copy Systems from Linked Model
								var systemIds = RevitHelper.GetSystemTypeIds(linkDoc);
								//var filterSystemIds = RevitHelper.FilterDuplicateSystemTypeByName(doc, linkDoc, systemIds);
								RevitHelper.CopyElements(doc, linkDoc, systemIds, Autodesk.Revit.DB.Transform.Identity);

								foreach (var id in uniqueElementIds)
								{
									foreach (var connectId in RevitHelper.GetRelatedElements(doc.GetElement(id)))
									{
										elemsToCopy.Add(connectId);
									}
								}
							}

							ICollection<ElementId> copyElemIds = RevitHelper.CopyElements(doc, linkDoc, elemsToCopy, transform);

							// If the linked model is plumbing and mechanical, after copying elements, reconnect connected elements.
							if (setting.LinkedModel.Discipline == LinkDiscipline.Mechanical ||
								setting.LinkedModel.Discipline == LinkDiscipline.Plumbing)
							{
								RevitHelper.ReconnectElements(doc, copyElemIds);
							}
						}
						catch (Exception ex)
						{
							TraceLogger.Instance.ExceptionLog("CopyFromLinksController::ProcessRequest => ", ex);

							TaskDialog.Show("Error", Constants.COPY_MODEL_ERROR);
						}
					}

					TaskDialog.Show("Notice", Constants.COPY_MODEL_SUCCESS);

					break;
			}

			return bFinish;
		}


		private bool AreConnectorClose(Connector c1, Connector c2)
		{
			const double Tolerance = 0.01;
			return c1.Origin.DistanceTo(c2.Origin) < Tolerance;
		}

	}

}
