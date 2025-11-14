using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Models;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;

namespace ClashSolver.Forms.Controllers
{
	public class ExAIResolveUIController : AIResolveUIController
	{

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		public ClashSolverRequestHandler Handler { get => m_Handler; }

		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		private ObservableCollection<Resolve> _resolves = new ObservableCollection<Resolve>();

		public ObservableCollection<Resolve> Resolves
		{
			get => _resolves;
			set
			{
				_resolves = value;
				OnPropertyChanged(nameof(Resolves));
			}
		}

		private Resolve _selectedResolve = new Resolve();

		public Resolve SelectedResolve
		{
			get => _selectedResolve;
			set
			{
				_selectedResolve = value;
				OnPropertyChanged(nameof(SelectedResolve));
			}
		}

		public ExAIResolveUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			WakeUp();
		}

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

		#region Event Handlers
		public override void OnOK()
		{
			AIResolveController ins = m_Handler.Instance as AIResolveController;

			ins.Resolve = SelectedResolve;

			MakeRequest((int)ClashSolverRequestId.ResolveIssue);
		}
		#endregion
	}
}
