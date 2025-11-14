
namespace ClashSolver.UI.Models
{
	public class CSCategory : SelectableItem
	{
		#region Fields

		private long _elementId;
		private string _type;
		private string _version;

		#endregion

		#region Properties

		public long ElementId
		{
			get { return _elementId; }
			set { _elementId = value; OnPropertyChanged(nameof(ElementId)); }
		}

		public string Type
		{
			get { return _type; }
			set { _type = value; OnPropertyChanged(nameof(Type)); }
		}

		public string Version
		{
			get => _version;
			set => _version = value;
		}

		public CSCategory(int id, string name)
		{
			Name = name;
			Id = id;
		}

		#endregion

		#region Constructors

		public CSCategory()
		{

		}

		#endregion;
	}
}
