
namespace ClashSolver.UI.Models
{
	public class Collaborator : BaseModel
	{
		#region Fields

		private int _no;
		private string _name = "";
		private string _email = "";
		private MultiSelectComboBoxItemViewModel _responsibilities;

		#endregion

		#region Properties

		public int No
		{
			get => _no;
			set { _no = value; OnPropertyChanged(nameof(No)); }
		}

		public string Name
		{
			get => _name;
			set { _name = value; OnPropertyChanged(nameof(Name)); }
		}

		public string Email
		{
			get => _email;
			set { _email = value; OnPropertyChanged(nameof(Email)); }
		}

		public MultiSelectComboBoxItemViewModel Responsibilities
		{
			get => _responsibilities;
			set
			{
				_responsibilities = value;
				OnPropertyChanged(nameof(Responsibilities));
			}
		}

		#endregion

		#region Constructors

		public Collaborator()
		{
			_responsibilities = new MultiSelectComboBoxItemViewModel();
		}

		#endregion
	}
}
