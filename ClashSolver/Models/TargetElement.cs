using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ClashSolver.Models
{
	public class TargetElement
	{
		public ElementId Id { get; set; }

		public ElementId LinkModelId { get; set; }

		public bool IsLinkedElement { get; set; }
	}
}
