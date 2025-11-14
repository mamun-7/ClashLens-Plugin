using ClashSolver.Request;
using Autodesk.Revit.DB;

namespace ClashSolver.Controllers
{
	public class SyncModelController : Controller
	{
		#region Intiitialization

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			return true;
		}

		#endregion

		#region Reqest Handler;

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.SyncModel:
					break;
				default:
					break;
			}

			return bFinish;
		}

		#endregion
	}
}
