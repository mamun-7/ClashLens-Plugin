using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using ClashSolver.Utils;

namespace ClashSolver.Controllers
{
	public class ManageTeamController : Controller
	{
		private List<LinkedModel> _models = new List<LinkedModel>();
		public List<LinkedModel> Models
		{
			get => _models;
			set => _models = value;
		}

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();
			
			_models = RevitHelper.GetLinkedProjects(doc);

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
				case ClashSolverRequestId.ManageLinks:
					break;
				case ClashSolverRequestId.ManageLinksClosed:
					bFinish = true;

					//	Get Existing LinkedModels
					ICollection<ElementId> collection = ExternalFileUtils.GetAllExternalFileReferences(doc);

					foreach(LinkedModel model in Models)
					{
						bool bExist = false;
						foreach(ElementId eId in collection)
						{
							if(eId.Value == model.ElementId)
							{
								bExist = true;
								break;
							}
						}

						if(!bExist)
						{
							//	Convert the file path to a ModelPath
							ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(model.Url);

							//	Check if the file exists
							if(!System.IO.File.Exists(model.Url))
							{
								TaskDialog.Show("Error", "The specified linked model file does not exist.");
								return false;
							}

							try
							{
								Transaction trans = new Transaction(doc);
								trans.Start("Add Linked Model");

								//	Create the RevitLinkType
								RevitLinkOptions options = new RevitLinkOptions(false);
								LinkLoadResult result = RevitLinkType.Create(doc, modelPath, options);

								//	Create an instance of the Linked model
								RevitLinkInstance revitLinkInstance = RevitLinkInstance.Create(doc, result.ElementId);
								trans.Commit();
							}
							catch (Exception ex)
							{
								TaskDialog.Show("Error", $"Failed to add linked model: {ex.Message}");
								return false;
							}
						}
					}

					break;
				default:
					break;
			}

			return bFinish;
		}

	}
}
