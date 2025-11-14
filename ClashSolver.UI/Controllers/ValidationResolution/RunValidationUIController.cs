using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class RunValidationUIController: BaseUIController
	{
		#region Fields

		// For Clash Detection
		private ClashType _clashType = new ();
		private ValidationType _validationType = ValidationType.Quick;
		private DetailLevel _detailLevel = DetailLevel.Basic;
		private ObservableCollection<ClashDetectionSet> _clashDetectionSets = [];

		// For Scope
		private bool _isFilterByParts = false;

		private bool _isFilterBySectionBox = false;

		private bool _isFilterByScopeBox = false;
		private bool _canFilterByScopeBox = false;
		private bool _isFilterByLevel = false;

		private ObservableCollection<SelectableItem> _scopes = [];
		private ObservableCollection<SelectableItem> _levels = [];
		private ObservableCollection<SelectableItem> _phaseSelectors = [];

		#endregion

		#region Properties

		public ValidationType ValidationType
		{
			get { return _validationType; }
			set
			{
				_validationType = value;
				OnPropertyChanged(nameof(ValidationType));
			}
		}

		public ClashType ClashType
		{
			get { return _clashType; }
			set
			{
				_clashType = value;
				OnPropertyChanged(nameof(ClashType));
			}
		}

		public DetailLevel DetailLevel
		{
			get => _detailLevel;
			set
			{
				_detailLevel = value;
				OnPropertyChanged(nameof(DetailLevel));
			}
		}

		public ObservableCollection<ClashDetectionSet> Sets
		{
			get => _clashDetectionSets;

			set
			{
				_clashDetectionSets = value;
				OnPropertyChanged(nameof(Sets));
			}
		}

		// For Scope

		public bool IsFilterByParts
		{
			get { return _isFilterByParts; }
			set
			{
				_isFilterByParts = value;
				OnPropertyChanged(nameof(IsFilterByParts));
			}
		}

		public bool CanFilterByScopeBox
		{
			get { return _canFilterByScopeBox; }
			set
			{
				_canFilterByScopeBox = value;
				OnPropertyChanged(nameof(CanFilterByScopeBox));
			}
		}
		public bool IsFilterByScopeBox
		{
			get { return _isFilterByScopeBox; }
			set
			{
				_isFilterByScopeBox = value;
				OnPropertyChanged(nameof(IsFilterByScopeBox));
			}
		}

		public ObservableCollection<SelectableItem> Scopes
		{
			get { return _scopes; }
			set
			{
				_scopes = value;
				OnPropertyChanged(nameof(Scopes));
			}
		}

		public bool IsFilterByLevel
		{
			get { return _isFilterByLevel; }
			set
			{
				_isFilterByLevel = value;
				OnPropertyChanged(nameof(IsFilterByLevel));
			}
		}

		public ObservableCollection<SelectableItem> Levels
		{
			get { return _levels; }
			set
			{
				_levels = value;
				OnPropertyChanged(nameof(Levels));
			}
		}

		public ObservableCollection<SelectableItem> Phases
		{
			get { return _phaseSelectors; }
			set
			{
				_phaseSelectors = value;
				OnPropertyChanged(nameof(Phases));
			}
		}

		public List<CSElement> Elements { get; set; }

		#endregion

		#region Event Handers

		public virtual bool RetrieveElementsToBeAnalyzed() { return false; }

		public virtual List<Issue> FindClash(CSElement element) { return []; }

		public virtual void UpdateIssues(List<Issue> issues) { }

		public void DeleteIssues()
		{
			IssueTableAdapter.Instance.DeleteByProjectId(Project.Id);
		}

		#endregion
	}
}
