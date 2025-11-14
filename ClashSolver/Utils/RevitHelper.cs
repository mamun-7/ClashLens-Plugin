using Architexor.Core;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using ClashSolver.Models;
using ClashSolver.UI;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Issue = ClashSolver.UI.Models.Issue;

namespace ClashSolver.Utils
{
	public class RevitHelper
	{
		private const double tolerance = 1e-3;

		#region Geometry

		private static Solid GetElementSolid(Element element)
		{
			Options options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true };

			// Get geometry
			GeometryElement geom = element.get_Geometry(options);

			foreach (GeometryObject geoObject in geom)
			{
				if (geoObject is Solid solid && solid.Volume > tolerance)
				{
					return solid;
				}
			}

			return null; // no valid solid found
		}

		public static List<Solid> GetCurvesFromABeam(FamilyInstance beam)
		{
			// Options for retrieving geometry
			Options options = new Options
			{
				ComputeReferences = true,
				IncludeNonVisibleObjects = true
			};

			GeometryElement geomElem = beam.get_Geometry(options);

			CurveArray curves = new CurveArray();
			List<Solid> solids = new List<Solid>();

			//Find all solids and insert them into solid array
			AddCurvesAndSolids(geomElem, ref curves, ref solids);

			return solids;
		}

		private static void AddCurvesAndSolids(GeometryElement geomElem, ref CurveArray curves, ref List<Solid> solids)
		{
			foreach (GeometryObject geomObj in geomElem)
			{
				Curve curve = geomObj as Curve;
				if (null != curve)
				{
					curves.Append(curve);
					continue;
				}
				Solid solid = geomObj as Solid;
				if (null != solid)
				{
					solids.Add(solid);
					continue;
				}
				//If this GeometryObject is Instance, call AddCurvesAndSolids
				GeometryInstance geomInst = geomObj as GeometryInstance;
				if (null != geomInst)
				{
					GeometryElement transformedGeomElem
						= geomInst.GetInstanceGeometry(geomInst.Transform);
					AddCurvesAndSolids(transformedGeomElem, ref curves, ref solids);
				}
			}
		}

		public static Solid GetSolidFromFamilyInstance(FamilyInstance familyInstance)
		{
			if (familyInstance == null)
				return null;

			// Options for retrieving geometry
			Options options = new Options
			{
				ComputeReferences = true,
				IncludeNonVisibleObjects = true
			};

			// Get the geometry of the FamilyInstance
			GeometryElement geometryElement = familyInstance.get_Geometry(options);
			if (geometryElement == null)
				return null;

			foreach (GeometryObject geometryObject in geometryElement)
			{
				if (geometryObject is Solid solid && solid.Volume > 0)
				{
					// Return the solid if found
					return solid;
				}
				else if (geometryObject is GeometryInstance geometryInstance)
				{
					// Get the instance geometry
					GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();

					foreach (GeometryObject instanceObject in instanceGeometry)
					{
						if (instanceObject is Solid instanceSolid && instanceSolid.Volume > 0)
						{
							return instanceSolid;
						}
					}
				}
			}

			// No valid solid found
			return null;
		}

		public static Transform GetTransform(FamilyInstance familyInstance)
		{
			if (familyInstance == null)
				return null;

			// Options for retrieving geometry
			Options options = new Options
			{
				ComputeReferences = true,
				IncludeNonVisibleObjects = true
			};

			// Get the geometry of the FamilyInstance
			GeometryElement geometryElement = familyInstance.get_Geometry(options);
			if (geometryElement == null)
				return null;

			foreach (GeometryObject geometryObject in geometryElement)
			{
				if (geometryObject is GeometryInstance geometryInstance)
				{
					// Get the instance geometry
					GeometryElement instanceGeometry = geometryInstance.GetInstanceGeometry();
					Transform instanceTransform = geometryInstance.Transform;

					return instanceTransform;
				}
			}

			// No valid solid found
			return null;
		}

		public static XYZ GetGlobalPoint(Element element, XYZ point)
		{
			// Create a rectangular curve loop for the opening
			Transform transform = Transform.Identity;
			if (element is FamilyInstance familyInstance)
			{
				transform = familyInstance.GetTransform();
			}
			else if (element.Location is LocationPoint locPoint)
			{
				transform = Transform.CreateTranslation(locPoint.Point);
			}
			else if (element.Location is LocationCurve locCurve)
			{
				transform = Transform.CreateTranslation(locCurve.Curve.Evaluate(0.5, true));
			}

			return transform.OfPoint(point);
		}

		private static BoundingBoxXYZ CombineBoundingBoxes(BoundingBoxXYZ bbox1, BoundingBoxXYZ bbox2)
		{
			if (bbox1 == null || bbox2 == null) return null;

			return new BoundingBoxXYZ
			{
				Min = new XYZ(
							Math.Min(bbox1.Min.X, bbox2.Min.X),
							Math.Min(bbox1.Min.Y, bbox2.Min.Y),
							Math.Min(bbox1.Min.Z, bbox2.Min.Z)),
				Max = new XYZ(
							Math.Max(bbox1.Max.X, bbox2.Max.X),
							Math.Max(bbox1.Max.Y, bbox2.Max.Y),
							Math.Max(bbox1.Max.Z, bbox2.Max.Z))
			};
		}

		public static BoundingBoxXYZ GetBoundingBox(Element element)
		{
			Document doc = element.Document;

			View view = doc.ActiveView;
			BoundingBoxXYZ boundingBox = element.get_BoundingBox(view);

			return boundingBox;
		}

		public static double CalculateAngleBetweenPipeAndHost(Document doc, Element pipeElement, Element hostElement)
		{
			// Get the pipe element
			LocationCurve pipeLocation = pipeElement.Location as LocationCurve;
			if (pipeLocation == null)
			{
				throw new InvalidOperationException("Pipe element does not have a valid location curve.");
			}

			// Get the direction vector of the pipe
			XYZ pipeDirection = (pipeLocation.Curve.GetEndPoint(1) - pipeLocation.Curve.GetEndPoint(0)).Normalize();

			// Get the host element
			XYZ hostNormal = null;

			// Determine the normal vector of the host element
			if (hostElement is Floor)
			{
				hostNormal = XYZ.BasisZ; // For floors, the normal is typically the Z-axis
			}
			else if (hostElement is Wall)
			{
				hostNormal = (hostElement as Wall).Orientation; // For walls, use the wall's orientation
			}
			else if (hostElement is Ceiling)
			{
				hostNormal = XYZ.BasisZ;
			}
			else
			{
				hostNormal = XYZ.BasisZ;
			}

			// Calculate the angle between the pipe direction and the host normal
			double dotProduct = pipeDirection.DotProduct(hostNormal);
			double angle = Math.Acos(dotProduct) * (180.0 / Math.PI); // Convert radians to degrees

			// Ensure the angle is between 0 and 90 degrees
			if (angle > 90)
			{
				angle = 180 - angle;
			}

			return angle;
		}

		private static bool AABBsIntersect(XYZ minA, XYZ maxA, XYZ minB, XYZ maxB)
		{
			return (minA.X <= maxB.X && maxA.X >= minB.X) &&
						 (minA.Y <= maxB.Y && maxA.Y >= minB.Y) &&
						 (minA.Z <= maxB.Z && maxA.Z >= minB.Z);
		}

		private static bool BoxesIntersect(XYZ minA, XYZ maxA, XYZ minB, XYZ maxB)
		{
			return (IsPointInsideBoundingBox(minA, minB, maxB) || IsPointInsideBoundingBox(maxA, minB, maxB) ||
							IsPointInsideBoundingBox(minB, minA, maxA) || IsPointInsideBoundingBox(maxB, minA, maxA));
		}

		private static bool IsPointInsideBoundingBox(XYZ point, XYZ bboxMin, XYZ bboxMax)
		{
			return (point.X >= bboxMin.X && point.X <= bboxMax.X &&
							point.Y >= bboxMin.Y && point.Y <= bboxMax.Y &&
							point.Z >= bboxMin.Z && point.Z <= bboxMax.Z);
		}

		private static BoundingBoxXYZ GetBoundingBoxFromGeometry(Element element)
		{
			GeometryElement geomElement = element.get_Geometry(new Options());
			if (geomElement == null) return null;

			BoundingBoxXYZ boundingBox = new BoundingBoxXYZ();
			bool hasGeometry = false;

			foreach (GeometryObject geomObj in geomElement)
			{
				if (geomObj is Solid solid && solid.Volume > 0)
				{
					if (!hasGeometry)
					{
						boundingBox.Min = solid.GetBoundingBox().Min;
						boundingBox.Max = solid.GetBoundingBox().Max;
						hasGeometry = true;
					}
					else
					{
						boundingBox.Min = new XYZ(
							Math.Min(boundingBox.Min.X, solid.GetBoundingBox().Min.X),
							Math.Min(boundingBox.Min.Y, solid.GetBoundingBox().Min.Y),
							Math.Min(boundingBox.Min.Z, solid.GetBoundingBox().Min.Z)
						);
						boundingBox.Max = new XYZ(
							Math.Max(boundingBox.Max.X, solid.GetBoundingBox().Max.X),
							Math.Max(boundingBox.Max.Y, solid.GetBoundingBox().Max.Y),
							Math.Max(boundingBox.Max.Z, solid.GetBoundingBox().Max.Z)
						);
					}
				}
			}

			if (!hasGeometry) return null;

			return boundingBox;
		}

