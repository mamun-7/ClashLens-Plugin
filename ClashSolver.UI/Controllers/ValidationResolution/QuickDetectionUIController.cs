using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class QuickDetectionUIController : BaseUIController
	{
		#region Fields

		private ObservableCollection<LinkedModel> _linkedModels = new ObservableCollection<LinkedModel>();

		#endregion

		#region Properties

		public ObservableCollection<LinkedModel> LinkedModels
		{
			get => _linkedModels;
			set
			{
				_linkedModels = value;
				OnPropertyChanged(nameof(LinkedModels));
			}
		}

		#endregion

		#region Constructors

		#endregion

		#region Event Handlers

		public void DeleteIssues()
		{
			IssueTableAdapter.Instance.DeleteByProjectId(Project.Id);
		}


		public virtual void UpdateIssues()
		{

		}

		public virtual List<Issue> FindClash(int id)
		{
			return null;
		}

		public virtual int GetTotalCount()
		{
			return 0;
		}

		#endregion
	}
}
