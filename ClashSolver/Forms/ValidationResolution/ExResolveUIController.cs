using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Models;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;

namespace ClashSolver.Forms.Controllers
{
	public class ExResolveUIController : ResolveUIController
	{
		#region Fields

		protected ClashSolverRequestHandler m_Handler;
		protected ExternalEvent m_ExEvent;
		private ObservableCollection<Resolve> _resolves = new ObservableCollection<Resolve>();
		private Resolve _selectedResolve = new Resolve();

		#endregion

		#region Properties

		public ClashSolverRequestHandler Handler { get => m_Handler; }
		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		public ObservableCollection<Resolve> Resolves
		{
			get => _resolves;
			set
			{
				_resolves = value;
				OnPropertyChanged(nameof(Resolves));
			}
		}

		public Resolve SelectedResolve
		{
			get => _selectedResolve;
			set
			{
				_selectedResolve = value;
				OnPropertyChanged(nameof(SelectedResolve));
			}
		}

		public ExResolveUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

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
			ResolveController ins = m_Handler.Instance as ResolveController;

			ins.Resolve = SelectedResolve;
			ins.Settings = Settings;

			MakeRequest((int)ClashSolverRequestId.ResolveIssue);
		}

		public override void OnSelectTarget()
		{
			ElementId targetId = new ElementId(SelectedResolve.TargetId);
			UIDocument uidoc = Application.GetUiApplication().ActiveUIDocument;
			uidoc.Selection.SetElementIds(new List<ElementId>() { targetId });
		}
		#endregion
	}
}
