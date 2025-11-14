using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Utils;

namespace ClashSolver.Forms.Controllers
{
	public class ExClashSettingsUIController : ClashSettingsUIController
	{

		#region Fields

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		#endregion

		#region Properites

		public ClashSolverRequestHandler Handler { get => m_Handler; }

		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		#endregion

		#region Constructor

		public ExClashSettingsUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			//	Initialize Data Context

			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			ins.Initialize();

			m_ExEvent = ExternalEvent.Create(m_Handler);
			WakeUp();
		}

		public override void Initialize()
		{
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			Project = Application.thisApp.Project;

			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					detectionSet.ALinkedModels =
					[
						new LinkedModel() { Name = "Current Project" }
					];

					detectionSet.ALinkedModel = detectionSet.ALinkedModels[0];

					List<LinkedModel> temp =
					[
						new LinkedModel() { Name = "Current Project" }
					];

					if (detectionSet.IsIncludeLink)
					{
						foreach (var model in ins.LinkModels)
						{
							temp.Add(new LinkedModel()
							{
								Name = model.Name,
								ProjectId = model.ProjectId,
								ElementId = model.ElementId,
								InstanceId = model.InstanceId,
								Url = model.Url,
								Discipline = model.Discipline,
								Description = model.Description
							});
						}
					}
					detectionSet.BLinkedModels = [.. temp];

					if (detectionSet.BlinkInstanceId > 0)
					{
						if (detectionSet.BLinkedModels.Where(x => x.InstanceId == detectionSet.BlinkInstanceId).Count() > 0)
						{
							detectionSet.BLinkedModel = detectionSet.BLinkedModels.Where(x => x.InstanceId == detectionSet.BlinkInstanceId).First();
						}
						else
						{
							TaskDialog.Show("Error", $"The linked model could not be found. The {detectionSet.Name} set is not available.");
							DetectionSetTableAdapter.Instance.Delete(detectionSet.Id);
							continue;
						}
					}
					else
					{
						detectionSet.BLinkedModel = detectionSet.BLinkedModels[0];
					}

					//	Remove the categories that are not used in the project
					var usedElementCategories = GetUsedElementCategories(detectionSet.ALinkedModel);
					detectionSet.AElementCategories = [.. detectionSet.AElementCategories.Where(x => usedElementCategories.Contains(x.ElementId))];

					usedElementCategories = GetUsedElementCategories(detectionSet.BLinkedModel);
					detectionSet.BElementCategories = [.. detectionSet.BElementCategories.Where(x => usedElementCategories.Contains(x.ElementId))];

					Sets.Add(detectionSet);
				}
			}

			if (Sets.Count > 0)
			{
				SelectedSet = Sets[0];
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

		#region Controller implementation
		public override void OnOK()
		{
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			base.OnOK();

			//MakeRequest((int)ClashSolverRequestId.CopyElements);
		}

		public override void OnAdd(string name = "")
		{
			int duplicatedCount = Sets.Where(x => x.Name.Contains(name)).Count();

			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;
			Document doc = ins.GetDocument();

			var dbCategories = CategoryTableAdapter.Instance.GetAll().Select(x => x as CSCategory);
			var usedCategories = GetUsedElementCategories();

			List<CSCategory> aCategories = [], bCategories = [];

			foreach (var dbCategory in dbCategories)
			{
				if (usedCategories.Contains(dbCategory.ElementId))
				{
					aCategories.Add(new CSCategory()
					{
						Id = dbCategory.Id,
						ElementId = dbCategory.ElementId,
						Name = dbCategory.Name,
						IsSelected = false,
						Type = dbCategory.Type,
						Version = dbCategory.Version
					});

					bCategories.Add(new CSCategory()
					{
						Id = dbCategory.Id,
						ElementId = dbCategory.ElementId,
						Name = dbCategory.Name,
						IsSelected = false,
						Type = dbCategory.Type,
						Version = dbCategory.Version
					});
				}
			}

			ClashDetectionSet detectionSet = new ClashDetectionSet()
			{
				ProjectId = Project.Id,
				Name = name + (duplicatedCount > 0 ? "(" + duplicatedCount.ToString() + ")" : ""),
				AElementCategories = [..aCategories],
				BElementCategories = [.. bCategories]
			};

			long res = DetectionSetTableAdapter.Instance.Insert(detectionSet);

			if (res > 0)
			{
				detectionSet.Id = res;
				Sets.Add(detectionSet);

				SelectedSet = detectionSet;

				UpdateLinkModels();
			}
		}

		public override void OnDuplicate(int nIndex)
		{

		}

		public override string GetProjectId()
		{
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			return RevitHelper.GetProjectId(ins.GetDocument());
		}

		public override void UpdateLinkModels()
		{
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			List<LinkedModel> temp =
			[
				new LinkedModel() { Name = "Current Project" }
			];

			if (SelectedSet != null && SelectedSet.Id > 0)
			{
				if (SelectedSet.IsIncludeLink)
				{
					foreach (var model in ins.LinkModels)
					{
						temp.Add(model);
					}
				}

				SelectedSet.ALinkedModels =
				[
					new LinkedModel() { Name = "Current Project" }
				];

				SelectedSet.ALinkedModel = SelectedSet.ALinkedModels[0];

				SelectedSet.BLinkedModels = [.. temp];

				if (SelectedSet.BlinkInstanceId > 0 && SelectedSet.BLinkedModels.Where(x => x.InstanceId == SelectedSet.BlinkInstanceId).Count() > 0)
				{
					SelectedSet.BLinkedModel = SelectedSet.BLinkedModels.Where(x => x.InstanceId == SelectedSet.BlinkInstanceId).First();
				}
				else
				{
					SelectedSet.BLinkedModel = SelectedSet.BLinkedModels[0];
				}

				IsSetTabEnabled = true;
			}
			else
			{
				IsSetTabEnabled = false;
			}
		}

		public override List<long> GetUsedElementCategories(LinkedModel linkedModel = null)
		{
			List<long> res = [];

			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;
			Document doc = ins.GetDocument();

			if(linkedModel != null && linkedModel.InstanceId > 0)
			{
				// If the used categories are not set, get them from the linked model
				if (linkedModel.UsedCategories.Count > 0)
				{
					return linkedModel.UsedCategories;
				}
				else // If there are no used categories, get them from the linked model
				{
					RevitLinkInstance linkInstance = doc.GetElement(new ElementId(linkedModel.InstanceId)) as RevitLinkInstance;

					if (linkInstance == null)
					{
						TaskDialog.Show("Error", "The linked model is not valid.");
						return [];
					}

					var usedCategoryIds = RevitHelper.GetUsedCategoryElementIds(linkInstance.GetLinkDocument());

					linkedModel.UsedCategories = usedCategoryIds;
					return usedCategoryIds;
				}
			}
			else // If the linked model is not set, get the used categories from the current project
			{
				return ins.UsedCategories;
			}
		}

		#endregion
	}
}
