using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashSolver.UI.Models
{
	public class SystemSpecificTolerance: BaseModel
	{
		#region Properties

		public int Id { get; set; }

		public string System { get; set; }

		public string Name { get; set; }

		public string Description { get;set; }

		public double High { get; set; }

		public double Medium { get; set; }

		public double Low { get; set; }

		#endregion
	}
}
