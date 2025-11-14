using ClashSolver.UI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ClashSolver.UI.Controllers
{
	public class CopyFromLinksUIController : BaseUIController
	{
		#region Fields

		private CopyType _copyType = CopyType.Overwrite;
		private WorkSets _workSets = WorkSets.NOCREATE_FROM_LINK;
		private bool _isOriginalWorksetsCopy = true;
		private GridCopyType _gridCopyType = GridCopyType.FROM_ARCHITECTURE;
		private ObservableCollection<LinkedModel> _linkedModels = new ObservableCollection<LinkedModel>();
		private LinkedModel _selectedCopyModel = new LinkedModel();

		private ObservableCollection<LinkedModelCopySetting> _linkedModelCopySettings = [];

		private LinkedModel _currentLinkedModel = new();
		private ObservableCollection<CSCategory> _currentElementCategories = [];
		private ObservableCollection<CSCategory> _currentSelElementCategories = [];

		#endregion

		#region Properties

		public CopyType CopyType
		{
			get => _copyType;
			set
			{
				_copyType = value;
				OnPropertyChanged(nameof(CopyType));
			}
		}

		public WorkSets WorkSets
		{
			get => _workSets;
			set
			{
				_workSets = value;
				OnPropertyChanged(nameof(WorkSets));
			}
		}

		public bool IsOriginalWorksetsCopy
		{
			get => _isOriginalWorksetsCopy;
			set
			{
				_isOriginalWorksetsCopy = value;
				OnPropertyChanged(nameof(IsOriginalWorksetsCopy));
			}
		}

		public GridCopyType GridCopyType
		{
			get => _gridCopyType;
			set
			{
				_gridCopyType = value;
				OnPropertyChanged(nameof(GridCopyType));
			}
		}

		public LinkedModel SelectedCopyModel
		{
			get => _selectedCopyModel;
			set
			{
				_selectedCopyModel = value;
				OnPropertyChanged(nameof(SelectedCopyModel));
			}
		}

		public ObservableCollection<LinkedModel> LinkedModels
		{
			get => _linkedModels;
			set
			{
				_linkedModels = value;
				OnPropertyChanged(nameof(LinkedModels));
			}
		}

		public ObservableCollection<LinkedModelCopySetting> LinkedModelCopySettings
		{
			get => _linkedModelCopySettings;
			set
			{
				_linkedModelCopySettings = value;
				OnPropertyChanged(nameof(LinkedModelCopySettings));
			}
		}

		public LinkedModel CurrentLinkedModel
		{
			get => _currentLinkedModel;
			set
			{
				_currentLinkedModel = value;
				OnPropertyChanged(nameof(CurrentLinkedModel));
			}
		}

		public ObservableCollection<CSCategory> CurrentElementCategories
		{
			get { return _currentElementCategories; }
			set
			{
				_currentElementCategories = value;
				OnPropertyChanged(nameof(CurrentElementCategories));
			}
		}

		public ObservableCollection<CSCategory> CurrentSelElementCategories
		{
			get { return _currentSelElementCategories; }
			set
			{
				_currentSelElementCategories = value;
				OnPropertyChanged(nameof(CurrentSelElementCategories));
			}
		}

		#endregion

		#region Constructors

		public CopyFromLinksUIController()
		{

		}

		#endregion

		#region Event Handlers

		public void SetElementCategories(int nIndex)
		{
			if (nIndex < LinkedModels.Count)
			{
				CurrentLinkedModel = LinkedModels[nIndex];

				LinkedModelCopySetting selLinkedModelCopySetting = LinkedModelCopySettings.Where(x => x.LinkedModel != null && x.LinkedModel.ElementId == CurrentLinkedModel.ElementId).FirstOrDefault();

				if(selLinkedModelCopySetting == null)
				{
					selLinkedModelCopySetting = GetLinkedModelCopySetting(CurrentLinkedModel);

					if(selLinkedModelCopySetting != null)
					{
						LinkedModelCopySettings.Add(selLinkedModelCopySetting);
					}
				}

				if(selLinkedModelCopySetting != null)
				{
					CurrentElementCategories = selLinkedModelCopySetting.ElementCategories;
					CurrentSelElementCategories = selLinkedModelCopySetting.SelectedElementCategories;
				}
			}
		}

		public void OnChooseAll()
		{
			foreach (var elementCategory in CurrentElementCategories)
			{
				CurrentSelElementCategories.Add(elementCategory);
			}

			CurrentElementCategories.Clear();
		}

		public void OnChoose(int nIndex)
		{
			if (nIndex < 0)
				return;

			CurrentSelElementCategories.Add(CurrentElementCategories[nIndex]);

			CurrentElementCategories.RemoveAt(nIndex);
		}

		public void OnDeChooseAll()
		{
			foreach (var elementCategory in CurrentSelElementCategories)
			{
				CurrentElementCategories.Add(elementCategory);
			}

			CurrentSelElementCategories.Clear();
		}

		public void OnDeChoose(int nIndex)
		{
			if (nIndex < 0)
				return;

			CurrentElementCategories.Add(CurrentSelElementCategories[nIndex]);

			CurrentSelElementCategories.RemoveAt(nIndex);
		}

		public void OnRun()
		{

		}

		public void OnReset()
		{

		}

		public override void OnOK()
		{
			base.OnOK();
		}

		#endregion

		#region Helper Methods

		protected virtual LinkedModelCopySetting GetLinkedModelCopySetting(LinkedModel model)
		{
			return null;
		} 

		#endregion
	}
}