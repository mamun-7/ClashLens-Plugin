using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using ClashSolver.Utils;
using Architexor.Core;
using Autodesk.Revit.DB.Events;
using ClashSolver.UI.TableAdapters;
using DocumentFormat.OpenXml.Drawing;
using System.Configuration;
using System.Linq;
using DocumentFormat.OpenXml.EMMA;
using System.Windows.Controls;

namespace ClashSolver.Controllers
{
	public class ManageLinksController : Controller
	{
		private List<LinkedModel> _models = new List<LinkedModel>();
		public List<LinkedModel> Models
		{
			get => _models;
			set => _models = value;
		}

		public double Longitude {
			get {
				Document doc = GetDocument();
				ProjectLocation location = doc.ActiveProjectLocation;
				SiteLocation siteLoc = location.GetSiteLocation();
				return siteLoc.Longitude * (180 / Math.PI);
			}
			set {
			}
		}

		public double Latitude {
			get {
				Document doc = GetDocument();
				ProjectLocation location = doc.ActiveProjectLocation;
				SiteLocation siteLoc = location.GetSiteLocation();
				return siteLoc.Latitude * (180 / Math.PI);
			}
			set {
			}
		}

		public double Angle
		{
			get
			{
				Document doc = GetDocument();
				ProjectLocation location = doc.ActiveProjectLocation;
				SiteLocation siteLoc = location.GetSiteLocation();
				ProjectPosition position = location.GetProjectPosition(XYZ.Zero);
				return position.Angle * (180 / Math.PI);
			}
			set
			{
			}
		}

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			//	Get existing linked models from database

			_models = RevitHelper.GetLinkedProjectsFromDB(Application.thisApp.Project.Id);

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
								TraceLogger.Instance.ExceptionLog("ManageLinksController::ProcessRequest => ", ex);
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

		public static bool DocOpenedHandler(DocumentOpenedEventArgs args)
		{
			Document doc = args.Document;

			// Get or create project from database
			string uniqueId = RevitHelper.GetProjectId(doc);
			var dbProject = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;

			if (dbProject == null)
			{
				Project project = new Project()
				{
					Name = RevitHelper.GetProjectName(doc),
					UniqueId = RevitHelper.GetProjectId(doc),
					Path = doc.PathName,
					Version = doc.Application.VersionNumber
				};

				long res = ProjectTableAdapter.Instance.Insert(project);

				if (res < 0)
				{
					TaskDialog.Show(Constants.ERROR, Constants.FAIL_ADD_PROJECT);
					return false;
				}

				dbProject = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;
			}

			Application.thisApp.Project = dbProject;

			long projectId = Application.thisApp.Project.Id;

			// Clear existing linked models from the database
			//LinkModelTableAdapter.Instance.DeleteByProjectId(projectId);

			// Get existing linked models from Revit Project
			var linkedModels = RevitHelper.GetLinkedProjects(doc);
			var dbModels = new List<LinkedModel>();
			foreach(var model in LinkModelTableAdapter.Instance.GetByProjectId(projectId))
			{
				if(model is LinkedModel)
				{
					dbModels.Add(model as LinkedModel);
				}
			}

			if (linkedModels.Count > 0)
			{
				// Add linked model in the project if it does not exist in the database.
				foreach (var model in linkedModels)
				{
					if (dbModels.Where(x => x.InstanceId == model.InstanceId).Count() == 0)
					{
						//	Insert the linked model to the database
						model.ProjectId = projectId;
						LinkModelTableAdapter.Instance.Insert(model);
					}
				}

				// Delete linked model in the database if it does not in the project.
				var deleteModels = dbModels.Where(model => !linkedModels.Select(x => x.InstanceId).Contains(model.InstanceId));
				if(deleteModels.Any())
				{
					foreach (var model in deleteModels)
					{
						LinkModelTableAdapter.Instance.Delete(model.Id);
					}
				}
			}

			return true;
		}

		public static bool DocChangedHandler(DocumentChangedEventArgs args)
		{
			Document doc = args.GetDocument();

			bool bHas = false;

			try
			{
				// first we check if the element was deleted
				ICollection<ElementId> elems = args.GetDeletedElementIds();

				if (elems.Count > 0)
				{
					var linkModels = LinkModelTableAdapter.Instance.GetAll().Cast<LinkedModel>().ToList();

					foreach (ElementId eId in elems)
					{
						if (linkModels.Select(x => x.ElementId).Contains(eId.Value))
						{
							var model = linkModels.Where(x => x.ElementId == eId.Value).FirstOrDefault();

							//	Remove the linked model from the database
							if(model != null)
							{
								long res = LinkModelTableAdapter.Instance.Delete(model.Id);
							}
						}
					}
				}

				elems = args.GetModifiedElementIds();

				if(elems.Count > 0)
				{
					foreach (ElementId eId in elems)
					{
						Element e = doc.GetElement(eId);

						if (e is RevitLinkType linkType)
						{
							
						}
					}
				}

				elems = args.GetAddedElementIds();
				if (elems.Count > 0)
				{
					foreach (ElementId eId in elems)
					{
						Element e = doc.GetElement(eId);

						if (e is RevitLinkInstance link)
						{
							RevitLinkType linkType = doc.GetElement(link.GetTypeId()) as RevitLinkType;

							if(Application.thisApp.Project != null)
							{
								LinkedModel model = new LinkedModel()
								{
									ElementId = linkType.Id.Value,
									Name = linkType.Name,
									ProjectId = Application.thisApp.Project.Id,
									InstanceId = link.Id.Value,
									Url = RevitHelper.GetLinkedModelPath(doc, linkType)
								};

								//	Remove the linked model from the database
								long res = LinkModelTableAdapter.Instance.Insert(model);
							}
						}
					}
				}

				return bHas;
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.Message);
				return false;
			}
		}

		public static bool DocSavedHandler(DocumentSavedEventArgs args)
		{
			//Controller controller;
			try
			{
				Document doc = args.Document;

				long projectId = Application.thisApp.Project.Id;

				var linkedModels = RevitHelper.GetLinkedProjects(doc);

				if (linkedModels != null && linkedModels.Count > 0)
				{
					foreach (var model in linkedModels)
					{
						var dbModel = LinkModelTableAdapter.Instance.GetByInstanceId(projectId, model.InstanceId);

						if (dbModel == null)
						{
							//	Insert the linked model to the database
							model.ProjectId = projectId;
							LinkModelTableAdapter.Instance.Insert(model);
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.ToString());
				TraceLogger.Instance.ExceptionLog("ManageLinksController::DocSavedHandler => ", ex);
				return false;
			}
		}

		public static bool DocSavedAsHandler(DocumentSavedAsEventArgs args)
		{
			//Controller controller;
			try
			{
				Document doc = args.Document;

				long projectId = Application.thisApp.Project.Id;

				var linkedModels = RevitHelper.GetLinkedProjects(doc);

				if (linkedModels != null && linkedModels.Count > 0)
				{
					foreach (var model in linkedModels)
					{
						var dbModel = LinkModelTableAdapter.Instance.GetByInstanceId(projectId, model.InstanceId);

						if (dbModel == null)
						{
							//	Insert the linked model to the database
							model.ProjectId = projectId;
							LinkModelTableAdapter.Instance.Insert(model);
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.ToString());
				TraceLogger.Instance.ExceptionLog("ManageLinksController::DocSavedHandler => ", ex);
				return false;
			}
		}
	}
}
