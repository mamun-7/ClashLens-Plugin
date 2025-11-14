using System.Collections.Generic;
using System.Windows;

namespace ClashSolver.UI.Models
{
	public class MultiSelectComboBoxItemViewModel : BaseModel
	{
		private Dictionary<string, object> _items;
		private Dictionary<string, object> _selectedItems;

		public Dictionary<string, object> Items
		{
			get
			{
				return _items;
			}
			set
			{
				_items = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		public Dictionary<string, object> SelectedItems
		{
			get
			{
				return _selectedItems;
			}
			set
			{
				_selectedItems = value;
				OnPropertyChanged(nameof(SelectedItems));
			}
		}

		public MultiSelectComboBoxItemViewModel()
		{
			_items = new Dictionary<string, object>();
			Items.Add("Architecture", "Architecture");
			Items.Add("Structure", "Structure");
			Items.Add("Electrical", "Electrical");
			Items.Add("Mechanical", "Mechanical");

			_selectedItems = new Dictionary<string, object>();
			SelectedItems.Add("Architecture", "Architecture");
		}

		private void Submit()
		{
			foreach (KeyValuePair<string, object> item in SelectedItems)
				MessageBox.Show(item.Key);
		}
	}
}