		public static BoundingBoxXYZ TransformBoundingBox(BoundingBoxXYZ boundingBox, Transform transform)
		{
			if (boundingBox != null)
			{
				BoundingBoxXYZ transformedBox = new BoundingBoxXYZ()
				{
					Min = transform.OfPoint(boundingBox.Min),
					Max = transform.OfPoint(boundingBox.Max)
				};
				return transformedBox;

			}

			return null;
		}

		public static Solid GetSolidFromElement(Element element)
		{
			GeometryElement geomElement = element.get_Geometry(new Options());
			if (geomElement == null) return null;

			foreach (GeometryObject geomObj in geomElement)
			{
				if (geomObj is Solid solid && solid.Volume > 0)
				{
					return solid;
				}
			}

			return null;
		}

		public static Outline GetOutlineFromElement(Element element)
		{
			// Get the bounding box of element A
			BoundingBoxXYZ bbox = element.get_BoundingBox(null);
			if (bbox == null) return null;

			Outline outline = new Outline(bbox.Min, bbox.Max);

			return outline;
		}

		public static bool IsBoundingBoxIntersecting(BoundingBoxXYZ sectionBox, BoundingBoxXYZ elementBoundingBox)
		{
			XYZ min1 = sectionBox.Min;
			XYZ max1 = sectionBox.Max;
			XYZ min2 = elementBoundingBox.Min;
			XYZ max2 = elementBoundingBox.Max;

			// Check if the bounding boxes intersect in all three axes(X, Y, Z)
			bool isIntersecting = (min1.X <= max2.X && max1.X >= min2.X) &&
				(min1.Y <= max2.Y && max1.Y >= min2.Y) &&
				(min1.Z <= max2.Z && max1.Z >= min2.Z);

			// Check if the element's bounding box is completley inside the section box
			bool isInside = (min1.X <= min2.X && max1.X >= max2.X) &&
					(min1.Y <= min2.Y && max1.Y >= max2.Y) &&
					(min1.Z <= min2.Z && max1.Z >= max2.Z);

			return isIntersecting || isInside;
		}
		#endregion

		#region Retrieve Categories from Document

