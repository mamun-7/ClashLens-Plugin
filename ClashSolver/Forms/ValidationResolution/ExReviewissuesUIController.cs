using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Utils;
using System.Text.Json;
using Architexor.Core;
using ClashSolver.Models;
using System.Windows.Input;
using ClashSolver.UI;
using System.Windows.Forms;
using System.Linq;

namespace ClashSolver.Forms.Controllers
{
	public class ExReviewIssuesUIController : ReviewIssuesUIController
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

		public ExReviewIssuesUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;

			ins.Initialize();

			FilterCommand = new RelayCommand<object>(OnFilterButtonClick);

			Initialize();
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
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;
		}

		public void Update(Document doc = null)
		{
			//	Initialize Data Context
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;

			if(doc == null)
			{
				doc = ins.GetDocument();
			}

			Project dbProject = ProjectTableAdapter.Instance.GetByUniqueId(RevitHelper.GetProjectId(doc)) as Project;

			if (dbProject == null)
			{
				Project project = new Project()
				{
					Name = RevitHelper.GetProjectName(doc),
					UniqueId = RevitHelper.GetProjectId(doc),
					Path = doc.PathName,
					Version = doc.Application.VersionNumber
				};

				ProjectTableAdapter.Instance.Insert(project);
			}

			Project = ProjectTableAdapter.Instance.GetByUniqueId(RevitHelper.GetProjectId(doc)) as Project;

			if (Project == null)
			{
				return;
			}

			Initialize();

			UpdateFilterSets();

			// Get the activated scope box in View3D
			var currentScopeBox = RevitHelper.GetCurrentScopeBox(doc);
			if (currentScopeBox != null)
			{
				IsFilterByScope = true;
				CurrentScopeBoxId = currentScopeBox.Id.Value;
			}
			else
			{
				IsFilterByScope = false;
			}

			CurrentPageNumber = 1;
		}

		private void OnFilterButtonClick(object columnHeader)
		{
			string headerName = columnHeader.ToString();
			if (headerName == "ElementA" || headerName == "ElementB" || headerName == "Status" || headerName == "Severity")
			{
				SetFilters(headerName);
			}
		}

		public override void ClearAll()
		{
			long res = IssueTableAdapter.Instance.DeleteByProjectId(Project.Id);
			if(res >= 0)
			{
				Issues.Clear();
				PageCount = 1;
				CurrentPageNumber = 1;
			}
		}

		public void Clear()
		{
			Project = null;
			PageCount = 1;
			CurrentPageNumber = 1;
		}

		public override void HighlightClash(Issue issue)
		{
			if (issue == null)
				return;

			SelectedIssue = issue;

			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;
			ins.Issue = issue;

			MakeRequest((int)ClashSolverRequestId.HighlightClash);
		}

		public override void Reset()
		{
			MakeRequest((int)ClashSolverRequestId.ResetIssues);
		}

		public override async Task<bool> ResolveIssueAsync(Issue issue)
		{
			bool res = false;
			List<Resolve> resolves = [];

			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;
			ins.Issue = issue;

			if (IsOpeningResolution(issue) || IsReRoutingResolution(issue))
			{
				issue.ResolveMethod = ResolveMethod.Manual;
				resolves = GetManualResolution(issue);
			} 
			else
			{
				issue.ResolveMethod = ResolveMethod.AI;
				resolves = await GetResolvesByAI(issue);
			}

			if (resolves.Count > 0)
			{
				res = true;
				ins.Resolves = resolves;

				MakeRequest((int)ClashSolverRequestId.ResolveIssue);
			}

			return res;
		}

		public override void UpdateIssues()
		{
			base.UpdateIssues();

			MakeRequest((int)ClashSolverRequestId.FilterTags);
		}

		public List<Resolve> GetManualResolution(Issue issue)
		{
			List<Resolve> res = [];
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;
			Document doc = ins.GetDocument();
			int no = 1;

			if(IsOpeningResolution(issue))
			{
				var targetId = IsMEPCurve(issue.CategoryA.ElementId) ? issue.ElementIdB : issue.ElementIdA;
				var targetCategory = IsMEPCurve(issue.CategoryA.ElementId) ? issue.CategoryB : issue.CategoryA;

				Resolve resolve = new Resolve()
				{
					No = no++,
					TargetId = targetId,
					Category = targetCategory,
					Type = ResolveType.Opening,
					Action = "Opening",
					Description = $"Create opening on the target element.",
					Issue = issue
				};

				res.Add(resolve);
			}

			if(IsReRoutingResolution(issue))
			{
				// There are 2 options to resolve by rerouting.
				Resolve resolve = new Resolve()
				{
					No = no++,
					TargetId = issue.ElementIdA,
					Category = issue.CategoryA,
					Type = ResolveType.Reroute,
					Action = "ReRouting",
					Description = $"Change the route of {issue.CategoryA.Name}({issue.ElementIdA}) element.",
					Issue = issue
				};

				res.Add(resolve);

				resolve = new Resolve()
				{
					No = no++,
					TargetId = issue.ElementIdB,
					Category = issue.CategoryB,
					Type = ResolveType.Reroute,
					Action = "ReRouting",
					Description = $"Change the route of {issue.CategoryB.Name}({issue.ElementIdB}) element.",
					Issue = issue
				};

				res.Add(resolve);
			}
			return res;
		}

		public async Task<List<Resolve>> GetResolvesByAI(Issue issue)
		{
			List<Resolve> res = [];
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;
			Document doc = ins.GetDocument();

			var client = new ChatGPTService();
			var resolutions = await client.GetClashResolutionAsync(ins.GetUserInput(issue));

			if(resolutions == null)
			{
				return res;
			}

			foreach (var choice in resolutions.choices)
			{
				try
				{
					string jsonText = choice.message.content.Replace("'", "\"");
					var resolves = JsonSerializer.Deserialize<List<ChatContent>>(jsonText);
					int temp = 1;
					foreach (var chatResolve in resolves)
					{
						Resolve resolve = new Resolve()
						{
							No = temp,
							TargetId = issue.ElementIdB,
							Category = issue.CategoryB,
							Action = chatResolve.action,
							Description = chatResolve.description,
						};

						ResolveParameter parameter = new ResolveParameter();

						switch (chatResolve.action)
						{
							case "Move":
								parameter = new MoveResolveParameter()
								{
									X = chatResolve.parameter.offset.x,
									Y = chatResolve.parameter.offset.y,
									Z = chatResolve.parameter.offset.z,
								};
								break;
							default:
								break;
						}
						resolve.Parameter = parameter;
						res.Add(resolve);

						temp++;
					}
				}
				catch (Exception ex)
				{
					TraceLogger.Instance.ExceptionLog("ExReviewIssueUIController::GetResolves => ", ex);
					continue;
				}
			}

			return res;
		}

		private bool IsOpeningResolution(Issue issue)
		{
			bool res = false;
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;

			long categoryIdA = issue.CategoryA.ElementId;
			long categoryIdB = issue.CategoryB.ElementId;

			if ( IsHostForOpening(categoryIdA) && IsMEPCurve(categoryIdB) || 
				IsHostForOpening(categoryIdB) && IsMEPCurve(categoryIdA))
			{
				res = true;
			}

			return res;
		}

		private bool IsReRoutingResolution(Issue issue)
		{
			bool res = false;
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;

			long categoryIdA = issue.CategoryA.ElementId;
			long categoryIdB = issue.CategoryB.ElementId;

			//if (IsHostForOpening(categoryIdA) && IsMEPCurve(categoryIdB) ||
			//	IsHostForOpening(categoryIdB) && IsMEPCurve(categoryIdA) ||
			if(IsMEPCurve(categoryIdA) && IsMEPCurve(categoryIdB))
			{
				res = true;
			}

			return res;
		}

		private bool IsHostForOpening(long categoryId)
		{
			return	categoryId == (long)BuiltInCategory.OST_Walls || 
					categoryId == (long)BuiltInCategory.OST_Floors ||
					categoryId == (long)BuiltInCategory.OST_Ceilings ||
					categoryId == (long)BuiltInCategory.OST_StructuralFraming;
		}

		private bool IsMEPCurve(long categoryId)
		{
			return	categoryId == (long)BuiltInCategory.OST_PipeCurves || 
					categoryId == (long)BuiltInCategory.OST_DuctCurves || 
					categoryId == (long)BuiltInCategory.OST_CableTray || 
					categoryId == (long)BuiltInCategory.OST_Conduit || 
					categoryId == (long)BuiltInCategory.OST_FlexPipeCurves || 
					categoryId == (long)BuiltInCategory.OST_FlexDuctCurves;
		}

		public override void Report()
		{
			ReviewIssuesController ins = m_Handler.Instance as ReviewIssuesController;

			MakeRequest((int)ClashSolverRequestId.IssueReport);
		}

		public void ImportIssuesFromExcel()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
				Title = "Open Excel File",
			};

			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = openFileDialog.FileName;

				List<Issue> issues = ExcelAdapter.ImportFromExcel(filePath);

				foreach (var issue in issues)
				{
					IssueTableAdapter.Instance.Insert(issue);
				}

				Update();
			}
		}

		public void SelectIssue(Issue issue)
		{
			SelectedIssue = Issues.Where(x => x.Id == issue.Id).FirstOrDefault();
		}

		#endregion
	}

	public class RelayCommand<T> : ICommand
	{
		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;

		public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
		public void Execute(object parameter) => _execute((T)parameter);
		public event EventHandler CanExecuteChanged;
	}
}
