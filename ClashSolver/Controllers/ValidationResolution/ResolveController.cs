using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Architexor.Core;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using ClashSolver.Models;
using ClashSolver.Request;
using ClashSolver.Resolver;
using ClashSolver.UI;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Utils;
using DocumentFormat.OpenXml.Office.Y2022.FeaturePropertyBag;

namespace ClashSolver.Controllers
{
	public class ResolveController : Controller
	{
		public Resolve Resolve { get; set; }
		public UI.Models.Settings Settings { get; set; }

		public override bool Initialize()
		{
			// Read Linked Models from Revit document
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
					try
					{
						switch (Resolve.Type)
						{
							case ResolveType.Opening:
								bFinish = HandleCreateOpening();
								break;
							case ResolveType.Reroute:
								bFinish = HandleReRouting();
								break;
							default:
								break;
						}
					}
					catch (Exception ex)
					{
						TaskDialog.Show("Error", $"Error while resolving clash. {ex}");
						TraceLogger.Instance.ExceptionLog($"ResolveByOpeningController::ProcessRequest => ", ex);
					}
					break;
				default:
					break;
			}
			return bFinish;
		}

		private bool HandleCreateOpening()
		{
			bool bFinish = false;
			Document doc = GetDocument();

			// Prepare
			// In this case target is Floor, Wall, Structural Opening and the source is Pipes.
			var (target, source, intersection) = Prepare(doc, Resolve.Issue);

			if (target == null || source == null || intersection == null)
			{
				return bFinish;
			}

			// Validate
			if (!Validate(intersection, source, target))
			{
				return bFinish;
			}

			// Create Opening
			CreateOpening(doc, intersection, target, source);

			// Select clashed elements
			Application.GetUiApplication().ActiveUIDocument.Selection.SetElementIds(new List<ElementId>() { target.Id });

			return bFinish;
		}

		private bool HandleReRouting()
		{
			bool bFinish = false;
			Document doc = GetDocument();

			// Prepare
			var (target, host, intersection) = Prepare(doc, Resolve.Issue);

			if (target == null || host == null || intersection == null)
			{
				TaskDialog.Show("Error", "Elements are invalidated.");
				return bFinish;
			}

			ReRoute(doc, host, target);

			return bFinish;
		}

		/// <summary>
		/// Get elements to be interacted each other.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="issue"></param>
		/// <returns></returns>
		private (Element target, Element source, Intersection intersection) Prepare(Document doc, Issue issue)
		{
			ElementId sourceId = new ElementId(issue.ElementIdB == Resolve.TargetId ? issue.ElementIdA: issue.ElementIdB);
			ElementId targetId = new ElementId(issue.ElementIdB == Resolve.TargetId ? issue.ElementIdB : issue.ElementIdA);

			// Get elements from host and link documents
			Element sourceElement = doc.GetElement(sourceId);

			Intersection intersection;
			if (issue.LinkModelB != null && issue.LinkModelB.InstanceId > 0)
			{
				// Get Linked Model
				RevitLinkInstance linkInstance = doc.GetElement(new ElementId(issue.LinkModelB.InstanceId)) as RevitLinkInstance;
				Document linkDocument = linkInstance.GetLinkDocument();
				Element targetElement = linkDocument.GetElement(targetId);

				if (issue.Intersection != null)
				{
					intersection = issue.Intersection;
				}
				else
				{
					intersection = RevitHelper.GetIntersection(sourceElement, targetElement, linkInstance);
				}

				return (targetElement, sourceElement, intersection);
			}
			else
			{
				Element pipeElement = doc.GetElement(targetId);

				if (issue.Intersection != null)
				{
					intersection = issue.Intersection;
				}
				else
				{
					intersection = RevitHelper.GetIntersection(sourceElement, pipeElement);

				}

				return (pipeElement, sourceElement, intersection);
			}
		}

		private bool Validate(Intersection intersection, Element pipe, Element host)
		{
			Document doc = GetDocument();
			double openingSize = -1;
			XYZ center = RevitHelper.GetXYZPoint(intersection.Center);
			XYZ direction = RevitHelper.GetXYZPoint(intersection.Direction).Normalize();

			// Get the bouding box of the intersection
			XYZ minPoint = center + RevitHelper.GetXYZPoint(intersection.Min);
			XYZ maxPoint = center + RevitHelper.GetXYZPoint(intersection.Max);

			XYZ minPoint1 = RevitHelper.GetGlobalPoint(host, RevitHelper.GetXYZPoint(intersection.Min));
			XYZ maxPoint1 = RevitHelper.GetGlobalPoint(host, RevitHelper.GetXYZPoint(intersection.Max));

			// Project the bounding box corners onto the pipe direction
			double minProjection = minPoint.DotProduct(direction);
			double maxProjection = maxPoint.DotProduct(direction);

			// Calculate the opening withd along the pipe direction
			double openingWidth = Math.Abs(maxProjection - minProjection);

			if(Settings.IsCreateVerticalOpening || Settings.IsCreateHorizontalOpening)
			{
				return true;
			}

			//if (openingWidth < Util.MmToIU(Settings.MinOpeningSize))
			//{
			//	TaskDialog.Show("Warnings", "The opening was ignored because its size is smaller than the minimum opening size requirement.");
			//	return false;
			//}

			double angle = RevitHelper.CalculateAngleBetweenPipeAndHost(doc, pipe, host);
			
			//if (Util.DegreeToRadian(Settings.MinOpeningSlope) < angle)
			//{
			//	TaskDialog.Show("Warnings", "The opening was ignored because its slope exceeds the minimum opening slope threshold.");
			//	return false;
			//}

			return true;
		}

		private void CreateOpening(Document doc, Intersection intersection, Element host, Element target)
		{
			BaseOpeningResolver resolver = null;

			switch (Resolve.Category.Name)
			{
				case "Walls":
					resolver = new WallOpeningResolver();
					break;
				case "Floors":
					resolver = new FloorOpeningResolver();
					break;
				case "Ceilings":
					resolver = new CeilOpeningResolver();
					break;
				case "Structural Framing":
					resolver = new BeamOpeningResolver();
					break;
				default:
					break;
			}

			resolver.Document = doc;
			resolver.Intersection = intersection;
			resolver.HostElement = host;
			resolver.PipeElement = target;
			resolver.Settings = Settings;

			resolver.CreateOpening();
		}

		private void ReRoute(Document doc, Element host, Element target)
		{
			Pipe pipe = target as Pipe;
			if (pipe == null)
			{
				TaskDialog.Show("Error", "The target eleemnt is not pipe.");
				return;
			}
			Collection<ElementId> clashIds = new Collection<ElementId>() { host.Id };

			using (Transaction trans = new Transaction(doc, "ReRoute Element"))
			{
				trans.Start();

				ReRoutingResolver resolver = new ReRoutingResolver();

				resolver.Resolve(pipe, clashIds);

				trans.Commit();
			}
		}
	}
}
