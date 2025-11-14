using Autodesk.Revit.DB;
using ClashSolver.UI;
using ClashSolver.UI.Models;
using ClashSolver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashSolver.Resolver
{
	public enum OpeningHostType
	{
		None = 0x1,
		Wall,
		Floor,
		Ceil,
		Beam
	}

	public enum OpeningShape
	{
		None = 0x1,
		Line,
		Rectangular,
		Circle
	}

	public class BaseOpeningResolver
	{
		public Document Document { get; set; }

		public Issue Issue { get; set; }

		public Element HostElement { get; set; }

		public Element PipeElement { get; set; }

		public UI.Models.Settings Settings { get; set; }

		public Intersection Intersection { get; set; }

		public OpeningHostType HostType { get; set; }

		public OpeningShape OpeningShapeType { get; set; }

		protected double CorrectLength(double length)
		{
			// Convert length from IU to MM
			double lengthInMM = Util.IUToMm(length);

			// Round the length according to RoundupType
			if(Settings.RoundupType != RoundupType.None)
			{
				// Remove the first character and parse the rest as the interval unit
				string intervalStr = Settings.RoundupType.ToString().Substring(1);

				if(int.TryParse(intervalStr, out int interval))
				{
					lengthInMM = Math.Ceiling(lengthInMM / interval) * interval;
				}
			}

			// Convert length back to IU
			return Util.MmToIU(lengthInMM);
		}

		public virtual void GetOpeningParameters()
		{

		}

		public virtual bool CanbeResolved()
		{
			return true;
		}

		public virtual void CreateOpening()
		{

		}

	}
}
