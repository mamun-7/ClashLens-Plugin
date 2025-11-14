using System.Collections.ObjectModel;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class ComplianceReportUIController : BaseUIController
	{
		#region Fields

		private ObservableCollection<ReportContent> _reportContents = [];
		private ReportType _selectedReportType = ReportType.ALL_TESTS_COMBINED;
		private ReportFormat _selectedReportFormat = ReportFormat.Xlsx;

		#endregion

		#region Properties

		public ObservableCollection<ReportContent> ReportContents
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

		#endregion

		#region Constructors

		public ComplianceReportUIController() 
		{
			// Get Report Contents From Database
			foreach(var obj in ReportContentTableAdapter.Instance.GetByType(type:"Compliance"))
			{
				if(obj is ReportContent reportContent)
				{
					ReportContents.Add(new ReportContent()
					{
						Id = reportContent.Id,
						Name = reportContent.Name
					});
				}
			}
		}

		#endregion
	}
}
