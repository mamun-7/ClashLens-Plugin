using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;

namespace ClashSolver.UI.Models
{
	public class LinkedModelCopySetting : BaseModel
	{
		#region Fields

		private LinkedModel _linkedModel = new LinkedModel();

		private ObservableCollection<CSCategory> _elementCategories = new ObservableCollection<CSCategory>();

		private ObservableCollection<CSCategory> _selectedElementCategories = new ObservableCollection<CSCategory>();

		#endregion

		#region Properties

		public LinkedModel LinkedModel
		{
			get => _linkedModel;
			set
			{
				_linkedModel = value;
				OnPropertyChanged(nameof(LinkedModel));
			}
		}

		public ObservableCollection<CSCategory> ElementCategories
		{
			get => _elementCategories;
			set
			{
				_elementCategories = value;
				OnPropertyChanged(nameof(ElementCategories));
			}
		}

		public ObservableCollection<CSCategory> SelectedElementCategories
		{
			get => _selectedElementCategories;
			set
			{
				_selectedElementCategories = value;
				OnPropertyChanged(nameof(SelectedElementCategories));
			}
		}

		public List<SelectableItem> UsedFamilies { get; set; }

		public List<SelectableItem> DuplicatedFamilies { get; set; }

		#endregion

		#region Helper Methods

		public List<long> GetFamilyIdsToCopy()
		{
			List<long> familyIds = new List<long>();

			// ignore families that are not selected in Duplicated Families
			var ignoreFamilies = DuplicatedFamilies.Where(x => !x.IsSelected).ToList();
			foreach(var family in  UsedFamilies.Where(x => !ignoreFamilies.Contains(x)).ToList())
			{
				familyIds.Add(family.Id);
			}

			return familyIds;

			#endregion
		}
	}
}
