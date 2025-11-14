
namespace ClashSolver.UI.Models
{
	public class ReportContent : SelectableItem
	{
		#region Fields

		private long _id;
		private string _name;
		private string _type;

		#endregion

		#region Properties

		public long Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Type
		{
			get => _type; 
			set => _type = value;
		}

		#endregion
	}
}