		public static List<CSCategory> GetElementCategories(Document doc)
		{
			List<CSCategory> res = [];

			Categories categories = doc.Settings.Categories;

			foreach (var categoryObj in categories)
			{
				if (categoryObj is Category category && category.CategoryType == CategoryType.Model)
				{
					res.Add(new CSCategory()
					{
#if REVIT2024 || REVIT2025
						ElementId = category.Id.Value,
#else
						ElementId = category.Id.IntegerValue,
#endif
						Name = category.Name,
						Type = category.CategoryType.ToString(),
						Version = doc.Application.VersionNumber
					});
				}
			}

			res.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));

			return res;
		}

		public static List<long> GetUsedCategoryElementIds(Document doc)
		{
			List<long> res = new List<long>();

			string[] elemCategories = ["Air Terminals", "Bridge Cables", "Bridge Decks", "Bridge Framing", "Casework", "Ceilings", "Columns", "Conduit Fittings", "Conduit Runs", "Conduits", "Doors", "Duct Accessories", "Duct Fittings", "Duct Insulations", "Duct Linings", "Ducts", "Electrical Circuits", "Electrical Equipment", "Electrical Fixtures", "Fire Alarm Devices", "Fire Protection", "Flex Ducts", "Flex Pipes", "Floors", "Furniture", "Generic Models", "Lighting Devices", "Lighting Fixtures", "Mechanical Control Devices", "Mechanical Equipment", "Mechanical Equipment Sets", "Medical Equipment", "MEP Fabrication Containment", "MEP Fabrication Ductwork", "MEP Fabrication Ductwork Stiffeners", "MEP Fabrication Hangers", "MEP Fabrication Pipework", "Pipe Accessories", "Pipe Fittings", "Pipe Insulations", "Pipe Placeholders", "Pipe Segments", "Pipes", "Planting", "Plumbing Equipment", "Plumbing Fixtures", "Railings", "Ramps", "Roofs", "Site", "Specialty Equipment", "Stairs", "Structural Area Reinforcement", "Structural Beam Systems", "Structural Columns", "Structural Connections", "Structural Fabric Areas", "Structural Fabric Reinforcement", "Structural Foundations", "Structural Framing", "Structural Path Reinforcement", "Structural Rebar", "Structural Rebar Couplers", "Structural Stiffeners", "Structural Tendons", "Structural Trusses", "Telephone Devices", "Temporary Structures", "Vertical Circulation", "Vibration Management", "Walls", "Windows"];
			Categories categories = doc.Settings.Categories;

			foreach (var categoryObj in categories)
			{
				var category = categoryObj as Category;

				// Get Model Categories Except Walls, Detail Items
				if (category != null && category.CategoryType == CategoryType.Model && elemCategories.Contains(category.Name))
				{
					// Use a FilteredElementColletor to check if there are any elements in the category
					var collector = new FilteredElementCollector(doc).OfCategoryId(category.Id).WhereElementIsNotElementType();

					if (collector.GetElementCount() > 0) // Category has elements
					{
						res.Add(category.Id.Value);
					}
				}
			}

			return res;
		}

		public static ElementId GetCategoryIdByName(Document doc, string name)
		{
			var categories = doc.Settings.Categories;

			foreach (var obj in categories)
			{
				if (obj is Category category && category.Name == name)
				{
					return category.Id;
				}
			}

			return null;
		}

		#endregion

		#region Retrieve Families from Document

		public static List<SelectableItem> GetUsedFamilyIds(Document doc, ElementId categoryId, List<string> ignoreFamilyNames = null)
		{
			// Initialize a hash set to store unique family IDs
			var res = new List<SelectableItem>();

			// Collect all elements in the specified category
			FilteredElementCollector collector = new FilteredElementCollector(doc)
				.OfCategoryId(categoryId)
				.WhereElementIsNotElementType();

			// Iterate through the elements and retrieve their Family IDs
			foreach (Element element in collector)
			{
				var family = GetFamily(element);

				// If a valid family symbol is found, add its Family ID to the set
				if (family != null && res.Where(x => x.Id == family.Id.Value).Count() == 0)
				{
					res.Add(new SelectableItem()
					{
						Id = family.Id.Value,
						Name = family.Name,
						IsSelected = false
					});
				}
			}

			return res;
		}
		private static Family GetFamily(Element element)
		{
			if (element == null)
			{
				return null;
			}

			// Check if the element is a FamilyInstance
			if (element is FamilyInstance familyInstance)
			{
				return familyInstance.Symbol.Family;
			}

			// For other elements, get the ElementType and then the Family
			ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
			if (elementType != null)
			{
				// Retrieve the family using the FamilyName property
				FilteredElementCollector collector = new FilteredElementCollector(element.Document)
					.OfClass(typeof(Family))
					.WhereElementIsElementType();

				foreach (Family family in collector)
				{
					if (family.Name == elementType.FamilyName)
					{
						return family;
					}
				}
			}

			return null;
		}

		public static HashSet<ElementId> GetUsedFamilyIds(Document doc, BuiltInCategory category)
		{
			// Initialize a hash set to store unique family IDs
			HashSet<ElementId> familyIds = new HashSet<ElementId>();

			// Collect all elements in the specified category
			FilteredElementCollector collector = new FilteredElementCollector(doc)
				.OfCategory(category)
				.WhereElementIsNotElementType();

			// Iterate through the elements and retrieve their Family IDs
			foreach (Element element in collector)
			{
				FamilySymbol symbol = null;

				if (element is FamilyInstance familyInstance)
				{
					// Get the family symbol of the instance
					symbol = familyInstance.Symbol;
				}
				else if (element is MEPCurve mepCurve)
				{
					// Get the type of the MEP element, which has the family information
					symbol = doc.GetElement(mepCurve.GetTypeId()) as FamilySymbol;
				}

				// If a valid family symbol is found, add its Family ID to the set
				if (symbol != null)
				{
					familyIds.Add(symbol.Family.Id);
				}
			}

			return familyIds;
		}

		public static List<ElementId> GetSystemTypeIds(Document linkedDoc)
		{
			// Collect all piping system types in the linked document
			FilteredElementCollector collector = new FilteredElementCollector(linkedDoc)
				.OfClass(typeof(MEPSystemType));

			List<ElementId> systemTypeIds = new List<ElementId>();
			foreach (Element element in collector)
			{
				systemTypeIds.Add(element.Id);
			}

			return systemTypeIds;
		}

		#endregion

		#region Retrieve Elements from Document

		public static Element GetElement(Document doc, long elementId, LinkedModel linkModel = null)
		{
			if(elementId < 0)
			{
				return null;
			}

			Element element = null;

			if (linkModel != null && linkModel.InstanceId > 0)
			{
				if (doc.GetElement(new ElementId(linkModel.InstanceId)) is RevitLinkInstance linkInstanceA)
				{
					Document linkDocA = linkInstanceA.GetLinkDocument();
					element = linkDocA.GetElement(new ElementId(elementId));
				}
			}
			else
			{
				element = doc.GetElement(new ElementId(elementId));
			}

			return element;
		}

		public static List<Element> GetElementsInSectionBox(Document doc, View3D view)
		{
			// list to store elements within the SectionBox
			List<Element> elementsInSectionBox = new List<Element>();

			// Check if the view is a 3D view and has a SectionBox
			if (view != null && view.ViewType == ViewType.ThreeD)
			{
				BoundingBoxXYZ sectionBox = view.GetSectionBox();

				if (sectionBox != null)
				{
					XYZ min = sectionBox.Min;
					XYZ max = sectionBox.Max;

					// Create a FilteredElementCollecor to get all elements in the document
					FilteredElementCollector collector = new FilteredElementCollector(doc);

					// Iterate through all elements in the document
					foreach (Element element in collector)
					{
						// Get the bounding box of the current element
						BoundingBoxXYZ elementBoundingBox = element.get_BoundingBox(null);
						if (elementBoundingBox != null)
						{
							XYZ elementMin = elementBoundingBox.Min;
							XYZ elementMax = elementBoundingBox.Max;

							// Check if the element's bounding box intersects the SectionBox
							if (IsBoundingBoxIntersecting(sectionBox, elementBoundingBox))
							{
								elementsInSectionBox.Add(element);
							}
						}
					}
				}
			}

			return elementsInSectionBox;
		}

		public static List<ElementId> GetElementsByCategoryName(Document doc, string name)
		{
			List<ElementId> res = new List<ElementId>();

			var categories = doc.Settings.Categories;

			foreach (var obj in categories)
			{
				if (obj is Category category)
				{
					// Create a filtered element collector
					FilteredElementCollector collector = new FilteredElementCollector(doc);

					// Retrieve elements
					var elemIds = collector.OfCategoryId(category.Id)
										.WhereElementIsNotElementType()
										.ToElementIds();

					foreach (var id in elemIds)
					{
						res.Add(id);
					}
				}
			}

			return res;
		}

		public static List<ElementId> GetElementsByCategoryId(Document doc, ElementId categoryId)
		{
			List<ElementId> res = new List<ElementId>();

			var categories = doc.Settings.Categories;

			foreach (var obj in categories)
			{
				if (obj is Category category && category.Id == categoryId)
				{
					// Create a filtered element collector
					FilteredElementCollector collector = new FilteredElementCollector(doc);

					// Retrieve elements
					var elemIds = collector.OfCategoryId(category.Id)
										.WhereElementIsNotElementType()
										.ToElementIds();

					foreach (var id in elemIds)
					{
						res.Add(id);
					}
				}
			}

			return res;
		}

		public static List<long> GetElementsByCategoryId(Document doc, long nCategoryId)
		{
			List<long> res = new List<long>();

			var categories = doc.Settings.Categories;

			ElementId categoryId = new ElementId(nCategoryId);

			foreach (var obj in categories)
			{
				var category = obj as Category;

				if (category.Id == categoryId)
				{
					// Create a filtered element collector
					FilteredElementCollector collector = new FilteredElementCollector(doc);

					// Retrieve elements
					var elemIds = collector.OfCategoryId(category.Id)
										.WhereElementIsNotElementType()
										.ToElementIds();

					foreach (var id in elemIds)
					{
#if REVIT2024 || REVIT2025
						res.Add(id.Value);
#else
						res.Add(id.IntegerValue);
#endif
					}
				}
			}

			return res;
		}

		public static List<FamilyInstance> GetElementsByFamilyName(Document doc, string familyName = "")
		{
			// Collect all FamilyInstance elements in the project
			FilteredElementCollector collector = new FilteredElementCollector(doc)
				.OfClass(typeof(FamilyInstance));

			// Filter by Family Name
			List<FamilyInstance> clashInstances = collector
				.Cast<FamilyInstance>()
				.Where(x => x.Symbol.Name == familyName)
				.ToList();

			return clashInstances;
		}

		public static Element GetElementFromHostOrLinkedDoc(Document hostDoc, ElementId elementId, out Transform transform)
		{
			transform = Transform.Identity;

			// Try to get the element from the host document
			Element element = hostDoc.GetElement(elementId);
			if (element != null) return element;

			// If not in host, search in linked documents
			FilteredElementCollector linkInstances = new FilteredElementCollector(hostDoc).OfClass(typeof(RevitLinkInstance));
			foreach (RevitLinkInstance linkInstance in linkInstances)
			{
				Document linkedDoc = linkInstance.GetLinkDocument();
				if (linkedDoc == null) continue;

				Element linkedElement = linkedDoc.GetElement(elementId);
				if (linkedElement != null)
				{
					transform = linkInstance.GetTransform();
					return linkedElement;
				}
			}

			return null;
		}

		public static List<TargetElement> GetHostElementsInSectionBox(Document doc, BoundingBoxXYZ sectionBox)
		{
			List<TargetElement> res = new List<TargetElement>();

			try
			{
				var elements = new FilteredElementCollector(doc).WhereElementIsNotElementType().Where(x => x.get_Geometry(new Options()) != null);

				foreach (Element element in elements)
				{
					if (element.Category == null) continue;

					// Get the bouning box of the linked element
					BoundingBoxXYZ elementBoundingBox = element.get_BoundingBox(null);
					if (elementBoundingBox == null)
					{
						elementBoundingBox = GetBoundingBoxFromGeometry(element);

						if (elementBoundingBox == null)
						{
							continue;
						}
					}
					// Check if the element is within the section box
					if (IsBoundingBoxIntersecting(sectionBox, elementBoundingBox))
					{
						res.Add(new TargetElement()
						{
							Id = element.Id,
							IsLinkedElement = false,
						});
					}

				}
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("QuickDetectionController::GetHostElementsInSectionBox => ", ex);
			}

			return res;
		}

		/// <summary>
		/// Get elements in the host document inside the Bounding Box
		/// </summary>
		/// <param name="doc">Host Document</param>
		/// <param name="bbox">BoundingBox which detects eleemnts</param>
		/// <returns></returns>
		public static List<Element> GetElementsInBoundingBox(Document doc, BoundingBoxXYZ bbox)
		{
			List<Element> elements = new List<Element>();

			if (bbox == null)
				return elements;

			// Use BoundingBoxIsInsideFilter to efficiently filter elements inside the scope box
			var outline = new Outline(bbox.Min, bbox.Max);
			BoundingBoxIsInsideFilter filter1 = new BoundingBoxIsInsideFilter(outline);
			BoundingBoxIntersectsFilter filter2 = new BoundingBoxIntersectsFilter(outline);
			LogicalOrFilter filter = new LogicalOrFilter(filter1, filter2);
			FilteredElementCollector collector = new FilteredElementCollector(doc)
					.WhereElementIsNotElementType().WherePasses(filter);

			elements = [.. collector];

			return elements;
		}

		/// <summary>
		/// Get elements in the linked document inside the Bounding Box.
		/// </summary>
		/// <param name="doc">Host document which link instances reside.</param>
		/// <param name="sectionBox">BoundingBox which detects elements.</param>
		/// <param name="instanceIds">Link instances</param>
		/// <returns></returns>
		public static List<TargetElement> GetElementsInBoundingBox(Document doc, BoundingBoxXYZ sectionBox, List<long> instanceIds)
		{
			List<TargetElement> res = new List<TargetElement>();

			// Get section box and transform it to world coordinates
			Transform sectionTransform = doc.ActiveView.CropBox.Transform;
			(XYZ sectionMin, XYZ sectionMax) = GetTransformedBoundingBoxMinMax(sectionBox, sectionTransform);

			foreach (var linkInstanceId in instanceIds)
			{
				var linkInstance = doc.GetElement(new ElementId(linkInstanceId)) as RevitLinkInstance;

				// Get the transform of the linked model
				Transform linkTransform = linkInstance.GetTotalTransform();

				// Get the linked model document
				Document linkedDoc = linkInstance.GetLinkDocument();
				if (linkedDoc == null)
					continue;

				try
				{
					var elements = new FilteredElementCollector(linkedDoc).WhereElementIsNotElementType().ToElements();

					foreach (Element element in elements)
					{
						if (element.Category == null) continue;

						// Get the bouning box of the linked element
						BoundingBoxXYZ elementBox = element.get_BoundingBox(null);
						if (elementBox == null)
							continue;

						// Transform bounding box to host document coordinate system
						(XYZ elemMin, XYZ elemMax) = GetTransformedBoundingBoxMinMax(elementBox, linkTransform);

						// **Optimized AABB Check** (Fast exit if no overlap)
						if (!AABBsIntersect(sectionMin, sectionMax, elemMin, elemMax))
							continue;

						// **Detailed Intersection Check** (Only needed if AABB check passes)
						if (BoxesIntersect(sectionMin, sectionMax, elemMin, elemMax))
						{
							res.Add(new TargetElement()
							{
								Id = element.Id,
								LinkModelId = linkInstance.Id,
								IsLinkedElement = true,
							});
						}

					}
				}
				catch (Exception ex)
				{
					TraceLogger.Instance.ExceptionLog("QuickDetectionController::GetHostElementsInSectionBox => ", ex);
				}

			}

			return res;
		}

		public static string GetMaterialName(Element element)
		{
			// Get the material Ids from the element
			ICollection<ElementId> materialIds = element.GetMaterialIds(false);

			// if the element has materials, get the first material's name
			if (materialIds.Count > 0)
			{
				Material material = element.Document.GetElement(materialIds.First()) as Material;
				if (material != null)
				{
					return material.Name;
				}
			}

			return "No material found";
		}

		public static List<ElementId> GetRelatedElements(Element element)
		{
			List<ElementId> relatedElementIds = new List<ElementId>();

			// Get the connectors of the element
			ConnectorManager connectorManager = GetConnectorManager(element);
			if (connectorManager != null)
			{
				foreach (Connector connector in connectorManager.Connectors)
				{
					// Get the connected elements
					if (connector.IsConnected)
					{
						foreach (Connector connectedConnector in connector.AllRefs)
						{
							if (connectedConnector.Owner.Id != element.Id)
							{
								relatedElementIds.Add(connectedConnector.Owner.Id);
							}
						}
					}
				}
			}

			// Add other logic to find related elements (e.g., hosted elements, family instances, etc.)
			if (element is FamilyInstance familyInstance)
			{
				var host = familyInstance.Host;
				if (host != null)
				{
					relatedElementIds.Add(host.Id);
				}

				var hostedElements = new FilteredElementCollector(element.Document)
					.WherePasses(new ElementIntersectsElementFilter(element))
					.Where(x => x is FamilyInstance && ((FamilyInstance)x).Host != null && ((FamilyInstance)x).Host.Id == element.Id)
					.Select(element => element.Id).ToList();

				relatedElementIds.AddRange(hostedElements);
			}

			return relatedElementIds;
		}
		
		#endregion

		#region Get Information of Linked Models

		public static List<LinkedModel> GetLinkedProjects(Document doc)
		{
			List<LinkedModel> models = new List<LinkedModel>();

			ICollection<ElementId> collection = ExternalFileUtils.GetAllExternalFileReferences(doc);
			int nNo = 1;

			List<CSCategory> res = new List<CSCategory>();
			var collector = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).ToList();

			foreach (var obj in collector)
			{
				if (obj is RevitLinkInstance linkInstance)
				{
					RevitLinkType linkType = doc.GetElement(linkInstance.GetTypeId()) as RevitLinkType;

					Document linkedDoc = linkInstance.GetLinkDocument();

					string url = "";

					if (linkedDoc == null) continue;
					url = linkedDoc.PathName;

					string name = linkType.Name;

					LinkedModel linkedModel = new LinkedModel()
					{
						No = nNo++,
						ElementId = linkType.Id.Value,
						InstanceId = linkInstance.Id.Value,
						Name = linkType.Name,
						Url = url,
						Discipline = LinkDiscipline.None,
						Description = ""
					};

					models.Add(linkedModel);
				}

			}

			return models;
		}

		public static List<LinkedModel> GetLinkedProjectsFromDB(long projectId)
		{
			var models = new List<LinkedModel>();
			foreach (var obj in LinkModelTableAdapter.Instance.GetByProjectId(projectId))
			{
				if (obj is LinkedModel model)
				{
					models.Add(model);
				}
			}

			return models;
		}

		public static string GetLinkedModelPath(Document doc, RevitLinkType linkType)
		{
			if (linkType != null)
			{
				// Get the external file reference
				ExternalFileReference externalFileReference = linkType.GetExternalFileReference();

				if (externalFileReference != null)
				{
					// Convert the model path to a user-visible path
					ModelPath modelPath = externalFileReference.GetAbsolutePath();
					string path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

					return path;
				}
			}

			return null;
		}

		public static RevitLinkType GetLinkedType(Document doc, string name)
		{
			// Collect all Revit link types in the document
			FilteredElementCollector linkTypeCollector = new FilteredElementCollector(doc)
				.OfClass(typeof(RevitLinkType));

			foreach (RevitLinkType linkType in linkTypeCollector)
			{
				// Get the name of the link type and display it
				if (linkType.Name == name)
				{
					return linkType;
				}
			}

			return null;
		}

		#endregion

		#region Retrieve Information from Document

		public static string GetProjectId(Document doc)
		{
			// Retrieve theunique ID from the project's shared parameters
			//ProjectInfo projectInfo = doc.ProjectInformation;
			//Parameter param = projectInfo.LookupParameter("ProjectUniqueId");

			// Retrieve the unique ID fromt the project's global parameters
			var param = new FilteredElementCollector(doc)
				.OfClass(typeof(GlobalParameter))
				.Cast<GlobalParameter>()
				.FirstOrDefault(x => x.Name == "ProjectUniqueId");

			if (param != null)
			{
				var value = param.GetValue() as StringParameterValue;
				return value?.Value;
			}

			return "";
		}
		
		public static string GetProjectName(Document doc)
		{
			// Get the full path of the current Revit project file
			string projectFilePath = doc.PathName;

			// if the project is unsaved, PahtName will be an empty string
			if (string.IsNullOrEmpty(projectFilePath))
			{
				return "";
			}
			else
			{
				// Extract the file name from the full path
				string projectFileName = System.IO.Path.GetFileName(projectFilePath);

				return projectFileName;
			}
		}

		public static List<SelectableItem> GetAllScopeBoxes(Document doc)
		{
			List<SelectableItem> res = [];

			// Create a filter to collect all elements fo the ScopeBox category
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			collector.OfCategory(BuiltInCategory.OST_VolumeOfInterest);

			// Convert the collector to a list of documents
			List<Element> scopeBoxes = collector.ToElements() as List<Element>;

			// Get scope box to be set in the active view
			var currentBox = GetCurrentScopeBox(doc);

			foreach (Element scopeBox in scopeBoxes)
			{
				res.Add(new SelectableItem()
				{
					Id = scopeBox.Id.Value,
					Name = scopeBox.Name,
					IsSelected = currentBox != null && scopeBox.Id == currentBox.Id,
				});
			}

			if(res.Count > 0 && res.Where(x => x.IsSelected).Count() == 0)
			{
				res[0].IsSelected = true;
			}

			return res;
		}

		public static Element GetCurrentScopeBox(Document doc)
		{
			// Get scope box to be set in the active view
			View activeView = doc.ActiveView;
			Parameter scopeBoxParam = activeView.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);

			// Check if the parameter has a valid value
			if (scopeBoxParam != null && scopeBoxParam.HasValue)
			{
				// Get the scope box element ID

				var currentBoxId = scopeBoxParam.AsElementId();
				return doc.GetElement(currentBoxId);
			}

			return null;
		}

		public static List<SelectableItem> GetAllGrids(Document doc)
		{
			List<SelectableItem> res = [];

			// Create a filter to collect all levels
			List<Grid> grids = new FilteredElementCollector(doc)
				.OfClass(typeof(Grid))
				.OfCategory(BuiltInCategory.OST_Grids)
				.Cast<Grid>()
				.ToList();

			foreach (Element grid in grids)
			{
				res.Add(new SelectableItem()
				{
					Id = grid.Id.Value,
					Name = grid.Name
				});
			}

			return res;
		}

		public static List<SelectableItem> GetAllLevels(Document doc)
		{
			List<SelectableItem> res = [];

			// Create a filter to collect all levels
			List<Level> levels = new FilteredElementCollector(doc)
				.OfClass(typeof(Level))
				.OfCategory(BuiltInCategory.OST_Levels)
				.Cast<Level>()
				.ToList();

			foreach (Element level in levels)
			{
				res.Add(new SelectableItem()
				{
					Id = level.Id.Value,
					Name = level.Name
				});
			}

			return res;
		}

		public static List<ElementId> GetConnectedElements(Element element)
		{
			List<ElementId> res = [];
			Document doc = element.Document;

			ConnectorManager connectorManager = null;

			if (element is Pipe pipe)
			{
				connectorManager = pipe.ConnectorManager;
			}
			else if (element is FamilyInstance familyInstance)
			{
				// Get connectors for the family instance (which is the pipe accessory or fitting)
				connectorManager = familyInstance.MEPModel.ConnectorManager;
			}

			if (connectorManager != null)
			{
				foreach (Connector connector in connectorManager.Connectors)
				{
					// Get the connected element
					if (connector.IsConnected)
					{
						// Get the connected connector and the element it belongs to
						foreach (Connector connectedConnector in connector.AllRefs)
						{
							if (connectedConnector.IsConnected)
							{
								Element connectedElement = connectedConnector.Owner;

								if (connectedElement != null)
								{
									res.Add(connectedElement.Id);
								}
							}
						}
					}
				}
			}


			return res;
		}

		public static List<CSPhase> GetOriginalPhases(Document doc)
		{
			List<CSPhase> res = new List<CSPhase>();

			// Retrieve all project phases
			PhaseArray phases = doc.Phases;

			foreach (Phase phase in phases)
			{
				res.Add(new CSPhase()
				{
#if REVIT2024 || REVIT2025
					ElementId = phase.Id.Value,
#else
					ElementId = phase.Id.IntegerValue,
#endif
					Name = phase.Name,
					Description = $"Phase ID: {phase.Id.Value}, Name: {phase.Name}"
				});
			}

			return res;
		}

		private static (XYZ, XYZ) GetTransformedBoundingBoxMinMax(BoundingBoxXYZ bbox, Transform transform)
		{
			List<XYZ> corners = new List<XYZ>
				{
						transform.OfPoint(bbox.Min),
						transform.OfPoint(new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z)),
						transform.OfPoint(new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z)),
						transform.OfPoint(new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Max.Z)),
						transform.OfPoint(bbox.Max),
						transform.OfPoint(new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z)),
						transform.OfPoint(new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Max.Z)),
						transform.OfPoint(new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Max.Z))
				};

			XYZ minPoint = new XYZ(corners.Min(p => p.X), corners.Min(p => p.Y), corners.Min(p => p.Z));
			XYZ maxPoint = new XYZ(corners.Max(p => p.X), corners.Max(p => p.Y), corners.Max(p => p.Z));

			return (minPoint, maxPoint);
		}

		public static bool IsElementOnLevel(Document doc, ElementId elementId, string levelName)
		{
			// Get the element by its ID
			Element element = doc.GetElement(elementId);

			// Get the level parameter of the element
			Parameter levelParam = element.get_Parameter(BuiltInParameter.LEVEL_PARAM);

			if(levelParam == null || !levelParam.HasValue)
			{
				levelParam = element.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT);
			}

			// TODO Get Level Param for different object

			if(levelParam != null && levelParam.HasValue)
			{
				// Get the level element
				ElementId levelId = levelParam.AsElementId();
				Level level = doc.GetElement(levelId) as Level;

				// Check if the level name matches the desired level name
				if (level != null && level.Name.Equals(levelName, StringComparison.OrdinalIgnoreCase)) 
				{
					return true;
				}
			}

			return false;
		}

		public static void SetLinkedModelTransparencey(Document doc, int transparentValue)
		{
			// Get the activeview
			View actvieView = doc.ActiveView;

			// Get all linked models
			var linkedModels = GetLinkedProjects(doc);
			using (Transaction trans = new Transaction(doc, "Set Linked Model Transparency"))
			{
				trans.Start();

				foreach (LinkedModel model in linkedModels)
				{
					// Create an OverrideGraphicSettings object
					OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();

					// Set the transparency value	
					//overrideSettings.SetSurfaceTransparency(transparentValue);
					//overrideSettings.SetHalftone(halftone: true);

					//Apply the overrde to settgins to the liniked model in active view
					actvieView.SetElementOverrides(new ElementId(model.InstanceId), overrideSettings);
				}

				trans.Commit();
			}
		}

		#endregion

		#region Integrate With Clashes

		public static List<Element> FindClashes(Element elem, ElementId category = null)
		{
			List<Element> clashes;// = new List<Element>();

			Document doc = elem.Document;
			FilteredElementCollector collector = new FilteredElementCollector(doc);

			if (category != null)
			{
				clashes = (List<Element>)collector.OfCategoryId(category).WherePasses(new ElementIntersectsElementFilter(elem)).ToElements();
			}
			else
			{
				clashes = (List<Element>)collector.WherePasses(new ElementIntersectsElementFilter(elem)).ToElements();
			}

			var connectedElements = GetConnectedElements(elem);

			var result = clashes.Where(x => !connectedElements.Contains(x.Id)).ToList();

			return result;
		}

		public static List<Element> FindClashes(Element element, ElementId categoryId, RevitLinkInstance linkInstance = null)
		{
			List<Element> clashes = new List<Element>();

			Document doc = element.Document;
			FilteredElementCollector collector = null;

			if (linkInstance == null)
			{
				collector = new FilteredElementCollector(doc);
			} 
			else
			{
				// Get the linked document
				Document linkedDoc = linkInstance.GetLinkDocument();
				if (linkedDoc == null)
				{ return clashes; }

				// Collect elements in the linked document that intersect with the host element
				collector = new FilteredElementCollector(linkedDoc);

			}
			// Create an ElementIntersectsElementFilter for the host element
			ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(element);

			var intersectingElements = collector.OfCategoryId(categoryId).WherePasses(filter).ToElements();

			clashes = [.. intersectingElements];

			return clashes;
		}

		public static void ViewCurrentElementInRevit(Document doc, ElementId elementId)
		{
			View view = doc.ActiveView;
			Element element = doc.GetElement(elementId);
			BoundingBoxXYZ boundingBox = element.get_BoundingBox(view);

			UIApplication uiApp = Application.GetUiApplication();

			if (boundingBox != null)
			{
				XYZ min = boundingBox.Min;
				XYZ max = boundingBox.Max;

				// Get the UIView corresponding to the current active view
				IList<UIView> uiViews = uiApp.ActiveUIDocument.GetOpenUIViews();
				UIView activeUIView = null;

				foreach (UIView uiView in uiViews)
				{
					if (uiView.ViewId == doc.ActiveView.Id)
					{
						activeUIView = uiView;
						break;
					}
				}

				if (activeUIView != null)
				{
					// Use ZoomAndFitToRectangle to zoom to the bounding box of the element
					activeUIView.ZoomAndCenterRectangle(min, max);
				}
			}

			// Select the element
			ICollection<ElementId> elementIds = new List<ElementId> { element.Id };
			uiApp.ActiveUIDocument.Selection.SetElementIds(elementIds);
		}

		public static void ViewClashDetectionInRevit(Document doc, Element element1, Element element2)
		{
			View view = doc.ActiveView;

			if (element1 == null || element2 == null)
				return;

			BoundingBoxXYZ boundingBox = GetClashBoundingBox(element1, element2);

			UIApplication uiApp = Application.GetUiApplication();

			if (boundingBox != null)
			{
				XYZ min = boundingBox.Min;
				XYZ max = boundingBox.Max;

				// Get the UIView corresponding to the current active view
				IList<UIView> uiViews = uiApp.ActiveUIDocument.GetOpenUIViews();

				UIView activeUIView = null;

				foreach (UIView uiView in uiViews)
				{
					if (uiView.ViewId == doc.ActiveView.Id)
					{
						activeUIView = uiView;
						break;
					}
				}

				if (activeUIView != null)
				{
					// Use ZoomAndFitToRectangle to zoom to the bounding box of the element
					activeUIView.ZoomAndCenterRectangle(min, max);
				}
			}

			// Select the element
			ICollection<ElementId> elementIds = new List<ElementId> { element1.Id, element2.Id };
			uiApp.ActiveUIDocument.Selection.SetElementIds(elementIds);
		}

		public static void ViewClashDetectionInLinkModel(Document doc, Issue issue)
		{
			// Retrieve the elements
			Element element1 = null;
			Element element2 = null;

			if (issue.LinkModelA.InstanceId == 0 && issue.LinkModelB.InstanceId == 0)
			{
				return;
			}

			var linkInstanceA = doc.GetElement(new ElementId(issue.LinkModelA.InstanceId)) as RevitLinkInstance;
			var linkInstanceB = doc.GetElement(new ElementId(issue.LinkModelB.InstanceId)) as RevitLinkInstance;
			if (linkInstanceA == null || linkInstanceB == null) return;

			var linkDocA = linkInstanceA.GetLinkDocument();
			var linkDocB = linkInstanceB.GetLinkDocument();

			var transform1 = linkInstanceA.GetTransform();
			var transform2 = linkInstanceB.GetTransform();

			element1 = linkDocA.GetElement(new ElementId(issue.ElementIdA));
			element2 = linkDocB.GetElement(new ElementId(issue.ElementIdB));

			if (element1 == null || element2 == null)
			{
				TaskDialog.Show("Error", "Could not retrieve one or both elements.");
				return;
			}

			// Get bounding boxes and transform them to the host coordinate system
			BoundingBoxXYZ bbox1 = TransformBoundingBox(element1.get_BoundingBox(null), transform1);
			BoundingBoxXYZ bbox2 = TransformBoundingBox(element2.get_BoundingBox(null), transform2);

			// Combine bounding boxes to create a clash box
			BoundingBoxXYZ clashBox = CombineBoundingBoxes(bbox1, bbox2);

			ZoomToBoundingBox(doc, clashBox);
		}

		public static void ZoomToBoundingBox(Document doc, BoundingBoxXYZ bbox)
		{
			UIApplication uiApp = Application.GetUiApplication();

			if (bbox != null)
			{
				XYZ min = bbox.Min;
				XYZ max = bbox.Max;

				// Get the UIView corresponding to the current active view
				IList<UIView> uiViews = uiApp.ActiveUIDocument.GetOpenUIViews();

				UIView activeUIView = null;

				foreach (UIView uiView in uiViews)
				{
					if (uiView.ViewId == doc.ActiveView.Id)
					{
						activeUIView = uiView;
						break;
					}
				}

				if (activeUIView != null)
				{
					// Use ZoomAndFitToRectangle to zoom to the bounding box of the element
					activeUIView.ZoomAndCenterRectangle(min, max);
				}
			}
		}

		public static Intersection GetIntersection(Element element1, Element element2, RevitLinkInstance linkInstance = null)
		{
			Intersection intersection = null;

			// Perform intersection
			Solid intersectionSolid = GetIntersectionSolid(element1, element2, linkInstance);

			if (intersectionSolid == null || intersectionSolid.Volume == 0)
				return intersection;

			BoundingBoxXYZ bbox = intersectionSolid.GetBoundingBox();

			if (bbox == null)
				return intersection;

			// Calculate the location and size of the intersectionsolid
			XYZ center = intersectionSolid.ComputeCentroid();
			double intersectionVolume = intersectionSolid.Volume;

			// Extract LocationCurve information from SolidB
			XYZ direction = null;

			if(element2.Location is LocationCurve locationCurve)
			{
				Curve curve = locationCurve.Curve;
				XYZ startPoint = curve.GetEndPoint(0);
				XYZ endPoint = curve.GetEndPoint(1);

				direction = (endPoint - startPoint).Normalize();
			}

			XYZ minPoint = GetGlobalPoint(element1, bbox.Min);
			XYZ maxPoint = GetGlobalPoint(element1, bbox.Max);

			minPoint = bbox.Transform.OfPoint(bbox.Min);
			maxPoint = bbox.Transform.OfPoint(bbox.Max);

			intersection = new Intersection()
			{
				Center = GetVector3DPoint(center),
				Min = GetVector3DPoint(minPoint),
				Max = GetVector3DPoint(maxPoint),
				Direction = GetVector3DPoint(direction)
			};

			return intersection;
		}

		public static XYZ GetXYZPoint(System.Windows.Media.Media3D.Vector3D point)
		{
			return new XYZ(point.X, point.Y, point.Z);
		}

		public static System.Windows.Media.Media3D.Vector3D GetVector3DPoint(XYZ point)
		{
			if(point == null)
			{
				return new System.Windows.Media.Media3D.Vector3D(0, 0, 0);
			}

			return new System.Windows.Media.Media3D.Vector3D(point.X, point.Y, point.Z);
		}

		public static Solid GetIntersectionSolid(Element element1, Element element2, RevitLinkInstance linkInstance = null)
		{
			if (element1 == null || element2 == null)
			{
				return null;
			}

			// Get solids
			Solid solidA = GetElementSolid(element1);

			if (solidA == null && element1 is FamilyInstance familyInstanceA)
			{
				solidA = GetSolidFromFamilyInstance(familyInstanceA);
			}

			Solid solidB = GetElementSolid(element2);

			if (solidB == null && element2 is FamilyInstance familyInstanceB)
			{
				solidB = GetSolidFromFamilyInstance(familyInstanceB);
			}

			if (linkInstance != null)
			{
				// Get the transformation of the linked model
				Transform linkTransform = linkInstance.GetTransform();

				solidB = SolidUtils.CreateTransformed(solidB, linkTransform);
			}

			if (solidA == null || solidB == null)
			{
				return null;
			}

			// Perform intersection
			Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solidA, solidB, BooleanOperationsType.Intersect);

			var intersectionPoint = GetIntersectionPointBetweenBeamAndPipe(element1, element2);

			return intersection;
		}

		// When dealing with solids that are created via sweeps along curves, like pipes or structural raming(beams), even if these elements
		// visually intersect and pass the ElementIntersectsElementFilter, their solids may not behave well with Boolean operations.
		// 
		public static Intersection GetIntersectionBetweenBeamAndPipe (Element beam, Element pipe, RevitLinkInstance linkInstance = null)
		{
			Intersection intersection = null;

			// Get the solid geometry of the beam
			Solid beamSolid = GetSolidFromFamilyInstance(beam as FamilyInstance);
			if (beamSolid == null)
			{
				TaskDialog.Show("Error", "Could not retrieve solid geometry for the beam.");
				return intersection;
			}

			Curve beamLocation = (beam.Location as LocationCurve).Curve;

			Solid pipeSolid = GetElementSolid(pipe);

			if (linkInstance != null)
			{
				// Get the transformation of the linked model
				Transform linkTransform = linkInstance.GetTransform();
				pipeSolid = SolidUtils.CreateTransformed(pipeSolid, linkTransform);
			}

			Transform beamTransform = (beam as FamilyInstance).GetTransform();

			List<Curve> resultArray = new List<Curve>();
			// Iterate over faces of the beam solid
			foreach (Face beamFace in beamSolid.Faces)
			{
				foreach(Face pipeFace in pipeSolid.Faces)
				{
					Curve curve = null;
					if (beamFace.Intersect(pipeFace, out curve) == FaceIntersectionFaceResult.Intersecting)
					{
						if(IsValidCurve(curve, beamFace))
						{
							if (curve is Arc arc)
							{
								if(!resultArray.Any(result => (result as Arc).Center.DistanceTo(arc.Center) < tolerance))
								{
									resultArray.Add(curve);
								}
							}
						}
					}
				}
			}

			// Imagine that the pipe is intersecting with beam only two faces
			if(resultArray.Count == 2)
			{
				Curve transformCurve = resultArray[1];
				//Curve transformCurve = resultArray[0].CreateTransformed(beamTransform);
				XYZ originPoint = (transformCurve as Arc).Center;
				// Adopt one of the intersection points
				intersection = new Intersection()
				{
					Center = ConvertToVector3D(originPoint)
				};
			}

			return intersection;
		}

		private static bool IsValidCurve(Curve curve, Face beamFace)
		{
			// Check if the curve is snull
			if (curve == null) return false;

			// Check if the curve length is adjust tolerance
			if (curve.ApproximateLength < tolerance) return false;

			// Check if the curve lies on the beam's face
			XYZ start = curve.GetEndPoint(0);
			XYZ end = curve.GetEndPoint(1);
			IntersectionResult result = beamFace.Project(start);
			if(result != null)
			{
				double distance = start.DistanceTo(result.XYZPoint);
				if (distance <= tolerance) return true; // Point is on the face within tolerance
			}

			result = beamFace.Project(end);
			if(result != null)
			{
				double distance = end.DistanceTo(result.XYZPoint);
				if(distance <= tolerance) return true;
			}

			return true;
		}

		public static System.Windows.Media.Media3D.Vector3D ConvertToVector3D(XYZ point)
		{
			return new System.Windows.Media.Media3D.Vector3D(point.X, point.Y, point.Z);
		}

		public static Curve GetPipeCenterLine(Pipe pipe)
		{
			if (pipe == null)
			{
				throw new ArgumentNullException(nameof(pipe));
			}

			// Get the LocationCurve of the pipe
			LocationCurve locationCurve = pipe.Location as LocationCurve;

			if (locationCurve == null)
				throw new InvalidOperationException("The pipe does not have a valid LocationCurve.");

			return locationCurve.Curve;
		}

		public static BoundingBoxXYZ GetClashBoundingBox(Element element1, Element element2)
		{
			// Get bounding boxes of the two elements
			BoundingBoxXYZ bbox1 = element1.get_BoundingBox(null);
			BoundingBoxXYZ bbox2 = element2.get_BoundingBox(null);

			if (bbox1 == null || bbox2 == null)
				return null;

			// Calculate the intersection of the two bounding boxes
			XYZ minIntersection = new XYZ(
				Math.Max(bbox1.Min.X, bbox2.Min.X),
				Math.Max(bbox1.Min.Y, bbox2.Min.Y),
				Math.Max(bbox1.Min.Z, bbox2.Min.Z)
			);

			XYZ maxIntersection = new XYZ(
				Math.Min(bbox1.Max.X, bbox2.Max.X),
				Math.Min(bbox1.Max.Y, bbox2.Max.Y),
				Math.Min(bbox1.Max.Z, bbox2.Max.Z)
			);

			// Check if the bounding boxes actually overlap
			if (minIntersection.X < maxIntersection.X &&
				minIntersection.Y < maxIntersection.Y &&
				minIntersection.Z < maxIntersection.Z)
			{
				// Create a new BoundingBoxXYZ for the intersection
				BoundingBoxXYZ clashBoundingBox = new BoundingBoxXYZ()
				{
					Min = minIntersection,
					Max = maxIntersection
				};

				return clashBoundingBox;
			}

			// No intersection found
			return null;
		}

		public static void UpdateMarkers(Document doc)
		{
			MarkerSetting setting = Application.thisApp.Setting;

			// Set visibility of clash marker
			SetMarkerVisibility(doc, setting);

			// Set the color of clash marker
			SetMarkerColor(doc, setting);

			// Set the parameters of clash marker
			SetMarkerParameters(doc, setting);
		}

		private static void SetMarkerVisibility(Document doc, MarkerSetting setting)
		{
			View view = doc.ActiveView;

			using (Transaction trans = new Transaction(doc, "Set ClashMarker Visibility"))
			{
				trans.Start();

				// Get tje Generic Model category
				Category genericModelCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_GenericModel);

				if (genericModelCategory != null)
				{
					// Find the subcategory named "ClashMarker"
					Category clashMarkerSubcategory = genericModelCategory.SubCategories.Cast<Category>().FirstOrDefault(c => c.Name == "ClashMarker");

					if (clashMarkerSubcategory != null)
					{
						clashMarkerSubcategory.set_Visible(view, setting.IsShowClashMarker);
					}
				}

				trans.Commit();
			}
		}

		public static List<XYZ> GetIntersectionPointBetweenBeamAndPipe(Element beam, Element pipe)
		{
			List<XYZ> intersectionPoints = new List<XYZ>();

			Solid beamSolid = GetSolidFromFamilyInstance(beam as FamilyInstance);
			Solid pipeSolid = GetSolidFromElement(pipe);

			if (beamSolid == null || pipeSolid == null)
			{
				TaskDialog.Show("Error", "Could not retrieve solid geometry for the beam or pipe.");
				return intersectionPoints;
			}

			// Iterate over faces of the beam
			foreach (Face beamFace in beamSolid.Faces)
			{
				// Check for intersections with faces of the pipe
				foreach(Edge pipeEdge in pipeSolid.Edges)
				{
					Curve edgeCurve = pipeEdge.AsCurve();

					IntersectionResultArray resultArray;
					if (beamFace.Intersect(pipeEdge.AsCurve(), out resultArray) == SetComparisonResult.Overlap)
					{
						foreach (IntersectionResult result in resultArray)
						{
							intersectionPoints.Add(result.XYZPoint);
						}
					}
					else
					{
						// No direct intersection detected, so check with tolerance manually
						// Check both ends of the pipe edge
						List<XYZ> pointsToCheck = new List<XYZ>()
						{
							edgeCurve.GetEndPoint(0),
							edgeCurve.GetEndPoint(1)
						};

						foreach (var point in pointsToCheck)
						{
							IntersectionResult projection = beamFace.Project(point);
							if (projection != null)
							{
								double distance = (projection.XYZPoint - point).GetLength();
								if (distance < tolerance)
								{
									intersectionPoints.Add(projection.XYZPoint);
								}
							}
						}
					}
				}
			}

			return intersectionPoints;
		}

		private static void SetMarkerColor(Document doc, MarkerSetting setting)
		{
			View3D view3D = doc.ActiveView as View3D;

			var clashMarkers = GetElementsByFamilyName(doc, Constants.MARKER_FAMILY_NAME);
			using (Transaction trans = new Transaction(doc, "Set ClashMarker Color"))
			{
				trans.Start();
				OverrideGraphicSettings ogs = new OverrideGraphicSettings();

				// set the projection line color according to setting
				Color color = new Color(setting.TextHighColor.R, setting.TextHighColor.G, setting.TextHighColor.B);
				Color border = new Color(255, 0, 0);
				ogs.SetProjectionLineColor(border);
				string patternName = "<Solid fill>";
				ElementId patternId = GetFillPatternId(doc, patternName);
				ogs.SetSurfaceBackgroundPatternId(patternId);
				ogs.SetSurfaceBackgroundPatternColor(color);

				foreach (var marker in clashMarkers)
				{
					view3D.SetElementOverrides(marker.Id, ogs);
				}

				trans.Commit();
			}
		}
		
		private static void SetMarkerParameters(Document doc, MarkerSetting setting)
		{
			try
			{
				// Get the Clash Marker family
				Family clashMarkerFamily = new FilteredElementCollector(doc)
						.OfClass(typeof(Family))
						.Cast<Family>()
						.FirstOrDefault(f => f.Name == Constants.MARKER_FAMILY_NAME);
				using (Transaction trans = new Transaction(doc, "Update Clash Marker Text Size"))
				{
					trans.Start();

					if (clashMarkerFamily != null)
					{
						// Get the first available FamilySymbol
						FamilySymbol clashMarkerSymbol = doc.GetElement(clashMarkerFamily.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;

						if (clashMarkerSymbol != null)
						{
							// Set the Text Size parameter
							Parameter param = clashMarkerSymbol.LookupParameter("Text Size");
							param?.Set(Util.MmToIU(setting.MarkerSize));

							// Set the Box Size parameter
							param = clashMarkerSymbol.LookupParameter("Box Size");
							param?.Set(Util.MmToIU(setting.BoxSize));

							param = clashMarkerSymbol.LookupParameter("IsClashIdVisible");
							param?.Set(setting.IsDisplayClashId ? 1 : 0);

							param = clashMarkerSymbol.LookupParameter("IsBoxVisible");
							param?.Set(setting.MarkerType == MarkerType.Box ? 1 : 0);
						}
					}

					trans.Commit();
				}
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("RevitHelper::SetMarkerParameters => ", ex);
			}
		}

		public static ElementId GetFillPatternId(Document doc, string patternName)
		{
			// Use a FilteredElementCollector to collect all fill patterns in the document
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			collector.OfClass(typeof(FillPatternElement));

			// Find the fill pattern with the specified name
			FillPatternElement fillPatternElement = collector
					.Cast<FillPatternElement>()
					.FirstOrDefault(fp => fp.Name.Equals(patternName, StringComparison.OrdinalIgnoreCase));

			// Return the fill pattern ID if found, otherwise return InvalidElementId
			return fillPatternElement != null ? fillPatternElement.Id : ElementId.InvalidElementId;
		}

		#endregion

		#region Error Handling

		private static void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
		{
			FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
			IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
			foreach (FailureMessageAccessor failure in failureMessages)
			{
				FailureDefinitionId failID = failure.GetFailureDefinitionId();

				// Add the failure definition IDs you want to ignore
				if (failID == BuiltInFailures.GeneralFailures.ErrorInSymbolFamilyResolved ||
					failID == BuiltInFailures.FamilyFailures.InstOutsideFaceBoundary ||
					failID == BuiltInFailures.FamilyFailures.DuplicateTypeName)
				{
					failuresAccessor.DeleteWarning(failure);
				}
			}
			e.SetProcessingResult(FailureProcessingResult.Continue);
		}

		public static List<ElementId> FilterDuplicateSystemTypeByName(Document hostDoc, Document linkedDoc, List<ElementId> systemTypeIds)
		{
			// Get all piping system types in the host document
			FilteredElementCollector hostCollector = new FilteredElementCollector(hostDoc)
				.OfClass(typeof(MEPSystemType));

			HashSet<string> hostSystemTypeNames = new HashSet<string>();
			foreach (MEPSystemType hostSystemType in hostCollector)
			{
				hostSystemTypeNames.Add(hostSystemType.Name);
			}


			var filteredIds = systemTypeIds.AsParallel()
				.Where(id =>
				{
					MEPSystemType linkedSystemType = linkedDoc.GetElement(id) as MEPSystemType;
					return linkedSystemType != null && !hostSystemTypeNames.Contains(linkedSystemType.Name);
				}).ToList();

			return filteredIds;
		}

		#endregion

		#region Handle

		public static List<CSPhase> GetPhasesFromLink(Document doc, string name)
		{
			List<CSPhase> res = new List<CSPhase>();

			// Collect all Revit link instances in the current model
			FilteredElementCollector collector = new FilteredElementCollector(doc)
				.OfClass(typeof(RevitLinkInstance));

			foreach (Element e in collector)
			{
				RevitLinkInstance linkInstance = e as RevitLinkInstance;

				if (linkInstance != null && linkInstance.Name == name)
				{
					// Access the linked document
					Document linkedDoc = linkInstance.GetLinkDocument();
					if (linkedDoc != null)
					{
						// Get all phases from the linked document
						PhaseArray phases = linkedDoc.Phases;

						foreach (Phase phase in phases)
						{
							res.Add(new CSPhase()
							{
								ElementId = phase.Id.Value,
								Name = phase.Name,
								Description = $"Phase ID: {phase.Id.Value}, Name: {phase.Name}"
							});
						}
					}
				}
			}


			return res;
		}

		public static void CopyFamilies(Document hostDoc, Document linkedDoc, IEnumerable<ElementId> familyIds)
		{
			using (Transaction trans = new Transaction(hostDoc, "Copy Families"))
			{
				trans.Start();

				foreach (ElementId familyId in familyIds)
				{
					Family family = linkedDoc.GetElement(familyId) as Family;
					if (family != null)
					{
						hostDoc.LoadFamily(linkedDoc.PathName, new FamilyLoadOpions(), out Family loadedFamily);
					}
				}

				trans.Commit();
			}
		}

		public static ICollection<ElementId> CopyElements(Document doc, Document linkedDoc, ICollection<ElementId> elementIds, Transform transform)
		{
			if (elementIds.Count == 0)
			{
				return null;
			}

			using (Transaction trans = new Transaction(doc, "Copy Elements from Linked Model"))
			{
				FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
				failureHandlingOptions.SetFailuresPreprocessor(new WarningException());
				trans.SetFailureHandlingOptions(failureHandlingOptions);

				trans.Start();

				CopyPasteOptions options = new CopyPasteOptions();
				options.SetDuplicateTypeNamesHandler(new IgnoreDuplicateTypeHandler());

				ICollection<ElementId> copyElemIds = null;

				try
				{
					// Subscribe to the FailureProcessing event
					doc.Application.FailuresProcessing += OnFailuresProcessing;

					copyElemIds = ElementTransformUtils.CopyElements(linkedDoc, elementIds, doc, transform, options);
				}
				catch (Autodesk.Revit.Exceptions.InvalidDataStreamException ex)
				{
					TaskDialog.Show("Error", $"Error during copy: {ex.Message}");
				}
				finally
				{
					// Unsubscribe from the FailureProcessing event
					doc.Application.FailuresProcessing -= OnFailuresProcessing;
				}

				trans.Commit();

				return copyElemIds;
			}
		}

		public static List<SelectableItem> GetDuplicatedFamilies(Document hostDoc, Document linkedDoc, List<SelectableItem> families)
		{
			var duplicatedFamilies = new List<SelectableItem>();

			FilteredElementCollector hostFamilies = new FilteredElementCollector(hostDoc).OfClass(typeof(Family));
			Dictionary<ElementId,string> hostFamilyNames = hostFamilies
				.Cast<Family>()
				.ToDictionary(f => f.Id, f => f.Name);

			foreach (var family in families)
			{
				Family linkedFamily = linkedDoc.GetElement(new ElementId(family.Id)) as Family;

				if (linkedFamily == null)
					continue;

				string familyName = linkedFamily.Name;
				if (linkedFamily != null && hostFamilyNames.ContainsKey(linkedFamily.Id))
				{
					hostFamilyNames.TryGetValue(linkedFamily.Id, out string	name);

					duplicatedFamilies.Add(new SelectableItem() { Id = linkedFamily.Id.Value, Name = name, IsSelected = false });
				}
			}

			return duplicatedFamilies;
		}

		public static void HandleDuplicateFamilies(Document hostDoc, Document linkedDoc, List<SelectableItem> families)
		{
			using (Transaction trans = new Transaction(hostDoc, "Delete Families in Host document"))
			{
				trans.Start();

				foreach (SelectableItem family in families)
				{
					var id = new ElementId(family.Id);
					if (hostDoc.GetElement(id) != null)
					{
						hostDoc.Delete(id);
					}

				}

				trans.Commit();
			}
		}

		public static void ReconnectElements(Document doc, ICollection<ElementId> copiedElementIds)
		{
			foreach (var elementId in copiedElementIds)
			{
				try
				{
					Element element = doc.GetElement(elementId);

					ConnectorManager connectors = Util.GetConnectorManager(element);

					if (connectors != null)
					{
						foreach (Connector connector in connectors.Connectors)
						{
							// Find nearby connectors and attempt to connect them
							foreach (Connector otherConnector in connectors.Connectors)
							{
								if (!connector.IsConnected && AreConnectorClose(connector, otherConnector))
								{
									connector.ConnectTo(otherConnector);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					TraceLogger.Instance.ExceptionLog("RevitHelper::ReconnectElements => ", ex);
					continue;
				}
			}
		}

		private static ConnectorManager GetConnectorManager(Element element)
		{
			if (element is MEPCurve mepCurve)
			{
				return mepCurve.ConnectorManager;
			}
			else if (element is FamilyInstance familyInstance)
			{
				return familyInstance.MEPModel?.ConnectorManager;
			}
			return null;
		}

		private static bool AreConnectorClose(Connector c1, Connector c2)
		{
			const double Tolerance = 0.01; // Adjust as needed
			return c1.Origin.DistanceTo(c2.Origin) < Tolerance;
		}

		public static void FixMEPCurveDirection(Document doc, ElementId copiedElementId)
		{
			Element element = doc.GetElement(copiedElementId);
			if (element is MEPCurve mEPCurve)
			{
				ConnectorSet connectors = mEPCurve.ConnectorManager.Connectors;
				foreach (Connector connector in connectors)
				{
					if (connector.IsConnected)
					{
						// Check and adjust the flow direction if needed
						Connector connectedTo = connector.AllRefs.Cast<Connector>().FirstOrDefault(c => c.Owner.Id != mEPCurve.Id);
						if (connectedTo != null)
						{
							// Ensure flow direction compatibility
							if (!AreDirectionsCompatible(connector, connectedTo))
							{
								// Reverse the MEPCurve if directions don't match
								ReverseMEPCurve(doc, mEPCurve);
							}
						}
					}
				}
			}
		}

		private static void ReverseMEPCurve(Document doc, MEPCurve curve)
		{
			// Get the current geometry of the curve
			LocationCurve locationCurve = curve.Location as LocationCurve;
			if (locationCurve != null)
			{
				// Get the start and end points
				XYZ startPoint = locationCurve.Curve.GetEndPoint(0);
				XYZ endPoint = locationCurve.Curve.GetEndPoint(1);

				// Create a new curve with reversed points
				Line reversedLine = Line.CreateBound(endPoint, startPoint);

				// Set the new reversed geometry
				locationCurve.Curve = reversedLine;
			}
		}

		private static bool AreDirectionsCompatible(Connector connector, Connector connectedTo)
		{
			return connector.CoordinateSystem.BasisZ.IsAlmostEqualTo(connectedTo.CoordinateSystem.BasisZ.Negate());
		}

		#endregion
	}

	public class IgnoreDuplicateTypeHandler: IDuplicateTypeNamesHandler
	{
		public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
		{
			return DuplicateTypeAction.UseDestinationTypes;
		}
	}

	public class FamilyLoadOpions : IFamilyLoadOptions
	{
		public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
		{
			overwriteParameterValues = true;
			return true;
		}

		public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
		{
			source = FamilySource.Project;
			overwriteParameterValues = true;
			return true;
		}
	}

	public class WarningException : IFailuresPreprocessor
	{
		public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
		{
			IList<FailureMessageAccessor> faillist = new List<FailureMessageAccessor>();
			//Inside event handler, get all warnings
			faillist = failuresAccessor.GetFailureMessages();
			foreach (FailureMessageAccessor failure in faillist)
			{
				//Check FailureDefinitionIds against ones that you want to dismiss
				FailureDefinitionId failID = failure.GetFailureDefinitionId();
				//Prevent Revit from showing Unenclosed warnings.
				// Here, you can add another Failure if you want
				if (failID == BuiltInFailures.GeneralFailures.ErrorInSymbolFamilyResolved ||
					failID == BuiltInFailures.FamilyFailures.InstOutsideFaceBoundary ||
					failID == BuiltInFailures.FamilyFailures.DuplicateTypeName)
				{
					failuresAccessor.DeleteWarning(failure);
				}
			}
			return FailureProcessingResult.Continue;
		}
	}

	public class ConvertUtils
	{
		public static XYZ ParseXYZ(string location)
		{
			// Remove the prefix and suffix
			string trimmed = location.Trim('X', 'Y', 'Z', '(', ')');

			// Splite the coordinates
			string[] parts = trimmed.Split(',');

			if (parts.Length == 3 &&
				double.TryParse(parts[0], out double x) &&
				double.TryParse(parts[1], out double y) &&
				double.TryParse(parts[2], out double z))
			{
				return new XYZ(x, y, z);
			}

			throw new FormatException("Invalid XYZ format");
		}
	}
}
