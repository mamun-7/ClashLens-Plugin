using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Base;
using ClashSolver.Request;
using System.Collections.Generic;

namespace ClashSolver
{
	abstract public class Controller
	{
		//  Selected elements to apply the action
		protected List<Element> m_Elements = new();

		protected UIApplication m_uiApp;
		public UIApplication UIApp { get => m_uiApp; set => m_uiApp = value; }

		public List<Element> Elements
		{
			get { return m_Elements; }
			set { m_Elements = Elements; }
		}

		public abstract void Initialize();

		public abstract bool ProcessRequest(ClashSolverRequestId reqId);

		public Document GetDocument()
		{
			return m_uiApp.ActiveUIDocument.Document;
		}
	}
}
