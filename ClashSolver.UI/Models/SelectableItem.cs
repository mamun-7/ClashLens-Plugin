
namespace ClashSolver.UI.Models
{
	public class SelectableItem : BaseModel
	{
		#region Fields

		protected long _id;
		protected bool _isSelected = false;
		protected string _name = "";

		#endregion

		#region Properties

		public long Id
		{
			get { return _id; }
			set { _id = value; OnPropertyChanged(nameof(Id)); }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; OnPropertyChanged(nameof(Name)); }
		}

		#endregion
	}
}
