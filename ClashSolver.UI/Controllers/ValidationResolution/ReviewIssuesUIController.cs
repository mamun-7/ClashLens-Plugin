using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Architexor.Core;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class ReviewIssuesUIController : BaseUIController
	{
		#region Fields

		private string _validationTime = "";
		private bool _isFilterBySet = false;
		private bool _isFilterByScope = false;
		private bool _isFilterByHeader = false;

		private Dictionary<string, object> _existingFilterSets = [];
		private Dictionary<string, object> _selectedFilterSets = [];

		private ObservableCollection<Issue> _issues = new ObservableCollection<Issue>();
		private ObservableCollection<SelectableItem> _filters = new ObservableCollection<SelectableItem>();

		private Issue _selectedIssue = new Issue();

		private IssueStatus _statusBy = IssueStatus.Open;
		private Visibility _visibility = Visibility.Shown;

		// For Pagination
		private int _currentPageNumber = 1;
		private int _pageCount = 1;

		private bool _isOpen = false;

		#endregion

		#region Properties

		public string ValidationTime 
		{
			get { return _validationTime; }
			set
			{
				_validationTime = value;
				OnPropertyChanged(nameof(ValidationTime));
			}
		}

		public bool IsFilterBySet
		{
			get { return _isFilterBySet; }
			set
			{
				_isFilterBySet = value;
				OnPropertyChanged(nameof(IsFilterBySet));
			}
		}

		public bool IsFilterByScope
		{
			get { return _isFilterByScope; }
			set
			{
				_isFilterByScope = value;
				OnPropertyChanged(nameof(IsFilterByScope));
			}
		}

		public long CurrentScopeBoxId { get; set; }

		public bool IsFilterByHeader
		{
			get { return _isFilterByHeader; }
			set
			{
				_isFilterByHeader = value;
				OnPropertyChanged(nameof(IsFilterByHeader));
			}
		}

		public Dictionary<string, object> FilterSets
		{
			get { return _existingFilterSets; }
			set
			{
				_existingFilterSets = value;
				OnPropertyChanged(nameof(FilterSets));
			}
		}

		public Dictionary<string, object> SelectedFilterSets
		{
			get { return _selectedFilterSets; }
			set
			{
				_selectedFilterSets = value;
				OnPropertyChanged(nameof(SelectedFilterSets));
			}
		}

		public ObservableCollection<Issue> Issues
		{
			get => _issues;
			set
			{
				_issues = value;
				OnPropertyChanged(nameof(Issues));
			}
		}

		public Issue SelectedIssue
		{
			get { return _selectedIssue; }
			set
			{
				_selectedIssue = value;
				OnPropertyChanged(nameof(SelectedIssue));
			}
		}

		public ObservableCollection<SelectableItem> Filters
		{
			get => _filters;
			set
			{
				_filters = value;
				OnPropertyChanged(nameof(Filters));
			}
		}

		public IssueStatus ByStatus
		{
			get => _statusBy;
			set
			{
				_statusBy = value;
				OnPropertyChanged(nameof(ByStatus));
			}
		}

		public Visibility ByVisibility
		{
			get => _visibility;
			set
			{
				_visibility = value;
				OnPropertyChanged(nameof(ByVisibility));
			}
		}

		public int CurrentPageNumber
		{
			get => _currentPageNumber;
			set
			{
				_currentPageNumber = value;
				OnPropertyChanged(nameof(CurrentPageNumber));
				UpdateIssues();
			}
		}

		public int PageCount
		{
			get => _pageCount;
			set
			{
				_pageCount = value;
				OnPropertyChanged(nameof(PageCount));
			}
		}

		public List<string> FilteredACategoryIds { get; set; }

		public List<string> FilteredBCategoryIds { get; set; }

		public List<string> FilteredStatus { get; set; }

		public List<string> FilteredSeverity { get; set; }

		public string HeaderName { get; set; }

		public ICommand FilterCommand { get; set; }

		public bool IsOpen
		{
			get => _isOpen;
			set
			{
				_isOpen = value;
				OnPropertyChanged(nameof(IsOpen));
			}
		}

		#endregion

		#region Constructors

		// For Test
		public ReviewIssuesUIController()
		{
#if UITEST
			Project = ProjectTableAdapter.Instance.GetById(1) as Project;
			FilterSets.Clear();
			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					FilterSets.Add(detectionSet.Name, detectionSet);
				}
			}

			//ExistingFilterSets = temp;
			//Project = ProjectTableAdapter.Instance.GetById(1) as Project;
			//CurrentPageNumber = 1;

			List<Issue> issues = new List<Issue>();

			foreach (var obj in IssueTableAdapter.Instance.GetByFilter(1, CurrentPageNumber))
			{
				if (obj is Issue issue)
				{
					issues.Add(issue);
				}
			}

			Issues = [.. issues];
