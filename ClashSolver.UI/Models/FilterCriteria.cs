using System.Collections.Generic;
using System.Windows.Documents;

namespace ClashSolver.UI.Models
{
	public class FilterCriteria
	{
		#region Properties

		public long UserId { get; set; }

		public long ProjectId { get; set; }

		/// <summary>
		/// Filter by selected detection sets
		/// </summary>
		public bool IsFilterBySet { get; set; } = false;

		/// <summary>
		/// Ids of selected detection sets
		/// </summary>
		public List<string> SelectedDetectionSets { get; set; } = new List<string>();

		/// <summary>
		/// Whether filtering by current scope in view3D or not
		/// </summary>
		public bool IsFilterByScope { get; set; } = false;

		/// <summary>
		/// Instance id of scope box in View3D of Revit project
		/// </summary>
		public long ScopeBoxId { get; set; }

		/// <summary>
		/// Whether filtering by headers of DataGrid in Issue dock panel
		/// </summary>
		public bool IsFilterByHeader { get; set; } = false;

		/// <summary>
		/// Ids of host categories
		/// </summary>
		public List<string> CategoryAIds { get; set; } = new List<string>();

		/// <summary>
		/// Ids of secondary categories
		/// </summary>
		public List<string> CategoryBIds { get; set; } = new List<string>();

		/// <summary>
		/// Statuses to be displaying issues
		/// </summary>
		public List<string> Statuses { get; set; } = new List<string>();

		/// <summary>
		/// Severities to be displaying issues
		/// </summary>
		public List<string> Severities { get; set; } = new List<string>();

		public int PageNumber { get; set; } = 1;

		#endregion
	}
}
