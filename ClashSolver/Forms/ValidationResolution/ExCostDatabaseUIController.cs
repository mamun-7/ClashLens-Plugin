using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;

namespace ClashSolver.Forms.Controllers
{
	public class ExCostDatabaseUIController : CostDatabaseUIController
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

		public ExCostDatabaseUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			CostDatabaseController ins = m_Handler.Instance as CostDatabaseController;

			ins.Initialize();

			//Update();

			//WakeUp();
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
	}
}
