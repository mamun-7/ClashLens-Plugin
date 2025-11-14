using System.Collections.ObjectModel;

namespace ClashSolver.UI.Models
{
	public class Folder : BaseModel
	{
		#region Fields

		private string _name = "";
		private ObservableCollection<Folder> _subFolders = new ObservableCollection<Folder>();

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public ObservableCollection<Folder> SubFolders 
		{
			get
			{
				return _subFolders;
			}
			set
			{
				_subFolders = value;
				OnPropertyChanged(nameof(SubFolders));
			}
		}

		#endregion
	}
}
