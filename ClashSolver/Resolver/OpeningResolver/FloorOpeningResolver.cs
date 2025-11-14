using Autodesk.Revit.DB;
using ClashSolver.Utils;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClashSolver.Resolver
{
	public class FloorOpeningResolver : BaseOpeningResolver
	{
		public FloorOpeningResolver()
		{
			HostType = OpeningHostType.Floor;
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

			Element floor = HostElement;
			Element pipe = PipeElement;

			if(floor == null)
			{
				return;
			}


			using (Transaction trans = new Transaction(Document, "Create Openings"))
			{
				trans.Start();

				//XYZ centerPoint = (minPoint + maxPoint) / 2;
				XYZ centerPoint = RevitHelper.GetXYZPoint(Intersection.Center);
				// Correct to Settings.RoundupType
				double width = CorrectLength(Intersection.Width);
				double depth = CorrectLength(Intersection.Depth);

				double openingWidth = Math.Max(width, depth);

				if(openingWidth > Settings.MinDiameterToRect)
				{
					OpeningShapeType = OpeningShape.Rectangular;
				}
				else
				{
					OpeningShapeType = OpeningShape.Circle;
				}

				List<Curve> curveLoop = new List<Curve>();

				switch (OpeningShapeType)
				{
					case OpeningShape.Circle:
						double diameter = Math.Max(width, depth);
						Arc arc = Arc.Create(centerPoint, diameter / 2, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
						curveLoop.Add(arc);
						break;
					case OpeningShape.Rectangular:
						XYZ p1 = new XYZ(centerPoint.X - width / 2, centerPoint.Y - depth / 2, centerPoint.Z);
						XYZ p2 = new XYZ(centerPoint.X + width / 2, centerPoint.Y - depth / 2, centerPoint.Z);
						XYZ p3 = new XYZ(centerPoint.X + width / 2, centerPoint.Y + depth / 2, centerPoint.Z);
						XYZ p4 = new XYZ(centerPoint.X - width / 2, centerPoint.Y + depth / 2, centerPoint.Z);

						curveLoop.Add(Line.CreateBound(p1, p2));
						curveLoop.Add(Line.CreateBound(p2, p3));
						curveLoop.Add(Line.CreateBound(p3, p4));
						curveLoop.Add(Line.CreateBound(p4, p1));
						break;
					default:
						break;
				}

				CurveLoop openingCurveLoop = CurveLoop.Create(curveLoop);

				CurveArray curveArray = new CurveArray();
				foreach(Curve curve in openingCurveLoop)
				{
					curveArray.Append(curve);
				}

				Opening opening = Document.Create.NewOpening(floor, curveArray, true);
				trans.Commit();
			}
		}

		/// <summary>
		/// Create Opening which make up of line on floor
		/// </summary>
		/// <param name="points">Points use to create Opening</param>
		private void DrawPlineOpening(List<Vector4> points)
		{
			Autodesk.Revit.DB.XYZ p1, p2; Line curve;
			CurveArray curves = Document.Application.Create.NewCurveArray();
			for (int i = 0; i < points.Count - 1; i++)
			{
				p1 = new Autodesk.Revit.DB.XYZ(points[i].X, points[i].Y, points[i].Z);
				p2 = new Autodesk.Revit.DB.XYZ(points[i + 1].X, points[i + 1].Y, points[i + 1].Z);
				curve = Line.CreateBound(p1, p2);
				curves.Append(curve);
			}

			p1 = new Autodesk.Revit.DB.XYZ(points[points.Count - 1].X,
				points[points.Count - 1].Y, points[points.Count - 1].Z);
			p2 = new Autodesk.Revit.DB.XYZ(points[0].X, points[0].Y, points[0].Z);
			curve = Line.CreateBound(p1, p2);
			curves.Append(curve);

			Element hostElement = HostElement;
			Document.Create.NewOpening(hostElement, curves, true);
		}

		/// <summary>
		/// Create Opening which make up of Circle on floor
		/// </summary>
		/// <param name="points">Points use to create Opening</param>
		private void DrawCircleOpening(List<Vector4> points)
		{
			CurveArray curves = Document.Application.Create.NewCurveArray();
			Autodesk.Revit.DB.XYZ p1 = new Autodesk.Revit.DB.XYZ(points[0].X, points[0].Y, points[0].Z);
			Autodesk.Revit.DB.XYZ p2 = new Autodesk.Revit.DB.XYZ(points[1].X, points[1].Y, points[1].Z);
			Autodesk.Revit.DB.XYZ p3 = new Autodesk.Revit.DB.XYZ(points[2].X, points[2].Y, points[2].Z);
			Autodesk.Revit.DB.XYZ p4 = new Autodesk.Revit.DB.XYZ(points[3].X, points[3].Y, points[3].Z);
			Arc arc = Arc.Create(p1, p3, p2);
			Arc arc2 = Arc.Create(p1, p3, p4);
			curves.Append(arc);
			curves.Append(arc2);

			Element hostElement = HostElement;
			Document.Create.NewOpening(hostElement, curves, true);
		}
	}
}
