using ClashSolver.Controllers;
using ClashSolver.Request;
using Autodesk.Revit.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using ClashSolver.Utils;
using DocumentFormat.OpenXml.EMMA;
using System.Collections.Generic;
using System.Windows.Controls;
using ClashSolver.UI.TableAdapters;
using System.Linq;
using ClashSolver.UI;

namespace ClashSolver.Forms.Controllers
{
	public class ExCopyFromLinksUIController : CopyFromLinksUIController
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

		public ExCopyFromLinksUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			CopyFromLinksController ins = m_Handler.Instance as CopyFromLinksController;

			ins.Initialize();

			LinkedModels = [.. ins.Models];

			if(LinkedModels != null && LinkedModels.Count > 0)
			{
				SelectedCopyModel = LinkedModels[0];
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
		public override void OnOK()
		{
			CopyFromLinksController ins = m_Handler.Instance as CopyFromLinksController;
			ins.LinkedModelCopySettings = [.. LinkedModelCopySettings];

			// Set linked model to copy grids and levels in CopyFromLinksController
			ins.GridCopyModel = SelectedCopyModel;

			if (!Validate())
				return;

			MakeRequest((int)ClashSolverRequestId.CopyElements);
		}

		private bool Validate()
		{
			CopyFromLinksController ins = m_Handler.Instance as CopyFromLinksController;
			Document doc = ins.GetDocument();

			foreach (var setting in LinkedModelCopySettings)
			{
				// Get linked model instance
				var linkInstanceId = new ElementId(setting.LinkedModel.InstanceId);
				RevitLinkInstance linkInstance = doc.GetElement(linkInstanceId) as RevitLinkInstance;

				if (linkInstance == null || linkInstance.GetLinkDocument() == null)
				{
					continue;
				}

				Document linkDoc = linkInstance.GetLinkDocument();

				// Get elements uniquely in specified categories from linked model
				HashSet<ElementId> uniqueElementIds = new HashSet<ElementId>();

				foreach (var category in setting.SelectedElementCategories)
				{
					var categoryId = new ElementId(category.ElementId);
					var linkCollector = new FilteredElementCollector(linkDoc).OfCategoryId(categoryId).WhereElementIsNotElementType().ToElementIds();

					foreach (var elemId in linkCollector)
					{
						uniqueElementIds.Add(elemId);
					}

					// Handle duplicated familes between linked model and current model
					var usedFamilies = RevitHelper.GetUsedFamilyIds(linkDoc, categoryId);
					var duplicatedFamilies = RevitHelper.GetDuplicatedFamilies(doc, linkDoc, usedFamilies);

					setting.UsedFamilies = usedFamilies;
					setting.DuplicatedFamilies = duplicatedFamilies;
				}

			}

			return true;
		}

		protected override LinkedModelCopySetting GetLinkedModelCopySetting(LinkedModel model)
		{
			CopyFromLinksController ins = m_Handler.Instance as CopyFromLinksController;
			Document doc = ins.GetDocument();

			RevitLinkInstance linkInstance = doc.GetElement(new ElementId(model.InstanceId)) as RevitLinkInstance;

			if (linkInstance == null || linkInstance.GetLinkDocument() == null)
			{
				return null;
			}

			List<long> linkCategoryIds = RevitHelper.GetUsedCategoryElementIds(linkInstance.GetLinkDocument());

			// Get Element Categories from database
			var dbElemCategories = new List<CSCategory>();
			foreach (var obj in CategoryTableAdapter.Instance.GetAll())
			{
				if (obj is CSCategory category)
				{
					dbElemCategories.Add(category);
				}
			}

			if (dbElemCategories.Count == 0)
			{
				// Need to get categories from Revit Document
				foreach (var elemCategory in RevitHelper.GetElementCategories(doc))
				{
					CategoryTableAdapter.Instance.Insert(elemCategory);
				}
			}

			// Get Element Categories from database filtering by used categories in Revit Link Document
			var linkElemCategories = dbElemCategories.Where(x => linkCategoryIds.Contains(x.ElementId)).Select(x =>
			new CSCategory()
			{
				Id = x.Id,
				ElementId = x.ElementId,
				Name = x.Name,
				Type = x.Type,
				Version = x.Version,
				IsSelected = false
			}).ToList();

			return new LinkedModelCopySetting()
			{
				LinkedModel = model,
				ElementCategories = [.. linkElemCategories]
			};
		}
		#endregion
	}
}
