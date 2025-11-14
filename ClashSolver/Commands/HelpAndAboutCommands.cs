using System.Windows.Forms;
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashSolver.Request;
using System.IO;
using ClashSolver.UI.Views.HelpAssistance;

namespace ClashSolver.Commands
{
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class HelpCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.Help);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class TutorialsCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.Help);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class AboutCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.About);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class LogCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			UIApplication uiApp = commandData.Application;
			//Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.About);

			// Define the source file path
			string sourceFilePath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"IntelliBIM",
					"ExceptionLogger.txt"
			);

			// Check if the source file exists
			if (!File.Exists(sourceFilePath))
			{
				MessageBox.Show("The log file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return Result.Cancelled;
			}

			// Open a folder browser dialog to select the destination folder
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the destination folder to copy the log file";

				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					string destinationFolder = folderBrowserDialog.SelectedPath;

					// Define the destination file path
					string destinationFilePath = Path.Combine(destinationFolder, "ExceptionLogger.txt" + "_" + DateTime.Now.ToString("yyyymmddhhmm"));

					try
					{
						// Copy the file to the selected folder
						File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
						MessageBox.Show("Log file copied successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"An error occurred while copying the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class LicenseCommand : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{

			UIApplication uiApp = commandData.Application;
			Application.thisApp.DoRequest(uiApp, ClashSolverRequestId.License);

			return Result.Succeeded;
		}
	}
}
