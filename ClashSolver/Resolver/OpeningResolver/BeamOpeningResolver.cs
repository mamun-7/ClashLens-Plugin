using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClashSolver.Utils;
using Autodesk.Revit.UI;

namespace ClashSolver.Resolver
{
	public class BeamOpeningResolver : BaseOpeningResolver
	{
		public BeamOpeningResolver()
		{
			HostType = OpeningHostType.Beam;
		}

		public override void GetOpeningParameters()
		{

		}	

		public override void CreateOpening()
		{
			Element beam = HostElement;

			// Get location line of beam
			Curve beamLine = (beam.Location as LocationCurve).Curve;
			XYZ startPt = beamLine.GetEndPoint(0);
			XYZ endPt = beamLine.GetEndPoint(1);
			XYZ direction = (endPt - startPt).Normalize();
			XYZ verticalDirection = direction.CrossProduct(XYZ.BasisZ);

			// If the cross product results in a zero vector (beam is vertical), use a fallback
			if(verticalDirection.IsZeroLength())
			{
				return;
			}

			using (Transaction trans = new Transaction(Document, "Create Beam Opening"))
			{
				trans.Start();

				XYZ centerPoint = RevitHelper.GetXYZPoint(Intersection.Center);
				List<Curve> curveLoop = new List<Curve>();

				double diameter = Intersection.Height; // Consider if beam is vertical
				Arc arc = Arc.Create(centerPoint, diameter / 2, 0, 2 * Math.PI, direction, XYZ.BasisZ);
				curveLoop.Add(arc);

				CurveLoop openingCurveLoop = CurveLoop.Create(curveLoop);

				CurveArray curveArray = new CurveArray();
				foreach(Curve curve in openingCurveLoop)
				{
					curveArray.Append(curve);
				}

				Opening opening = Document.Create.NewOpening(beam, curveArray, Autodesk.Revit.Creation.eRefFace.CenterY);
				trans.Commit();
			}
		}
	}
}