#endif
		}

		#endregion

		#region Initialization

		public void Initialize()
		{
			IsFilterBySet = false;
			FilteredACategoryIds = new List<string>();
			FilteredBCategoryIds = new List<string>();
			FilteredStatus = new List<string>();
			FilteredSeverity = new List<string>();
			CurrentPageNumber = 1;
		}

		#endregion

		#region Event Handlers

		public void FilterByStatus(IssueStatus status)
		{

		}

		public virtual void HighlightClash(Issue issue)
		{

		}

		public virtual async Task<bool> ResolveIssueAsync(Issue issue)
		{
			return true;
		}

		public virtual void Update()
		{
			FilterSets.Clear();
			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					FilterSets.Add(detectionSet.Name, detectionSet);
				}
			}

			CurrentPageNumber = 1;
		}

		public virtual void ClearAll()
		{
			
		}

		public virtual void Reset()
		{

		}

		public virtual void Report()
		{

		}

		#endregion

		#region Helper Methods

		public void UpdateFilterSets()
		{
			FilterSets.Clear();
			Dictionary<string, object> temp = new Dictionary<string, object>();

			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					temp.Add(detectionSet.Name, detectionSet);
				}
			}

			FilterSets = temp;

			if (FilterSets.Count > 0)
			{
				SelectedFilterSets = FilterSets.Take(1).ToDictionary(x => x.Key, x => x.Value);
			}
		}

		public void UpdateByScopeBox()
		{
			IsFilterBySet = false;

		}

		public virtual void UpdateIssues()
		{
			Issues.Clear();
			List<Issue> issues = new List<Issue>();

			if (Project != null)
			{
				// Filter by selected detection sets
				List<string> selDetectionSetIds = new List<string>();
				foreach (var obj in SelectedFilterSets)
				{
					if (obj.Value is ClashDetectionSet detectionSet)
					{
						selDetectionSetIds.Add(detectionSet.Id.ToString());
					}
				}

				int counterPerPage = Constants.ISSUE_COUNTER_PER_PAGE;

				int no = 1 + (CurrentPageNumber - 1) * counterPerPage;

				FilterCriteria filterCriteria = new FilterCriteria()
				{
					ProjectId = Project.Id,
					IsFilterBySet = IsFilterBySet,
					IsFilterByScope = IsFilterByScope,
					IsFilterByHeader = IsFilterByHeader,
					SelectedDetectionSets = selDetectionSetIds,
					ScopeBoxId = CurrentScopeBoxId,
					CategoryAIds = FilteredACategoryIds,
					CategoryBIds = FilteredBCategoryIds,
					Statuses = FilteredStatus,
					Severities = FilteredSeverity,
					PageNumber = CurrentPageNumber,
				};

				long totalCount = IssueTableAdapter.Instance.GetTotalCount(filterCriteria);
				PageCount = (int)((totalCount / counterPerPage) + (totalCount % counterPerPage == 0 ? 0 : 1));

				var filterIssues = IssueTableAdapter.Instance.GetByFilter(filterCriteria);

				foreach (var obj in filterIssues)
				{
					if (obj is Issue issue)
					{
						issue.No = no;
						issues.Add(issue);
						no++;
					}
				}

				Issues = [.. issues];

				if(Issues.Count > 0)
				{
					SelectedIssue = Issues[0];
				}
			}
		}

		public bool SetFilters(string headerName)
		{
			HeaderName = headerName;
			string columnName = "";
			List<string> selectedItems = new List<string>();
			switch (headerName)
			{
				case "ElementA":
					columnName = "CategoryA";
					selectedItems = FilteredACategoryIds;
					break;
				case "ElementB":
					columnName = "CategoryB";
					selectedItems = FilteredBCategoryIds;
					break;
				case "Status":
					columnName = "Status";
					selectedItems = FilteredStatus;
					break;
				case "Severity":
					columnName = "Severity";
					selectedItems = FilteredSeverity;
					break;
				default:
					break;
			}
			List<SelectableItem> temp = new List<SelectableItem>();

			switch (headerName)
			{
				case "ElementA":
				case "ElementB":
					foreach (var id in IssueTableAdapter.Instance.GetCategories(columnName))
					{
						var category = CategoryTableAdapter.Instance.GetById(id) as CSCategory;
						temp.Add(new SelectableItem()
						{
							Id = category.Id,
							Name = category.Name,
							IsSelected = selectedItems != null && selectedItems.Contains(id.ToString()),
						});
					}
					break;
				case "Status":
					foreach (var value in IssueTableAdapter.Instance.GetFilterNames(columnName))
					{
						IssueStatus status = (IssueStatus)Convert.ToInt32(value);

						temp.Add(new SelectableItem()
						{
							Id = (int)status,
							Name = status.ToString(),
							IsSelected = selectedItems != null && selectedItems.Contains(value),
						});
					}
					break;
				case "Severity":
					foreach (var name in IssueTableAdapter.Instance.GetFilterNames(columnName))
					{
						temp.Add(new SelectableItem()
						{
							Name = name,
							IsSelected = selectedItems != null && selectedItems.Contains(name),
						});
					}
					break;
				default:
					break;
			}


			Filters = [.. temp];

			return Filters.Count > 0;
		}

		public void FilterByHeader()
		{
			switch (HeaderName)
			{
				case "ElementA":
					FilteredACategoryIds = Filters.Where(x => x.IsSelected).Select(x => x.Id.ToString()).ToList();
					IsFilterByHeader = FilteredACategoryIds.Any();
					break;
				case "ElementB":
					FilteredBCategoryIds = Filters.Where(x => x.IsSelected).Select(x => x.Id.ToString()).ToList();
					IsFilterByHeader = FilteredBCategoryIds.Any();
					break;
				case "Status":
					FilteredStatus = Filters.Where(x => x.IsSelected).Select(x => x.Id.ToString()).ToList();
					IsFilterByHeader = FilteredStatus.Any();
					break;
				case "Severity":
					FilteredSeverity = Filters.Where(x => x.IsSelected).Select(x => x.Name).ToList();
					IsFilterByHeader = FilteredSeverity.Any();
					break;
				default:
					break;
			}

			CurrentPageNumber = 1;
		}
		#endregion
	}
}
