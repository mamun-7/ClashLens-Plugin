using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class ReportIssuesUIController : BaseUIController
	{
		#region Fields

		private ObservableCollection<SelectableItem> _reportContents = [];
		private ReportType _selectedReportType = ReportType.ALL_TESTS_COMBINED;
		private ReportFormat _selectedReportFormat = ReportFormat.Xlsx;
		private ObservableCollection<SelectableItem> _statuses = [];

		#endregion

		#region Properties

		public ObservableCollection<SelectableItem> ReportContents
		{
			get => _reportContents;
			set
			{
				_reportContents = value;
				OnPropertyChanged(nameof(ReportContents));
			}
		}

		public ReportType SelectedReportType
		{
			get => _selectedReportType;
			set
			{
				_selectedReportType = value;
				OnPropertyChanged(nameof(SelectedReportType));
			}
		}

		public ReportFormat SelectedReportFormat
		{
			get => _selectedReportFormat;
			set
			{
				_selectedReportFormat = value;
				OnPropertyChanged(nameof(SelectedReportFormat));
			}
		}

		public ObservableCollection<SelectableItem> Statuses
		{
			get => _statuses;
			set
			{
				_statuses = value;
				OnPropertyChanged(nameof(Statuses));
			}
		}

		#endregion

		#region Constructors

		public ReportIssuesUIController() 
		{
			List<string> reportContentNames = ["Id", "ElementA", "ElementB", "Status", "Severity", "AssignedBy", "AnalyzedAt"];
			// Get Report Contents From Database
			foreach (string name in reportContentNames)
			{
				ReportContents.Add(new SelectableItem()
				{
					IsSelected = true,
					Name = name
				});
			}


			foreach (var obj in Enum.GetNames(typeof(IssueStatus)))
			{
				Statuses.Add(new SelectableItem()
				{
					IsSelected = true,
					Name = obj.ToString()
				});
			}
		}

		#endregion

		#region Event Handlers

		public override void OnOK()
		{

		}

		#endregion
	}
}
