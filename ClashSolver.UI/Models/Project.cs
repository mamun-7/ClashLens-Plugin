
namespace ClashSolver.UI.Models
{
	public class Project : BaseModel
	{
		#region Fields

		private long _id;
		private string _name;
		private string _uniqueId;
		private string _path;
		private string _version;
		private string _validationTime;

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

		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		public string UniqueId
		{
			get { return _uniqueId; }
			set { _uniqueId = value; }
		}

		public string Version
		{
			get => _version;
			set => _version = value;
		}

		public string ValidationTime
		{
			get => _validationTime;
			set => _validationTime = value;
		}

		#endregion

		#region Constructors

		public Project(int id, string name)
		{
			_name = name;
			_id = id;
		}

		public Project()
		{

		}

		#endregion
	}
}
