using Autodesk.Revit.DB;
using ClashSolver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashSolver.Resolver
{
	public class WallOpeningResolver : BaseOpeningResolver
	{
		public WallOpeningResolver()
		{
			HostType = OpeningHostType.Wall;
		}

		public override void GetOpeningParameters()
		{

		}

		public override void CreateOpening()
		{
			if(Intersection == null)
			{
				return;
			}

			Wall wall = HostElement as Wall;
			Element element = PipeElement;

			if(wall == null)
			{
				return;
			}

			using (Transaction trans = new Transaction(Document, "Create Openings"))
			{
				trans.Start();

				XYZ minPoint = RevitHelper.GetXYZPoint(Intersection.Min);
				XYZ maxPoint = RevitHelper.GetXYZPoint(Intersection.Max);

				// Calculate the center point ad radius of the circular opening

				//XYZ centerPoint = (minPoint + maxPoint) / 2;
				XYZ centerPoint = RevitHelper.GetXYZPoint(Intersection.Center);
				//double radius = Math.Max(maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y) / 2;

				// TODO
				//if (OpeningShapeType == OpeningShape.Circle)
				//{
				//	// Load the circular opening family if not already loaded
				//	FamilySymbol circularOpeningSymbol = LoadCircularOpeningFamily(Document);

				//	if (circularOpeningSymbol != null)
				//	{
				//		// Activate the family symbol if it's not already active
				//		if (!circularOpeningSymbol.IsActive)
				//		{
				//			circularOpeningSymbol.Activate();
				//			Document.Regenerate();
				//		}

				//		// TODO
				//		// Place the circular opening family instance
				//		FamilyInstance circularOpening = Document.Create.NewFamilyInstance(centerPoint, circularOpeningSymbol, wall, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

				//		// Set the radius parameter of the circular opening
				//		Parameter radiusParam = circularOpening.LookupParameter("Radius");
				//		if(radiusParam != null)
				//		{
				//			radiusParam.Set(radius);
				//		}

				//		if (InstanceVoidCutUtils.CanBeCutWithVoid(wall))
				//		{
				//			InstanceVoidCutUtils.AddInstanceVoidCut(Document, wall, circularOpening);
				//		}
				//	}
				//}
				//else
				//{
					Document.Create.NewOpening(wall, minPoint, maxPoint);
				//}

				trans.Commit();
			}
		}

		private FamilySymbol LoadCircularOpeningFamily(Document doc)
		{
			// Check if the circular family is already loaded
			FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
			FamilySymbol circularOpeningSymbol = collector.FirstOrDefault(x => x.Name == "IntelliBIM_CircularOpening") as FamilySymbol;

			if(circularOpeningSymbol == null)
			{
				// Load the circular opening family from a file
				string familyPath = @"\Family\IntelliBIM_CircularOpening.rfa";

				if (doc.LoadFamily(familyPath, out Family family))
				{
					foreach (ElementId id in family.GetFamilySymbolIds())
					{
						circularOpeningSymbol = doc.GetElement(id) as FamilySymbol;
						if (circularOpeningSymbol != null)
						{
							break;
						}
					}
				}
			}

			return circularOpeningSymbol;
		}
	}
}
