using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Request;

namespace ClashSolver.Commands
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ManageLinksCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ManageLinks);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ClashSettingsCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ClashSettings);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ComplianceSettingsCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ComplianceSettings);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class MarkersCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.IssueMarkers);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class CostDatabaseCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.CostDatabase);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ManageTeamCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ManageTeam);

			return Result.Succeeded;
		}
	}
}
