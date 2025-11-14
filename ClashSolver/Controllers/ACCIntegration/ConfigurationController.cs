using ClashSolver.Request;
using Autodesk.Revit.DB;

namespace ClashSolver.Controllers
{
	public class ConfigurationController : Controller
	{
		#region Initialization

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			return true;
		}

		#endregion

		#region Request Handler

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.Configuration:
					break;
				default:
					break;
			}

			return bFinish;
		}

		#endregion
	}
}
