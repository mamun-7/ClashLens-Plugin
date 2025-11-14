using System.Collections.ObjectModel;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using Autodesk.Revit.UI;

namespace ClashSolver.Forms.Controllers
{
	public class ExManageLinksUIController : ManageLinksUIController
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

		public ExManageLinksUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);
			
			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			ManageLinksController ins = m_Handler.Instance as ManageLinksController;
			_longitude = ins.Longitude;
			_latitude = ins.Latitude;
			_angle = ins.Angle;

			ins.Initialize();
			LinkedModels = [.. ins.Models];

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
			ManageLinksController ins = m_Handler.Instance as ManageLinksController;
			foreach (LinkedModel model in LinkedModels)
			{
				ins.Models.Add(model);
				LinkModelTableAdapter.Instance.Update(model);
			}

			//MakeRequest((int)ClashSolverRequestId.ManageLinksClosed);
		}
		#endregion
	}
}
