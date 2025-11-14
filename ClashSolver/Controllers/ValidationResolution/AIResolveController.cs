using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ClashSolver.Request;
using ClashSolver.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB.Plumbing;
using ClashSolver.Models;
using ClashSolver.Resolver;
using ClashSolver.UI.TableAdapters;
using System.Runtime.Remoting;

namespace ClashSolver.Controllers
{
	public class AIResolveController : Controller
	{
		public Resolve Resolve { get; set; }

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			return true;
		}

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch (reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.ResolveIssue:
					{
						//var targetElement = doc.GetElement(new ElementId(Resolve.TargetId));

						//switch (Resolve.Type)
						//{
						//	case ResolveType.Move:

						//		var moveParam = Resolve.Parameter as MoveResolveParameter;

						//		XYZ vector = new XYZ(moveParam.X, moveParam.Y, moveParam.Z);

						//		// Start a transaction to modify the Revit model
						//		using (Transaction trans = new Transaction(doc, "Move Pipe"))
						//		{
						//			trans.Start();

						//			if (targetElement is Pipe pipe)
						//			{
						//				// Move the pipe by the given vector
						//				pipe.Location.Move(vector);  // This moves the pipe in the model

						//			}

						//			trans.Commit();
						//		}
						//		break;
						//	default:
						//		break;
						//}

					}
					break;
				default:
					break;
			}

			return bFinish;
		}

		private ElementId CopyElement(long instanceId, long elementId)
		{
			Document doc = GetDocument();

			var linkInstanceA = doc.GetElement(new ElementId(instanceId)) as RevitLinkInstance;
			var linkDocA = linkInstanceA.GetLinkDocument();
			var transformA = linkInstanceA.GetTransform();
			var elements = RevitHelper.CopyElements(doc, linkDocA, new List<ElementId>() { new ElementId(elementId) }, transformA);

			if(elements != null && elements.Count == 1)
			{
				return elements.First();
			}
			else
			{
				return null;
			}
		}
	}
}
