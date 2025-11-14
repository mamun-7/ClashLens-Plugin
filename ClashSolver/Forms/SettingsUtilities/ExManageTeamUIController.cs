using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;

namespace ClashSolver.Forms.Controllers
{
	public class ExManageTeamUIController : ManageTeamUIController
	{

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		public ClashSolverRequestHandler Handler { get => m_Handler; }

		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		public ExManageTeamUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			ManageTeamController ins = m_Handler.Instance as ManageTeamController;

			ins.Initialize();

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

		#region Controller implementation
		public override void OnOK()
		{
			ManageTeamController ins = m_Handler.Instance as ManageTeamController;

			//MakeRequest((int)ClashSolverRequestId.CopyElements);
		}
		#endregion
	}
}
