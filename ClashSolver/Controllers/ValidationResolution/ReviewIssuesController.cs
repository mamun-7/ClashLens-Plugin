using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using ClashSolver.Utils;
using ClashSolver.Forms.ValidationResolution;
using ClashSolver.UI.ValidationResolution.ReviewIssues;
using System.Collections.ObjectModel;
using ClashSolver.Models;
using System.Linq;
using ClashSolver.UI.Views.ValidationResolution.ReviewIssues;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Forms.Controllers;
using System.Xml.Linq;
using ClashSolver.UI;
using Autodesk.Revit.DB.Events;
using System.Windows;

namespace ClashSolver.Controllers
{
	public class ReviewIssuesController : Controller
	{
		private List<Resolve> _resolves = new List<Resolve>();
		private Issue _issue = new Issue();

		public List<Resolve> Resolves { get => _resolves; set => _resolves = value; }

		public Issue Issue { get => _issue; set => _issue = value; }

		public override bool Initialize()
		{
			return true;
		}

		public static bool DocChangedHandler(DocumentChangedEventArgs args)
		{
			Document doc = args.GetDocument();
			bool bHas = false;

			try
			{
				// first we check if the element was deleted
				ICollection<ElementId> elems = args.GetDeletedElementIds();
				if (elems.Count > 0)
				{
					foreach (ElementId eId in elems)
					{
						Issue issue = IssueTableAdapter.Instance.GetByTagId(eId.Value);
						if(issue != null)
						{
							IssueTableAdapter.Instance.Delete(issue.Id);
						}

						//Element elem = doc.GetElement(eId);
						//if(elem is FamilyInstance)
						//{
						//	FamilyInstance instance = (FamilyInstance)elem;
						//	if (instance.Symbol.FamilyName != Constants.MARKER_FAMILY_NAME) 
						//		continue;

						//	Parameter param = instance.LookupParameter("Clash ID");
						//	if(param?.AsString().Split(' ').Length > 1)
						//	{
						//		long id = Convert.ToInt64(param.AsString().Split(' ')[1]);
						//		Issue issue = IssueTableAdapter.Instance.GetById(id) as Issue;
						//		if(issue != null)
						//		{
						//			IssueTableAdapter.Instance.Delete(id);
						//		}
						//	}
						//}
					}
				}

				elems =  args.GetModifiedElementIds();

				if (!elems.Any()) return bHas;

				foreach (ElementId eId in elems)
				{
					Element e = doc.GetElement(eId);

					if (e is View3D view3D)
					{
						Parameter scopeBoxParam = view3D.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);

						if(scopeBoxParam != null)
						{
							foreach(var id in elems)
							{
								Element elem = doc.GetElement(id);
								if(elem.Category != null && elem.Category.BuiltInCategory == BuiltInCategory.OST_SectionBox)
								{
									Application.thisApp.ReviewIssuesUIController.Update(doc);
								}
							}
						}
					}
				}

				return bHas;
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.Message);
				return false;
			}
		}

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.ReviewIssues:
					break;
				case ClashSolverRequestId.IssueStatus:
					break;
				case ClashSolverRequestId.HighlightClash:
					HighlightClash();
					break;
				case ClashSolverRequestId.ResetIssues:
					ResetElements();
					break;
				case ClashSolverRequestId.ResolveIssue:
					{
						switch (Issue.ResolveMethod)
						{
							case ResolveMethod.Manual:
								{
									var manualController = new ExResolveUIController(ClashSolverRequestId.ManualResolution, Application.GetUiApplication())
									{
										Resolves = [.. Resolves],
										SelectedResolve = Resolves.Count > 0 ? Resolves[0] : new Resolve()
									};

									ResolveWindow wndResolve = new ResolveWindow(manualController);
									wndResolve.ShowDialog();
								}
								break;
							case ResolveMethod.AI:
								{
									// Call AI Resolve
									//Application.thisApp.DoRequest(Application.GetUiApplication(), ClashSolverRequestId.AIResolve);
									var aiController = new ExAIResolveUIController(ClashSolverRequestId.AIResolve, Application.GetUiApplication())
									{
										Resolves = [.. Resolves],
										SelectedResolve = Resolves.Count > 0 ? Resolves[0] : new Resolve()
									};
									WndAIResolve wndResolve = new WndAIResolve(aiController);
									wndResolve.ShowDialog();
								}
								break;
							default:
								break;
						}
								
					}
					break;
				case ClashSolverRequestId.IssueReport: // In case of clikcing Report button on Issue Dock Panel
					{
						Application.thisApp.DoRequest(Application.GetUiApplication(), ClashSolverRequestId.IssueReport);
					}
					break;
				default:
					break;
			}

			return bFinish;
		}

		//private Element GetCopiedElement(long instanceId, long elementId, long copyElementId, bool isA = true)
		//{
		//	Element element = null;
		//	Document doc = GetDocument();

		//	if (instanceId > 0)
		//	{
		//		var linkInstance = doc.GetElement(new ElementId(instanceId)) as RevitLinkInstance;

		//		if (linkInstance == null)
		//		{
		//			TaskDialog.Show("Error", "Cannot find the linked model instance");
		//			return null;
		//		}

		//		if (copyElementId > 0)
		//		{
		//			element = doc.GetElement(new ElementId(copyElementId));
		//		}

		//		var copyElemId = CopyElement(instanceId, elementId);

		//		if (copyElemId != null)
		//		{
		//			if (isA) Issue.CopyElementIdA = copyElemId.Value;
		//			else Issue.CopyElementIdB = copyElemId.Value;
		//			IssueTableAdapter.Instance.Update(Issue);
		//			element = doc.GetElement(copyElemId);
		//		}
		//		else
		//		{
		//			TaskDialog.Show("Error", "Cannot copy the element from the linked model");
		//			return null;
		//		}
		//	}
		//	else
		//	{
		//		element = doc.GetElement(new ElementId(elementId));
		//	}

		//	return element;
		//}

		private ElementId CopyElement(long instanceId, long elementId)
		{
			Document doc = GetDocument();

			var linkInstanceA = doc.GetElement(new ElementId(instanceId)) as RevitLinkInstance;
			var linkDocA = linkInstanceA.GetLinkDocument();
			var transformA = linkInstanceA.GetTransform();
			var elements = RevitHelper.CopyElements(doc, linkDocA, new List<ElementId>() { new ElementId(elementId) }, transformA);

			if (elements != null && elements.Count == 1)
			{
				return elements.First();
			}
			else
			{
				return null;
			}
		}

		public string GetUserInput(Issue issue)
		{
			string res = "";
			Document doc = GetDocument();
			View view = doc.ActiveView;

			Element elementA = RevitHelper.GetElement(doc, issue.ElementIdA, issue.LinkModelA);
			Element elementB = RevitHelper.GetElement(doc, issue.ElementIdB, issue.LinkModelB);

			// Get intersection information
			Intersection intersection = issue.Intersection;

			XYZ minPoint = RevitHelper.GetXYZPoint(intersection.Min);
			XYZ maxPoint = RevitHelper.GetXYZPoint(intersection.Max);
			XYZ centerPoint = RevitHelper.GetXYZPoint(intersection.Center);

			XYZ clashPoint = centerPoint;

			double width = maxPoint.X - minPoint.X;
			double depth = maxPoint.Y - minPoint.Y;
			double height = maxPoint.Z - minPoint.Z;	

			string aMaterialName = RevitHelper.GetMaterialName(elementA);
			string bMaterialName = RevitHelper.GetMaterialName(elementB);

			string userInput = $"**Element 1:** {issue.ElementA} Material: {aMaterialName}" +
				$"**Element 2:** {issue.ElementB} Material: {bMaterialName}" +
				$"**Clash Location:** {centerPoint.X},{centerPoint.Y},{centerPoint.Z})" +
				$"**Clash Type** Hard clash (volume of intersection: {width}x{depth}x{height})";
			return res;
		}

		private void HighlightClash()
		{
			Document doc = GetDocument();

			// Get 3D View from active document
			View3D view3D = doc.ActiveView as View3D;
			if (view3D == null)
			{
				TaskDialog.Show("Error", "Please open the 3D View.");
				return;
			}

			// Zoom in to the clash marker instance
			FamilyInstance markerInstance = doc.GetElement(new ElementId(Issue.TagId)) as FamilyInstance;
			if (markerInstance == null)
				return;
			using (Transaction tx = new Transaction(doc, "Select Family Instance"))
			{
				tx.Start();
				BoundingBoxXYZ bbox = markerInstance.get_BoundingBox(null);
				if (bbox == null)
					return;

				// Select clash marker instance
				var uiapp = Application.GetUiApplication();
				uiapp.ActiveUIDocument.Selection.SetElementIds(new List<ElementId>() { markerInstance.Id });

				RevitHelper.ZoomToBoundingBox(doc, bbox);
				tx.Commit();
			}

			// Get conflicting elements with marker
			List<ElementId> clashElementIds = new List<ElementId>();

			Element elementA = doc.GetElement(new ElementId(Issue.ElementIdA));
			if (elementA != null)
			{
				clashElementIds.Add(elementA.Id);
			}

			Element elementB = doc.GetElement(new ElementId(Issue.ElementIdB));
			if (elementB != null)
			{
				clashElementIds.Add(elementB.Id);
			}

			// Highlight conflicting elements with marker
			using (Transaction trans = new Transaction(doc, "Apply Transparency"))
			{
				trans.Start();

				OverrideGraphicSettings ogs1 = new OverrideGraphicSettings();
				ogs1.SetSurfaceTransparency(90);

				// Get all the elements inside the scope box
				var scopeBox = doc.GetElement(new ElementId(Issue.ScopeBox));

				List<Element> elements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToList();
				// If issue has ScopeBox Filter By ScopeBox
				if (scopeBox != null)
				{
					BoundingBoxXYZ bbox = scopeBox.get_BoundingBox(null);
					elements = RevitHelper.GetElementsInBoundingBox(doc, bbox);
				}

				OverrideGraphicSettings ogs2 = new OverrideGraphicSettings();
				// set the projection line color according to setting
				var setting = Application.thisApp.Setting;
				Color color = new (255, 0, 0);
				string patternName = "<Solid fill>";
				ElementId patternId = RevitHelper.GetFillPatternId(doc, patternName);
				ogs2.SetSurfaceBackgroundPatternId(patternId);
				ogs2.SetSurfaceBackgroundPatternColor(color);
				ogs2.SetProjectionLineColor(color);

				// Apply the override to elements inside the scope box
				foreach ( Element element in elements)
				{
					if (clashElementIds.Contains(element.Id))
					{
						view3D.SetElementOverrides(element.Id, ogs2);
					}
					else
					{
						if(element is FamilyInstance instance && instance.Category.BuiltInCategory == BuiltInCategory.OST_GenericModel)
						{
							continue;
						}

						view3D.SetElementOverrides(element.Id, ogs1);
					}
				}

				trans.Commit();
			}

			// Set the visibility of linked models
			using (Transaction trans = new Transaction(doc, "Set Link Visibility"))
			{
				trans.Start();

				// find the linked files and hide them
				FilteredElementCollector collector = new FilteredElementCollector(doc);
				ICollection<ElementId> elementIdSet =
					collector
					.OfCategory(BuiltInCategory.OST_RvtLinks)
					.OfClass(typeof(RevitLinkType))
					.ToElementIds();

				if(elementIdSet != null && elementIdSet.Count > 0)
				{
					doc.ActiveView.HideElements(elementIdSet);
				}

				// Unhide the link instance contained clashed element
				if (Issue.LinkModelB != null)
				{
					var unhideLinks = new List<ElementId>() { new ElementId(Issue.LinkModelB.ElementId) };
					doc.ActiveView.UnhideElements(unhideLinks);
				}

				trans.Commit();

			}
		}

		private void ResetElements()
		{
			Document doc = GetDocument();

			// Get 3D VIew from active document
			View3D view3D = doc.ActiveView as View3D;
			if (view3D == null) 
			{
				TaskDialog.Show("Error", "Please open the 3D View.");
				return;
			}

			// Reset overrides for all elements in the view
			FilteredElementCollector collector = new FilteredElementCollector(doc, view3D.Id)
				.WhereElementIsNotElementType();

			ResetOverrides(doc, view3D, collector.ToElementIds());
		}

		private void ResetOverrides(Document doc, View3D view3D, IEnumerable<ElementId> elementIds)
		{
			if(view3D == null)
			{
				TaskDialog.Show("Error", "Plase open a 3D View.");
				return;
			}

			using(Transaction trans = new Transaction(doc, "Reset Overrides"))
			{
				trans.Start();

				// Create an empty OverrideGraphicSettings object
				OverrideGraphicSettings defaultOverrides = new OverrideGraphicSettings();

				// Reset overrides for each element
				foreach (ElementId elemId in elementIds)
				{
					Element element = doc.GetElement(elemId);
					if (element is FamilyInstance instance && instance.Category.BuiltInCategory == BuiltInCategory.OST_GenericModel)
					{
						continue;
					}

					view3D.SetElementOverrides(elemId, defaultOverrides);
				}

				trans.Commit();
			}	
		}


		//private void HighlightMarker(FamilyInstance markerInstance)
		//{
		//	UIDocument uidoc = Application.GetUiApplication().ActiveUIDocument;
		//	Document doc = uidoc.Document;

		//	// Select the marker
		//	uidoc.Selection.SetElementIds(new List<ElementId> { markerInstance.Id });

		//	//  Get active 3D view

		//	View3D active3DView = doc.ActiveView as View3D;
		//	if (active3DView == null)
		//	{
		//		TaskDialog.Show("Error", "Please switch to a 3D view to highlight the marker");
		//		return;
		//	}

		//	// Get bounding box of the marker
		//	BoundingBoxXYZ bbox = markerInstance.get_BoundingBox(active3DView);
		//	if (bbox == null)
		//	{
		//		TaskDialog.Show("Error", "Bounding box is null. Ensure the element is visible in the view.");
		//		return;
		//	}

		//	using (Transaction trans = new Transaction(doc, "Zoom to Marker"))
		//	{
		//		trans.Start();

		//		// Adjust the section box of the 3D view to focus on the marker
		//		active3DView.IsSectionBoxActive = true;
		//		active3DView.SetSectionBox(bbox);

		//		trans.Commit();
		//	}
		//}

		//private void HighlightLinkedElements()
		//{
		//	Document doc = GetDocument();

		//	// Use TemporaryGraphicsManager to apply overrides
		//	using (Transaction tx = new Transaction(doc, "Highlight Linked Element"))
		//	{
		//		tx.Start();

		//		HighLightElement(new ElementId(Issue.ElementIdA), new ElementId(Issue.LinkModelA.InstanceId));
		//		HighLightElement(new ElementId(Issue.ElementIdB), new ElementId(Issue.LinkModelB.InstanceId));

		//		//HighLightElementByColor(new ElementId(Issue.ElementIdA), new ElementId(Issue.LinkInstanceA));
		//		//HighLightElementByColor(new ElementId(Issue.ElementIdB), new ElementId(Issue.LinkInstanceB));

		//		//CreateBoundingBoxGeometry(new ElementId(Issue.ElementIdA), new ElementId(Issue.LinkInstanceA));
		//		//CreateBoundingBoxGeometry(new ElementId(Issue.ElementIdB), new ElementId(Issue.LinkInstanceB));

		//		tx.Commit();
		//	}
		//}

		//private void HighLightElement(ElementId elementId, ElementId linkInstanceId)
		//{
		//	Document doc = GetDocument();

		//	var linkInstance = doc.GetElement(linkInstanceId) as RevitLinkInstance;
		//	if (linkInstance == null) return;

		//	var linkDoc = linkInstance.GetLinkDocument();
		//	var linkedElement = linkDoc.GetElement(elementId);

		//	// Create a reference from this eleemnt
		//	var reference = new Reference(linkedElement);

		//	// Convert the reference to be readable from the current document
		//	reference = reference.CreateLinkReference(linkInstance);

		//	var uiDoc = Application.GetUiApplication().ActiveUIDocument;
		//	uiDoc.Selection.SetReferences([reference]);
		//}

		//private void HighLightElementByColor(ElementId elementId, ElementId linkInstanceId)
		//{
		//	Document doc = GetDocument();
		//	Color clashColor = new Color(255, 0, 0); // RGB

		//	// Get the active view
		//	View activeView = doc.ActiveView;
		//	if (activeView == null) return;

		//	var linkInstance = doc.GetElement(linkInstanceId) as RevitLinkInstance;
		//	if (linkInstance == null) return;

		//	var linkDoc = linkInstance.GetLinkDocument();
		//	var linkedElement = linkDoc.GetElement(elementId);
		//	var transform = linkInstance.GetTransform();

		//	// Define graphic override settings
		//	OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
		//	overrideSettings.SetProjectionLineColor(clashColor);      // Set line color
		//	overrideSettings.SetProjectionLineWeight(8);         // Set bold line weight
		//	overrideSettings.SetSurfaceTransparency(50);         // Set transparency (optional)

		//	// Get the transform of the linked model
		//	Transform linkTransform = linkInstance.GetTransform();

		//	// Apply the override to the linked element in the view
		//	activeView.SetElementOverrides(elementId, overrideSettings);
		//}

		//private Material CreateOrGetmaterial(Document doc, string materialName, Color color)
		//{
		//	// Check if the material already exists
		//	FilteredElementCollector materialCollector = new FilteredElementCollector(doc).OfClass(typeof(Material));
		//	Material material = materialCollector.FirstOrDefault(c => c.Name == materialName) as Material;	

		//	if(material == null)
		//	{
		//		// Create a new material
		//		using(Transaction trans = new Transaction(doc, "Create Material"))
		//		{
		//			trans.Start();

		//			var materialId =  Material.Create(doc, materialName);
		//			material = doc.GetElement(materialId) as Material;
		//			material.Color = color;
		//			material.Transparency = 50;

		//			trans.Commit();
		//		}
		//	}

		//	return material;
		//}

		//private BoundingBoxXYZ TransformBoundingBox(BoundingBoxXYZ bbox, Transform transform)
		//{
		//	if (bbox == null) return null;

		//	return new BoundingBoxXYZ
		//	{
		//		Min = transform.OfPoint(bbox.Min),
		//		Max = transform.OfPoint(bbox.Max)
		//	};
		//}

		//private Solid CreateBoundingBoxGeometry(BoundingBoxXYZ bbox)
		//{
		//	XYZ min = bbox.Min;
		//	XYZ max = bbox.Max;

		//	// Define the 4 base edges of the bounding box as curves
		//	List<Curve> baseCurves = new List<Curve>
		//{
		//		Line.CreateBound(new XYZ(min.X, min.Y, min.Z), new XYZ(max.X, min.Y, min.Z)),
		//		Line.CreateBound(new XYZ(max.X, min.Y, min.Z), new XYZ(max.X, max.Y, min.Z)),
		//		Line.CreateBound(new XYZ(max.X, max.Y, min.Z), new XYZ(min.X, max.Y, min.Z)),
		//		Line.CreateBound(new XYZ(min.X, max.Y, min.Z), new XYZ(min.X, min.Y, min.Z))
		//};

		//	// Create a Curveloop from the base curves
		//	CurveLoop baseLoop = CurveLoop.Create(baseCurves);

		//	// Add the CurveLoop to a list, as a required by CreateExtrusionGeometry
		//	IList<CurveLoop> curveLoops = new List<CurveLoop>() { baseLoop };

		//	// Specifiy the extrusion direction (Z-axis) and height
		//	XYZ extrusionDirection = XYZ.BasisZ;
		//	double extrusionHeight = max.Z - min.Z;

		//	// Create the extrusion geometry
		//	Solid extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, extrusionDirection, extrusionHeight);

		//	return extrusion;
		//}
	}
}
