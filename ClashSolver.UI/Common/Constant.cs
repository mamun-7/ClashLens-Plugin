using System.ComponentModel.DataAnnotations;

namespace ClashSolver.UI
{
	#region Enums

	public enum MarkerType
	{
		Bubble = 0x1,
		Box,
		Pyramid
	}

	public enum ValidationType
	{
		Quick = 0x1,
		Custom
	}

	public enum DetailLevel
	{
		Basic = 0x1,
		Intermediate,
		Advanced
	}

	public enum IssueStatus
	{
		[Display(Name = "Open")]
		Open = 0x1,
		[Display(Name = "Pending Approval")]
		PendingApproval,
		[Display(Name = "Under Review")]
		UnderReview,
		[Display(Name = "Closed")]
		Closed
	}

	public enum LinkDiscipline
	{
		None = 0x1,
		Architectural,
		Structural,
		Mechanical,
		Electrical,
		Plumbing,
	}

	public enum ReportType
	{
		[Display(Name = "Current Test")]
		CURRENT_TEST,
		[Display(Name = "All Tests(Combined)")]
		ALL_TESTS_COMBINED,
		[Display(Name = "All Tests(Separate)")]
		ALL_TESTS_SEPERATE,
	}

	public enum ReportFormat
	{
		[Display(Name = "Xlsx")]
		Xlsx,
		[Display(Name = "Text")]
		Text,
		//[Display(Name = "XML")]
		//XML,
		//[Display(Name = "HTML")]
		//HTML
	}

	public enum FilterType
	{
		None = 0x1,
		All,
		Invert
	}

	public enum Visibility
	{
		[Display(Name = "Shown")]
		Shown = 0x1,
		[Display(Name = "Hidden")]
		Hidden,
		[Display(Name = "Halfton")]
		Halfton
	}

	public enum CopyType
	{
		Overwrite = 0x1,
		Newonly
	}

	public enum WorkSets
	{
		CREATE_FROM_LINKE = 0x1,
		NOCREATE_FROM_LINK
	}

	public enum GridCopyType
	{
		FROM_ARCHITECTURE = 0x1,
		FROM_SPECIFIC
	}

	public enum ResolveType
	{
		None = 0x1,
		Move,
		Reroute,
		Resize,
		Opening,
		Delete,
	}

	public enum ResolveMethod
	{
		None = 0x1,
		Manual,
		AI,
	}

	public enum RoundupType
	{
		[Display(Name="Do not round up")]
		None = 0x1,
		[Display(Name = "To the nearest 50mm")]
		M50,
		[Display(Name = "To the nearest 25mm")]
		M25,
		[Display(Name = "To the nearest 20mm")]
		M20,
		[Display(Name = "To the nearest 10mm")]
		M10,
		[Display(Name = "To the nearest 5mm")]
		M5,
		[Display(Name = "To the nearest 1mm")]
		M1,
	}

	public enum Direction
	{
		Up = 0x1,
		Down,
		Left,
		Right,
		Front,
		Back
	}
	#endregion
}
