using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.Utils;

namespace ClashSolver.Forms.Controllers
{
	public class ExConfigurationUIController : ConfigurationUIController
	{
		#region Fields

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		#endregion

		#region Properties

		public ClashSolverRequestHandler Handler { get => m_Handler; }

		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		#endregion

		#region Initialization

		public ExConfigurationUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			ConfigurationController ins = m_Handler.Instance as ConfigurationController;

			ins.Initialize();

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
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			base.OnOK();

			//MakeRequest((int)ClashSolverRequestId.CopyElements);
		}

		public override string GetProjectId()
		{
			ClashSettingsController ins = m_Handler.Instance as ClashSettingsController;

			return RevitHelper.GetProjectId(ins.GetDocument());
		}
		#endregion
	}
}
