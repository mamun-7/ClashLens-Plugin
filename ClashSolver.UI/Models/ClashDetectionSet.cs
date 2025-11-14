using System.Collections.ObjectModel;
using System.Linq;

namespace ClashSolver.UI.Models
{
	public class ClashDetectionSet : SelectableItem
	{
		#region Fields

		private long _projectId;

		// For Clash Pair Set
		private bool _isIncludeLink = false;
		private LinkedModel _aLinkedModel = new LinkedModel();
		private LinkedModel _bLinkedModel = new LinkedModel();
		private ObservableCollection<CSCategory> _aElementCategories = [];
		private ObservableCollection<CSCategory> _bElementCategories = [];
		private long _blinkInstanceId = -1;

		// For Tolerance
		private double _globalTolerance = 0;
		private bool _isDynamicOnSize = false;
		private bool _isSystemSpecific = false;
		public SeverityLevel _severityLevel = new SeverityLevel();

		private ObservableCollection<LinkedModel> _aLinkedModels = [];
		private ObservableCollection<LinkedModel> _bLinkedModels = [];


		#endregion

		#region Properties
		
		public long ProjectId 
		{
			get => _projectId;
			set
			{
				_projectId = value;
				OnPropertyChanged(nameof(ProjectId));
			}
		}

		public bool IsIncludeLink
		{
			get => _isIncludeLink;
			set
			{
				_isIncludeLink = value;
				OnPropertyChanged(nameof(IsIncludeLink));
			}
		}

		public ObservableCollection<CSCategory> AElementCategories
		{
			get => _aElementCategories;
			set
			{
				_aElementCategories = value;
				OnPropertyChanged(nameof(AElementCategories));
			}
		}

		public ObservableCollection<LinkedModel> ALinkedModels
		{
			get => _aLinkedModels;
			set
			{
				_aLinkedModels = value;
				OnPropertyChanged(nameof(ALinkedModels));
			}
		}

		public LinkedModel ALinkedModel
		{
			get => _aLinkedModel;
			set
			{
				_aLinkedModel = value;
				OnPropertyChanged(nameof(ALinkedModel));
			}
		}

		public ObservableCollection<LinkedModel> BLinkedModels
		{
			get => _bLinkedModels;
			set
			{
				_bLinkedModels = value;
				OnPropertyChanged(nameof(BLinkedModels));
			}
		}

		public LinkedModel BLinkedModel
		{
			get => _bLinkedModel;
			set
			{
				_bLinkedModel = value;
				OnPropertyChanged(nameof(BLinkedModel));
			}
		}

		public ObservableCollection<CSCategory> BElementCategories
		{
			get => _bElementCategories;
			set
			{
				_bElementCategories = value;
				OnPropertyChanged(nameof(BElementCategories));
			}
		}

		public long BlinkInstanceId
		{
			get => _blinkInstanceId;
			set
			{
				_blinkInstanceId = value;
				OnPropertyChanged(nameof(BlinkInstanceId));
			}
		}

		public double GlobalTolerance
		{
			get { return _globalTolerance; }
			set
			{
				_globalTolerance = value;
				OnPropertyChanged(nameof(GlobalTolerance));
			}
		}

		public bool IsDynamicOnSize
		{
			get => _isDynamicOnSize;
			set
			{
				_isDynamicOnSize = value;
				OnPropertyChanged(nameof(IsDynamicOnSize));
			}
		}

		public bool IsSystemSpecific
		{
			get => _isSystemSpecific;
			set
			{
				_isSystemSpecific = value;
				OnPropertyChanged(nameof(IsSystemSpecific));
			}
		}

		public SeverityLevel SeverityLevel
		{
			get { return _severityLevel; }
			set
			{
				_severityLevel = value;
				OnPropertyChanged(nameof(SeverityLevel));
			}
		}

		#endregion

		#region Methods

		public string GetSelectedCategoryA()
		{
			string temp = string.Join(",", AElementCategories.Where(x => x.IsSelected).Select(x => x.Id).ToArray()) ;

			return temp;
		}

		public string GetSelectedCategoryB()
		{
			string temp = string.Join(",", BElementCategories.Where(x => x.IsSelected) .Select(x => x.Id).ToArray());

			return temp;
		}

		#endregion
	}
}
