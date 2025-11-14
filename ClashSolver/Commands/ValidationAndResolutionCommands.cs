using Architexor.Core;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Request;

namespace ClashSolver.Commands
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class CopyFromLinksCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			// License Check
			//if(!Constants.thisUser.IsLicensed)
   //         {
   //             TaskDialog.Show("License Error", Constants.INVALID_LICENSE);
   //             return Result.Failed;
   //         }

            UIApplication uiApp = commandData.Application;
            Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.CopyFromLinks);

            return Result.Succeeded;
		}
	}


	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class QuickDetectionCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.QuickDetection);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class RunValidationCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //// License Check
            //if (!Constants.thisUser.IsLicensed)
            //{
            //    TaskDialog.Show("License Error", Constants.INVALID_LICENSE);
            //    return Result.Failed;
            //}

            UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.RunValidation);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ReviewIssuesCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //// License Check
            //if (!Constants.thisUser.IsLicensed)
            //{
            //    TaskDialog.Show("License Error", Constants.INVALID_LICENSE);
            //    return Result.Failed;
            //}

            UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ReviewIssues);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class IssueReportsCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //// License Check
            //if (!Constants.thisUser.IsLicensed)
            //{
            //    TaskDialog.Show("License Error", Constants.INVALID_LICENSE);
            //    return Result.Failed;
            //}

            UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.IssueReport);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class ComplianceHealthReportCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			//// License Check
			//if(!Constants.thisUser.IsLicensed)
   //         {
   //             TaskDialog.Show("License Error", Constants.INVALID_LICENSE);
   //             return Result.Failed;
   //         }

			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.ComplianceHealthReport);

			return Result.Succeeded;
		}
	}
}
