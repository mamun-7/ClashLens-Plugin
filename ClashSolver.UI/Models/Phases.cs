
namespace ClashSolver.UI.Models
{
	public class CSPhase : BaseModel
	{
		#region Fields

		private int _no = 0;
		private long _elementId = 0;
		private string _name = "";
		private string _description = "";

		#endregion

		#region Properties

		public int No
		{
			get { return _no; }
			set
			{
				_no = value;
				OnPropertyChanged(nameof(No));
			}
		}

		public long ElementId
		{
			get { return _elementId; }
			set
			{
				_elementId = value;
				OnPropertyChanged(nameof(ElementId));
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}
		public string Description
		{
			get { return _description; }
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Description));
			}
		}

		#endregion
	}

	public class LinkedPhase : CSPhase
	{
		#region Fields

		private LinkedModel _linkedModel = new LinkedModel();
		private CSPhase _selectedPhase = new CSPhase();

		#endregion

		#region Properties

		public LinkedModel LinkedModel
		{
			get
			{
				return _linkedModel;
			}
			set
			{
				_linkedModel = value;
				OnPropertyChanged(nameof(LinkedModel));
			}
		}

		public CSPhase SelectedPhase
		{
			get => _selectedPhase;
			set
			{
				_selectedPhase = value;
				OnPropertyChanged(nameof(SelectedPhase));
			}
		}

		#endregion
	}

	public class PhaseMatch : CSPhase
	{
		#region Fields

		private int _match;

		#endregion

		#region Properties

		public int Match
		{
			get { return _match; }
			set
			{
				_match = value;
				OnPropertyChanged(nameof(Match));
			}
		}

		#endregion

		#region Constructors

		public PhaseMatch(int no, string name, string description, int match)
		{
			Match = match;
		}

		#endregion
	}
}
