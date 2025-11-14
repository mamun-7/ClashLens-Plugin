using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Architexor.Core;
using ClashSolver.Utils;
using ClashSolver.UI;
using ClashSolver.Request;
using ClashSolver.Forms;
using ClashSolver.Forms.ValidationResolution;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Forms.Controllers;
using Autodesk.Revit.DB;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using ATXLicense;
using QLicense;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ClashSolver
{
	public class Application : IExternalApplication
	{
		#region Fields

		public static Application thisApp = null;//	internal
		internal static UIControlledApplication UIContApp = null;

		private readonly List<IExternal> m_Forms = new();
		private readonly List<Assembly> m_Assemblies = new();

		private EventHandler<DocumentCreatedEventArgs> m_hDocCreated = null;
		private EventHandler<DocumentChangedEventArgs> m_hDocChanged = null;
		private EventHandler<DocumentOpenedEventArgs> m_hDocOpened = null;
		private EventHandler<DocumentSavedEventArgs> m_hDocSaved = null;
		private EventHandler<DocumentSavedAsEventArgs> m_hDocSavedAs = null;
		private EventHandler<ViewActivatedEventArgs> m_hViewActivated = null;
		private EventHandler<SelectionChangedEventArgs> m_hSelectionChanged = null;

		private IssueDockPanel DockPanelProvider;

		public DockablePaneId PaneId => new DockablePaneId(new Guid("FAF92697-2CE7-46E0-B7D2-53037BD55505"));

		public ExReviewIssuesUIController ReviewIssuesUIController;

		public MarkerSetting Setting = null;

		#endregion

		#region Properties

		public Project Project { get; set; }

		#endregion

		#region Initialization

		private bool Initialize()
		{

			Document doc = GetUiApplication().ActiveUIDocument.Document;

			// Get the project information from database

			if (string.IsNullOrEmpty(doc.PathName))
			{
				TaskDialog.Show(Constants.ERROR, Constants.REQUIRE_SAVE);
				return false;
			}

			string uniqueId = RevitHelper.GetProjectId(doc);
			Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;

			if (Project == null)
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

				Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;
			}

			// Get the linked models information from api and database
			// This might be a long process, so I don't want to do it in thread
			//LinkedModels = new List<LinkedModel>();
			//var _models = RevitHelper.GetLinkedProjects(doc);
			//int no = 1;

			//foreach (var model in _models)
			//{
			//	var dbModel = LinkModelTableAdapter.Instance.GetByElementId(Project.Id, model.ElementId) as LinkedModel;

			//	if (dbModel != null && dbModel.ElementId == model.ElementId)
			//	{
			//		dbModel.No = no;
			//		LinkedModels.Add(dbModel);
			//	}
			//	else
			//	{
			//		model.No = no;
			//		model.ProjectId = Project.Id;
			//		long res = LinkModelTableAdapter.Instance.Insert(model);

			//		if (res > 0)
			//		{
			//			model.Id = res;

			//			LinkedModels.Add(model);
			//		}
			//		else
			//		{
			//			TaskDialog.Show(Constants.ERROR, Constants.FAIL_ADD_LINKEDMODEL);
			//			return false;
			//		}
			//	}
			//	no++;
			//}

			return true;
		}

		#endregion

		#region Event Handlers

		public Result OnShutdown(UIControlledApplication application)
		{
			for (int i = 0; i < m_Forms.Count; i++)
			{
				if (m_Forms[i].IVisible())
					m_Forms[i].IClose();
			}

			Unsubscribe(GetUiApplication());
			application.ViewActivated -= m_hViewActivated;
			m_hViewActivated = null;

			//	Check for update
			//if (UpdateHelper.CheckForUpdate(int.Parse(UIContApp.ControlledApplication.VersionNumber)))
			//{
			//	try
			//	{
			//		string url = Assembly.GetExecutingAssembly().Location;
			//		url = url.Substring(0, url.LastIndexOf("\\")) + "\\";
			//		//	Get the url of the plugin
			//		Process.Start(url + "AutoUpdater.exe");
			//	}
			//	catch (Exception e)
			//	{
			//		TaskDialog.Show("Error", e.Message);
			//	}
			//}

			return Result.Succeeded;
		}

		public Result OnStartup(UIControlledApplication application)
		{
			//	Read Setting
			try
			{
				//string sSettings = File.ReadAllText("settings.ini");
				//string[] settings = sSettings.Split('\n');
				//foreach (string setting in settings)
				//{
				//	string name = setting.Split('=')[0], value = setting.Split('=')[1];
				//	switch (name)
				//	{
				//		case "API_ENDPOINT":
				//			//Constants.API_ENDPOINT = value;
				//			break;
				//		default:
				//			break;
				//	}
				//}

				thisApp = this;
				UIContApp = application;

				//CheckLicense();

				Subscribe(GetUiApplication());

				m_hViewActivated = new EventHandler<ViewActivatedEventArgs>(OnViewActivated);
				application.ViewActivated += m_hViewActivated;

				//	Create a custom ribbon tab
				string tabName = Constants.BRAND;
				try
				{
					application.CreateRibbonTab(tabName);
				}
				catch (Exception) { }

				PushButtonData btnData;
				PushButton btn;
				BitmapImage img;

				string url = Assembly.GetExecutingAssembly().Location;
				m_Assemblies.Add(Assembly.GetExecutingAssembly());

				//	Get the url of the plugin
				//			url = url.Substring(0, url.LastIndexOf("\\")) + "\\" + "BasicSplit.dll";

				//	This line of code forces to load Components.dll
				//var temp = typeof(Components.MultiSelectComboBox);

				url = url.Substring(0, url.LastIndexOf("\\")) + "\\" + "Components.dll";
				Assembly.LoadFrom(url);

				url = Assembly.GetExecutingAssembly().Location;

				//			if (File.Exists(url))
				{
					//	Load the plugin
					//				Assembly assembly = Assembly.LoadFrom(url);
					//				m_Assemblies.Add(assembly);

					//  Create a ribbon panel
					RibbonPanel panel = application.CreateRibbonPanel(tabName, "Validation & Resolution");

					//	Create push buttons
					btnData = new("btnCopyFromLinks", "Copy From\nLinks", url, "ClashSolver.Commands.CopyFromLinksCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("copy_link_32.png");
					btn.LargeImage = img;

					//btnData = new("btnQuickDetection", "Quick\nDetection", url, "ClashSolver.Commands.QuickDetectionCommand");
					//btn = panel.AddItem(btnData) as PushButton;
					//img = ResourceHelper.GetEmbeddedImage("run_validation_32.png");
					//btn.LargeImage = img;

					btnData = new("btnRunValidation", "Run\nValidation", url, "ClashSolver.Commands.RunValidationCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("run_validation_32.png");
					btn.LargeImage = img;

					btnData = new("btnReviewIssues", "Review\nIssues", url, "ClashSolver.Commands.ReviewIssuesCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("review_32.png");
					btn.LargeImage = img;

					btnData = new("btnIssueReports", "Issue\nReports", url, "ClashSolver.Commands.IssueReportsCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("report_card_32.png");
					btn.LargeImage = img;

					btnData = new("btnComplianceHealthReport", "Compliance\nHealth Report", url, "ClashSolver.Commands.ComplianceHealthReportCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("health_graph_32.png");
					btn.LargeImage = img;

					//  Create a ribbon panel
					panel = application.CreateRibbonPanel(tabName, "Settings & Utilities");

					//	Create push buttons
					btnData = new("btnManageLinks", "Manage\nLinks", url, "ClashSolver.Commands.ManageLinksCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("message_link_32.png");
					btn.LargeImage = img;

					btnData = new("btnClashSettings", "Clash\nSettings", url, "ClashSolver.Commands.ClashSettingsCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("clash_settings_32.png");
					btn.LargeImage = img;

					btnData = new("btnComplianceSettings", "Compliance\nSettings", url, "ClashSolver.Commands.ComplianceSettingsCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("compliance_setting_32.png");
					btn.LargeImage = img;

					btnData = new("btnFiltersMarkers", "Issue\nMarkers", url, "ClashSolver.Commands.MarkersCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("place_marker_32.png");
					btn.LargeImage = img;

					btnData = new("btnCostDatabase", "Cost\nDatabase", url, "ClashSolver.Commands.CostDatabaseCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("cost_database_32.png");
					btn.LargeImage = img;

					btnData = new("btnManageTeam", "Manage\nTeam", url, "ClashSolver.Commands.ManageTeamCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("team_32.png");
					btn.LargeImage = img;

					// Create a ribbon panel
					panel = application.CreateRibbonPanel(tabName, "ACC Integration");

					// Create push buttons
					btnData = new("btnConfiguration", "Authorize\n", url, "ClashSolver.Commands.ConfigurationCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("login_32.png");
					btn.LargeImage = img;

					// Create push buttons
					btnData = new("btnLinkModel", "Link Model", url, "ClashSolver.Commands.LinkModelCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("link_32.png");
					btn.LargeImage = img;

					// Create push buttons
					btnData = new("btnUpload", "Synchronize", url, "ClashSolver.Commands.SyncModelCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("upload_32.png");
					btn.LargeImage = img;

					//  Create a ribbon panel
					panel = application.CreateRibbonPanel(tabName, "Help & About");

					//	Create push buttons
					btnData = new("btnHelp", "Help", url, "ClashSolver.Commands.HelpCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("help_32.png");
					btn.LargeImage = img;

					btnData = new("btnTutorials", "Tutorials", url, "ClashSolver.Commands.TutorialsCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("tutorial_32.png");
					btn.LargeImage = img;

					btnData = new("btnAbout", "About", url, "ClashSolver.Commands.AboutCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("about_32.png");
					btn.LargeImage = img;

					btnData = new("btnLog", "Log Export", url, "ClashSolver.Commands.LogCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("log_32.png");
					btn.LargeImage = img;

					btnData = new("btnLicense", "License", url, "ClashSolver.Commands.LicenseCommand");
					btn = panel.AddItem(btnData) as PushButton;
					img = ResourceHelper.GetEmbeddedImage("clash_settings_32.png");
					btn.LargeImage = img;
				}

				// Register Issue Dock Panel
				if (!DockablePane.PaneIsRegistered(PaneId))
				{
					ReviewIssuesUIController = new ExReviewIssuesUIController(ClashSolverRequestId.ReviewIssues, GetUiApplication());
					DockPanelProvider = new IssueDockPanel(ReviewIssuesUIController);

					UIContApp.RegisterDockablePane(PaneId, "Review Issues", DockPanelProvider);
				}

				// Load settings

				Setting = SettingTableAdpater.Instance.GetAll().Cast<MarkerSetting>().FirstOrDefault();

				if (Setting == null)
				{
					Setting = new MarkerSetting();

					SettingTableAdpater.Instance.Insert(Setting);
				}

				url = url.Substring(0, url.LastIndexOf("\\")) + "\\" + "ClashSolver.UI.dll";
				m_Assemblies.Add(Assembly.LoadFrom(url));
			}
			catch (Exception ex) 
			{
				TraceLogger.Instance.ExceptionLog("Aplication::OnStartup => ", ex);
				TaskDialog.Show("Application Error", ex.Message);
			}

			return Result.Succeeded;
		}

		public void CheckLicense()
		{
			License _lic = new();
            string sDeviceId = LicenseHandler.GenerateUID(_lic.AppName);

            ApiService.GetAsync(Constants.API_ENDPOINT + "core/subscription/license_check?deviceId=" + sDeviceId).ContinueWith(task =>
            {
                if (task.Exception == null)
                {
                    Architexor.Core.Utils.ParseLicenseResponse(task.Result);
                }
                else
                {
                    //	Exception
                    //task.Exception.InnerException?.Message;
                }
            });
        }

		private void Subscribe(UIApplication uiapp)
		{
			if (m_hDocCreated == null)
			{
				m_hDocCreated = new EventHandler<DocumentCreatedEventArgs>(DocCreatedHandler);
				uiapp.Application.DocumentCreated += m_hDocCreated;
			}

			if (m_hDocOpened == null)
			{
				m_hDocOpened = new EventHandler<DocumentOpenedEventArgs>(DocOpenedHandler);
				uiapp.Application.DocumentOpened += m_hDocOpened;
			}

			if (m_hDocChanged == null)
			{
				m_hDocChanged = new EventHandler<DocumentChangedEventArgs>(DocChangedHandler);
				uiapp.Application.DocumentChanged += m_hDocChanged;
			}

			if (m_hDocSaved == null)
			{
				m_hDocSaved = new EventHandler<DocumentSavedEventArgs>(DocSavedHandler);
				uiapp.Application.DocumentSaved += m_hDocSaved;
			}

			if (m_hDocSavedAs == null)
			{
				m_hDocSavedAs = new EventHandler<DocumentSavedAsEventArgs>(DocSavedAsHandler);
				uiapp.Application.DocumentSavedAs += m_hDocSavedAs;
			}

			if(m_hSelectionChanged == null)
			{
				m_hSelectionChanged = new EventHandler<SelectionChangedEventArgs>(OnSelectionChanged);
				uiapp.SelectionChanged += m_hSelectionChanged;
			}
		}

		/// <summary>
		///   Unsubscribing from DocumentOpened event
		/// </summary>
		/// 
		private void Unsubscribe(UIApplication uiapp)
		{
			if (m_hDocCreated == null)
			{
				uiapp.Application.DocumentCreated -= m_hDocCreated;
			}

			if (m_hDocOpened != null)
			{
				uiapp.Application.DocumentOpened -= m_hDocOpened;
				m_hDocOpened = null;
			}

			if (m_hDocSaved != null)
			{
				uiapp.Application.DocumentSaved -= m_hDocSaved;
				m_hDocSaved = null;
			}

			if (m_hDocSavedAs != null)
			{
				uiapp.Application.DocumentSavedAs -= m_hDocSavedAs;
				m_hDocSavedAs = null;
			}

			if (m_hDocChanged == null)
			{
				uiapp.Application.DocumentChanged -= m_hDocChanged;
				m_hDocChanged = null;
			}
		}

		public void DocCreatedHandler(object sender, DocumentCreatedEventArgs e)
		{
			// Get the created document
			Document doc = e.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			// Check if it's a project document

			ReviewIssuesUIController.Clear();

		}

		public void DocChangedHandler(object sender, DocumentChangedEventArgs args)
		{
			//Controller controller;
			Document doc = args.GetDocument();
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			try
			{
				// Update linked models in the database while inserting and deleting it in Revit project
				MethodInfo mi = thisApp.GetClassType("ManageLinksController")?.GetMethod("DocChangedHandler");
				bool hasML = false;
				if (mi != null)
					hasML = (bool)mi.Invoke(null, new object[] { args });

				// Update clash issues by Scope Box in view3D in Revit
				mi = thisApp.GetClassType("ReviewIssuesController")?.GetMethod("DocChangedHandler");
				bool bHasRI = false;
				if (mi != null)
					bHasRI = (bool)mi.Invoke(null, new object[] { args });

			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.Message + "\n" + ex.StackTrace);
			}
		}

		public void DocOpenedHandler(object sender, DocumentOpenedEventArgs args)
		{
			Document doc = args.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			try
			{
				MethodInfo mi = thisApp.GetClassType("ManageLinksController")?.GetMethod("DocOpenedHandler");
				bool bHasHL = false;
				if (mi != null)
					bHasHL = (bool)mi.Invoke(null, new object[] { args });

				if (CategoryTableAdapter.Instance.GetAll().Count == 0)
				{
					foreach (var elemCategory in RevitHelper.GetElementCategories(doc))
					{
						CategoryTableAdapter.Instance.Insert(elemCategory);
					}
				}
			}
			catch (Exception ex)
			{

				TraceLogger.Instance.ExceptionLog("Application::DocOpenedHandler => ", ex);
				//MessageBox.Show(ex.StackTrace);
			}
		}

		public void DocSavedHandler(object sender, DocumentSavedEventArgs args)
		{
			Document doc = args.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			try
			{
				ProjectUniqueIdValdiate(doc);

				// Insert prject to the database while saving document if it does not exist
				string uniqueId = RevitHelper.GetProjectId(doc);
				Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;

				if (Project == null)
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
					}

					Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;
				}

				MethodInfo mi = thisApp.GetClassType("ManageLinksController")?.GetMethod("DocSavedHandler");
				if (mi != null)
					mi.Invoke(null, new object[] { args });
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("Application::DocSavedHandler => ", ex);
				//MessageBox.Show(ex.StackTrace);
			}
		}

		public void DocSavedAsHandler(object sender, DocumentSavedAsEventArgs args)
		{
			Document doc = args.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			try
			{
				ProjectUniqueIdValdiate(doc);


				// Insert prject to the database while saving document if it does not exist
				string uniqueId = RevitHelper.GetProjectId(doc);
				Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;

				if (Project == null)
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
					}

					Project = ProjectTableAdapter.Instance.GetByUniqueId(uniqueId) as Project;
				}

				MethodInfo mi = thisApp.GetClassType("ManageLinksController")?.GetMethod("DocSavedAsHandler");
				if (mi != null)
					mi.Invoke(null, new object[] { args });
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("Application::DocSavedAsHandler => ", ex);
				//MessageBox.Show(ex.StackTrace);
			}
		}

		public void OnIdlingEvent(object sender, IdlingEventArgs e)
		{
			//MethodInfo mi = thisApp.GetClassType("HalfLap")?.GetMethod("OnIdlingEvent");
			//if (mi != null) mi.Invoke(null, new object[] { sender, e });

			thisApp.GetUIContApp().Idling -= OnIdlingEvent;
		}

		public void OnViewActivated(object sender, ViewActivatedEventArgs args)
		{
			Document doc = args.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			for (int i = m_Forms.Count - 1; i >= 0; i--)
			{
				IExternal f = m_Forms[i];
				if (f.IIsDisposed())
				{
					f.IClose();
					m_Forms.RemoveAt(i);
					continue;
				}

				f.IClose();
				m_Forms.Remove(f);
			}

			ProjectUniqueIdValdiate(doc);

			if(ReviewIssuesUIController != null)
			{
				// Update Review Issues
				ReviewIssuesUIController.Update(doc);
			}
		}

		public void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			Document doc = args.GetDocument();
			if (doc == null || doc.IsFamilyDocument)
			{
				//TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			ICollection<ElementId> selectedIds = args.GetSelectedElements();

			if (selectedIds.Count == 1)
			{
				Element element = doc.GetElement(selectedIds.FirstOrDefault());

				if (element != null
					&& element is FamilyInstance instance
					&& instance.Symbol.FamilyName == Constants.MARKER_FAMILY_NAME)
				{
					Issue issue = IssueTableAdapter.Instance.GetByTagId(instance.Id.Value);

					// Select Issue on the issue dock panel
					if (issue != null)
					{
						((SolidColorBrush)DockPanelProvider.gridIssues.Resources["SelectionColorKey"]).Color = SystemColors.HighlightColor;
						//ReviewIssuesUIController.SelectIssue(issue);
					}
				}
			}
		}

		#endregion

		#region Request Handlers

		//	This method creates and shows a modeless dialog, unless it already exists.
		//	<remarks>
		//		The external command invokes this on the end-user's request
		//	</remarks>
		public void DoRequest(UIApplication uiapp, ClashSolverRequestId reqId)
		{
			Document doc = uiapp.ActiveUIDocument.Document;
			if (doc == null || doc.IsFamilyDocument)
			{
				TaskDialog.Show(Constants.WARNING, Constants.INVALID_DOCUMENT);
				return;
			}

			try
			{
				for (int i = m_Forms.Count - 1; i >= 0; i--)
				{
					IExternal f = m_Forms[i];
					if (f.IIsDisposed())
					{
						f.IClose();
						m_Forms.RemoveAt(i);
						continue;
					}

					if (f.GetRequestId() == (int)ClashSolverRequestId.ManageLinks
						&& reqId == ClashSolverRequestId.ManageLinks)
					{
						f.IClose();
						m_Forms.Remove(f);
					}

					if (f.GetRequestId() == (int)ClashSolverRequestId.ReviewIssues
						&& reqId == ClashSolverRequestId.ReviewIssues)
					{
						f.IClose();
						m_Forms.Remove(f);
					}
				}

				// We give the objects to the new dialog;
				// The dialog becomes the owner responsible for disposing them, eventually.
				IExternal form = null;
				BaseUIController controller;

				if (!Initialize())
				{
					return;
				}

				switch (reqId)
				{
					case ClashSolverRequestId.CopyFromLinks:

                        controller = new ExCopyFromLinksUIController(reqId, uiapp);
                        form = (IExternal)GetClassInstance("WindowCopyFromLinks", controller);
                        break;
					case ClashSolverRequestId.QuickDetection:
						controller = new ExQuickDetectionUIController(reqId, uiapp);

						if (!controller.IsValid)
						{
							return;
						}

						form = (IExternal)GetClassInstance("WindowQuickDetection", controller);
						break;
					case ClashSolverRequestId.RunValidation:
                        controller = new ExRunValidationUIController(reqId, uiapp);
                        form = (IExternal)GetClassInstance("WindowRunValidation", controller);
                        break;
					case ClashSolverRequestId.ReviewIssues:

						if (DockablePane.PaneIsRegistered(thisApp.PaneId))
						{
							DockablePane docpanel = uiapp.GetDockablePane(thisApp.PaneId);

							if (docpanel.IsShown())
								docpanel.Hide();
							else
								docpanel.Show();
						}

						ReviewIssuesUIController.Initialize();

						//form = (IExternal)GetClassInstance("WndReviewIssues", controller);
						break;
					case ClashSolverRequestId.UpdateIssues:
						ReviewIssuesUIController.Update();
						break;
					case ClashSolverRequestId.AIResolve:
						//ExAIResolveUIController controller = new ExAIResolveUIController(ClashSolverRequestId.AIResolve, Application.GetUiApplication())
						//{
						//	Resolves = [.. Resolves]
						//};
						//WndAIResolve wndResolve = new WndAIResolve(controller);
						//wndResolve.ShowDialog();
						controller = new ExAIResolveUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("WndAIResolve", controller);
						break;
					case ClashSolverRequestId.IssueReport:
						controller = new ExReportIssuesUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("WndReportIssues", controller);
						break;
					case ClashSolverRequestId.ComplianceHealthReport:
						TaskDialog.Show("Notice", "Please wait for the next version.");
						return;
						//controller = new ExComplianceReportUIController(reqId, uiapp);
						//form = (IExternal)GetClassInstance("WndComplianceReport", controller);
						break;
					case ClashSolverRequestId.ManageLinks:
						controller = new ExManageLinksUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("WndManageLinks", controller);
						break;
					case ClashSolverRequestId.ClashSettings:
						controller = new ExClashSettingsUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("WindowClashSetting", controller);
						break;
					case ClashSolverRequestId.ComplianceSettings:
						TaskDialog.Show("Notice", "Please wait for the next version.");
						return;
						//controller = new ExComplianceSettingsUIController(reqId, uiapp);
						//form = (IExternal)GetClassInstance("WindowComplianceSetting", controller);
						break;
					case ClashSolverRequestId.IssueMarkers:
						controller = new ExIssueMarkersUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("IssueMarkerWindow", controller);
						break;
					case ClashSolverRequestId.CostDatabase:
						TaskDialog.Show("Notice", "Please wait for the next version.");
						return;
						//controller = new ExCostDatabaseUIController(reqId, uiapp);
						//form = (IExternal)GetClassInstance("WndCostDatabase", controller);
						break;
					case ClashSolverRequestId.ManageTeam:
						TaskDialog.Show("Notice", "Please wait for the next version.");
						return;
						//controller = new ExManageTeamUIController(reqId, uiapp);
						//form = (IExternal)GetClassInstance("WndManageTeam", controller);
						break;
					case ClashSolverRequestId.Configuration:
						TaskDialog.Show("Notice", "Please wait for the next version.");
						return;
						//controller = new ExConfigurationUIController(reqId, uiapp);
						//form = (IExternal)GetClassInstance("WndConfiguration", controller);
						break;
					case ClashSolverRequestId.License:
						controller = new ExLicenseUIController(reqId, uiapp);
						form = (IExternal)GetClassInstance("LicenseWindow", controller);
						break;
					default:
						break;
				}

				if (form != null)
				{
					form.IShow();
					m_Forms.Add(form);
				}
			}
			catch (Exception ex)
			{
				TaskDialog.Show(Constants.ERROR, Constants.DATABASE_ERROR + $"\n {ex}");

				TraceLogger.Instance.ExceptionLog($"Application::DoRequest => ", ex);
			}
		}

		//	Waking up the dialog from its waiting state.
		public void WakeRequestUp(ClashSolverRequestId reqId, bool bFinish = false)
		{
			foreach (IExternal f in m_Forms)
			{
				if (f.GetRequestId() == (int)reqId)
				{
					f.WakeUp(bFinish);
				}
			}
		}

		#endregion

		#region Helper Methods

		public Type GetClassType(string sClassName)
		{
			foreach (Assembly assembly in m_Assemblies)
			{
				IEnumerable<Type> types = null;
				try { types = assembly.ExportedTypes; }
				catch (Exception) { continue; }
				foreach (Type t in types)
				{
					if (t.Name == sClassName)
					{
						return t;
					}
				}
			}
			return null;
		}

		public object GetClassInstance(string sClassName)
		{
			foreach (Assembly assembly in m_Assemblies)
			{
				IEnumerable<Type> types = null;
				try { types = assembly.ExportedTypes; }
				catch (Exception) { continue; }
				foreach (Type t in types)
				{
					if (t.Name == sClassName)
					{
						return Activator.CreateInstance(t);
					}
				}
			}
			return null;
		}

		public object GetClassInstance(string sClassName, params object[] args)
		{
			foreach (Assembly assembly in m_Assemblies)
			{
				foreach (Type t in assembly.ExportedTypes)
				{
					if (t.Name == sClassName)
					{
						return Activator.CreateInstance(t, args);
					}
				}
			}
			return null;
		}

		public UIControlledApplication GetUIContApp()
		{
			return UIContApp;
		}

		public static UIApplication GetUiApplication()
		{
			string versionNumber = UIContApp.ControlledApplication.VersionNumber;
			string fieldName = versionNumber switch
			{
				"2017" or "2018" or "2019" or "2020" or "2021" or "2022" or "2023" or "2024" or "2025" => "m_uiapplication",
				_ => "m_uiapplication",
			};
			var fieldInfo = UIContApp.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

			var uiApplication = (UIApplication)fieldInfo?.GetValue(UIContApp);

			return uiApplication;
		}

		public static void GetGUID()
		{
			//return uiApp.ActiveUIDocument.Document.ProjectInformation.UniqueId;
		}

		private void ProjectUniqueIdValdiate(Document doc)
		{

			// Retrieve the unique ID from the project's shared parameters
			string projectUniqueId = RevitHelper.GetProjectId(doc);

			// If the unique ID is not found, generate a new one and store it
			if (string.IsNullOrEmpty(projectUniqueId))
			{
				projectUniqueId = Guid.NewGuid().ToString();
				StoreProjectUniqueId(doc, projectUniqueId);
			}
		}

		private void StoreProjectUniqueId(Document doc, string uniqueId)
		{
			// Store the unique ID in the project's shared parameters
			//using (Transaction trans = new Transaction(doc, "Store Project Unique ID"))
			//{
			//	trans.Start();
			//	ProjectInfo projectInfo = doc.ProjectInformation;
			//	Parameter param = projectInfo.LookupParameter("ProjectUniqueId)");
			//	if(param == null)
			//	{
			//		// Create the parameter
			//		DefinitionFile defFile = doc.Application.OpenSharedParameterFile();
			//		DefinitionGroup defGroup = defFile.Groups.get_Item("Project");
			//		ExternalDefinition def = defGroup.Definitions.get_Item("ProjectUniqueId") as ExternalDefinition;
			//		param = projectInfo.get_Parameter(def);
			//	}
			//	param.Set(uniqueId);
			//	trans.Commit();
			//}

			// Store the unique ID in the project's global parameters
			using (Transaction trans = new Transaction(doc, "Store Project Unique ID"))
			{
				trans.Start();
				GlobalParameter param = GetOrCreateGlobalParameter(doc, "ProjectUniqueId");
				StringParameterValue stringValue = new StringParameterValue(uniqueId);
				param.SetValue(stringValue);
				trans.Commit();
			}
		}

		private GlobalParameter GetOrCreateGlobalParameter(Document doc, string paramName)
		{
			// Check if the global parameter already exists
			GlobalParameter param = new FilteredElementCollector(doc)
				.OfClass(typeof(GlobalParameter))
				.Cast<GlobalParameter>()
				.FirstOrDefault(p => p.Name == paramName);

			if (param == null)
			{
				// Create the global parameter
				GlobalParameter newParam = GlobalParameter.Create(doc, paramName, SpecTypeId.String.Text);
				return newParam;
			}

			return param;
		}

		#endregion

	}
}
