using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Request;
using ClashSolver.Utils;

namespace ClashSolver.Controllers
{
	abstract public class Controller
	{
		protected UIApplication m_uiApp;
		public UIApplication UIApp { get => m_uiApp; set => m_uiApp = value; }

		public abstract bool Initialize();

		public abstract bool ProcessRequest(ClashSolverRequestId reqId);

		public Document GetDocument()
		{
			return m_uiApp.ActiveUIDocument.Document;
		}

		public string GetProjectId()
		{
			Document doc = GetDocument();

			return RevitHelper.GetProjectId(doc);
		}

		public string GetProjectName()
		{
			Document doc = GetDocument();

			return RevitHelper.GetProjectName(doc);
		}

		public string GetProjectVesion()
		{
			Document doc = GetDocument();

			return doc.Application.VersionNumber;
		}
	}
}
