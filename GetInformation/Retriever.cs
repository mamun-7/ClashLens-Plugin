using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ClashSolver.Utils;

namespace ClashSolver
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class Information : IExternalCommand
  {
		public static Family Marker = null;

		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			UIDocument uidoc = commandData.Application.ActiveUIDocument;
			Document doc = uidoc.Document;

			Element elementA = null, elementB = null;

			foreach(var id in uiApp.ActiveUIDocument.Selection.GetElementIds())
			{
				elementA = doc.GetElement(id);
				break;
			}

			if (!CheckFamily(doc, "Clash Marker 3D"))
			{
				return Result.Failed;
			}

			if(elementA == null)
			{
				TaskDialog.Show("Error", "Please select the elements");

				return Result.Cancelled;
			}

			long pipeCategoryId = -2008044;


			// For link model
			var linkModels = RevitHelper.GetLinkedProjects(doc);
			ElementId linkInstanceId = new ElementId(linkModels.First().InstanceId);
			RevitLinkInstance linkInstance = doc.GetElement(linkInstanceId) as RevitLinkInstance;

			List<Element> clashedElements = RevitHelper.FindClashes(elementA, new ElementId(pipeCategoryId), linkInstance);

			// Find the intersection solid

			using (Transaction trans = new Transaction(doc, "Place Marker Family"))
			{
				trans.Start();

				foreach (var element in clashedElements)
				{
					if(element is Pipe pipe)
					{
						elementB = pipe;

						Solid intersectionSolid = RevitHelper.GetIntersection(elementA, elementB, linkInstance);

						BoundingBoxXYZ bbox = intersectionSolid.GetBoundingBox();

						Face bestFace = GetLargestFlatFace(intersectionSolid);
						if ((bestFace == null))
						{
							TaskDialog.Show("Error", "No suitable face found for placing the family");
							return Result.Failed;
						}

						var familySymbol = GetFamilySymbol(doc, Marker);

						if (!familySymbol.IsActive)
						{
							familySymbol.Activate();
							doc.Regenerate();
						}

						// Create a new face-based family instance
						Reference faceReference = bestFace.Reference;

						XYZ placementPoint = GetFaceCenter(bestFace);

						FamilyInstance instance = doc.Create.NewFamilyInstance(intersectionSolid.ComputeCentroid(), familySymbol, StructuralType.NonStructural);

						double boxHeight = bbox.Max.Z - bbox.Min.Z;
						double boxWidth = bbox.Max.Y - bbox.Min.Y;

						Parameter param = instance.LookupParameter("Clash Number");
						param?.Set($"Clash {instance.Id}");

						param = instance.LookupParameter("Top Offset");
						double topOffset = Util.IUToMm(boxHeight) / 2;
						param?.Set(Util.MmToIU(topOffset + 50));

						param = instance.LookupParameter("Side Offset");
						double sideOffset = Util.IUToMm(boxWidth) / 2;
						param?.Set(Util.MmToIU(sideOffset + 50));


						//View activeView = doc.ActiveView;

						//BoundingBoxXYZ bbox = instance.get_BoundingBox(null);
						//if (bbox == null)
						//	return Result.Failed;

						//using (Transaction tx = new Transaction(doc, "Select Family Instance"))
						//{
						//	tx.Start();
						//	uidoc.Selection.SetElementIds(new List<ElementId> { instance.Id });

						//	RevitHelper.ZoomToBoundingBox(doc, bbox);
						//	tx.Commit();
						//}

					}
				}

				trans.Commit();
			}


			//Create3DModelText(doc, "clash", wallMid, 0.05);

			return Result.Succeeded;
		}
		public ElementId GetElementIdFromHostOrLink(Document doc, Element element)
		{
			if(IsElementInDocument(doc, element))
			{
				return element.Id;
			}

			// If not found in the host document, search in linked documents
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			ICollection<Element> linkedInstances = collector.OfClass(typeof(RevitLinkInstance)).ToElements();

			foreach (Element e in linkedInstances)
			{
				RevitLinkInstance linkInstance = e as RevitLinkInstance;
				if (linkInstance != null)
				{
					Document linkDoc = linkInstance.GetLinkDocument();
					if (linkDoc != null)
					{
						if (IsElementInDocument(linkDoc, element))
						{
							return element.Id;
						}
					}
				}
			}

			// Return null if the element is not found
			return null;
		}

		public bool IsElementInDocument(Document doc, Element element)
		{
			return element.Document.Equals(doc);
		}

		private bool CheckFamily(Document doc, string name)
		{
			List<Family> families1 = new List<Family>(
							new FilteredElementCollector(doc)
								.WhereElementIsNotElementType()
								.OfClass(typeof(Family))
								.Where(ins => ins.Name == name)
								.ToList()
								.Cast<Family>()
								);
			List<Family> families = families1;

			if (families.Count > 0)
			{
				Marker = families[0];
				return true;
			}

			Transaction trans = new Transaction(doc, "Load Family");
			trans.Start("Load 3D Marker Family");
			try
			{
				string url = Assembly.GetExecutingAssembly().Location;
				url = url.Substring(0, url.LastIndexOf("\\")) + "\\";

				bool bRet = Marker != null || doc.LoadFamily(url + name + ".rfa", out Marker);
				if (bRet)
				{
					trans.Commit();
					return true;
				}

				trans.RollBack();
				return false;
			}
			catch (Exception)
			{
				trans.RollBack();
				MessageBox.Show("Can not find the family. Please contact the developer", "Error");
				return false;
			}
		}

		private FamilySymbol GetFamilySymbol(Document doc, Family family)
		{
			ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();

			foreach (ElementId eId in familySymbolIds)
			{
				FamilySymbol familySymbol = doc.GetElement(eId) as FamilySymbol;
				if (!familySymbol.IsActive)
					familySymbol.Activate();
				return familySymbol;
			}
			return null;
		}

		private Face GetLargestFlatFace(Solid solid)
		{
			Face largestFace = null;
			double maxArea = 0;

			foreach(Face face in solid.Faces)
			{
				if (face is PlanarFace planarFace)
				{
					double faceArea = planarFace.Area;
					if (faceArea > maxArea)
					{
						maxArea = faceArea;
						largestFace = planarFace;
					}
				}
			}

			return largestFace;
		}

		/// <summary>
		/// Gets the center point of a face
		/// </summary>
		/// <param name="face"></param>
		/// <returns></returns>
		private XYZ GetFaceCenter(Face face)
		{
			BoundingBoxUV bbox = face.GetBoundingBox();
			UV midParam = (bbox.Min + bbox.Max) / 2;
			return face.Evaluate(midParam);
		}
	}
}
