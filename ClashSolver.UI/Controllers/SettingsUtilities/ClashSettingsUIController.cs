using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.UI.Controllers
{
	public class ClashSettingsUIController : BaseUIController
	{
		#region Fields

		private ObservableCollection<ClashDetectionSet> _sets = new ObservableCollection<ClashDetectionSet>();
		private ClashDetectionSet _selectedSet = new ClashDetectionSet();
		private ObservableCollection<LinkedModel> _aLinkedModels = new ObservableCollection<LinkedModel>();
		private ObservableCollection<LinkedModel> _blinkedModels = new ObservableCollection<LinkedModel>();

		// Can edit DetectionSetTab
		private bool _isSetTabEnabled = false;

		#endregion

		#region Properties

		public ObservableCollection<ClashDetectionSet> Sets
		{
			get => _sets;
			set
			{
				_sets = value;
				OnPropertyChanged(nameof(Sets));
			}
		}

		public ClashDetectionSet SelectedSet
		{
			get => _selectedSet;
			set
			{
				_selectedSet = value;
				OnPropertyChanged(nameof(SelectedSet));
			}
		}

		public ObservableCollection<LinkedModel> ALinkedModels
		{
			get => _aLinkedModels;
			set
			{
				_aLinkedModels = value;
				OnPropertyChanged(nameof(ALinkedModels));
			}
		}


		public ObservableCollection<LinkedModel> BLinkedModels
		{
			get => _blinkedModels;
			set
			{
				_blinkedModels = value;
				OnPropertyChanged(nameof(BLinkedModels));
			}
		}

		public bool IsSetTabEnabled
		{
			get => _isSetTabEnabled;
			set
			{
				_isSetTabEnabled = value;
				OnPropertyChanged(nameof(IsSetTabEnabled));
			}
		}

		#endregion

		#region Constructors

		//	Test Constructor for UITest
		public ClashSettingsUIController() 
		{
#if UITEST
			Project = ProjectTableAdapter.Instance.GetById(1) as Project;

			foreach (var obj in DetectionSetTableAdapter.Instance.GetByProjectId(Project.Id))
			{
				if (obj is ClashDetectionSet detectionSet)
				{
					Sets.Add(detectionSet);
				}
			}

			if (Sets.Count > 0)
			{
				SelectedSet = Sets[0];
			}
#endif
		}

		#endregion

		#region Event Handlers

		public virtual void Initialize()
		{

		}

		public virtual void OnAdd(string name)
		{

		}

		public virtual void OnDuplicate(int nIndex)
		{
			
		}

		public virtual void OnRename(string name)
		{
			if (SelectedSet == null)
				return;

			if (string.IsNullOrWhiteSpace(name))
				return;

			SelectedSet.Name = name;
			DetectionSetTableAdapter.Instance.Update(SelectedSet);
		}

		public virtual void OnRemove()
		{
			if (SelectedSet == null)
				return;

			long id = SelectedSet.Id;
			long res = DetectionSetTableAdapter.Instance.Delete(id);
			
			if(res > 0)
			{
				int selIndex = Sets.IndexOf(SelectedSet);
				Sets.Remove(SelectedSet);
				OnPropertyChanged(nameof(Sets));
			}
		}

		public override void OnOK()
		{
			foreach(var set in Sets)
			{
				set.BlinkInstanceId = set.BLinkedModel.InstanceId;

				DetectionSetTableAdapter.Instance.Update(set);
			}
		}

		#endregion

		#region Helper Methods

		public virtual List<long> GetUsedElementCategories(LinkedModel linkedModel = null)
		{
			List<long> res = new List<long>();

			return res;
		}

		public void UpdateClashPairSet()
		{
			if (SelectedSet == null)
				return;
		}

		public virtual void UpdateLinkModels()
		{

		}

		public virtual void UpdateAElementCategories()
		{

			if(SelectedSet == null || !(SelectedSet.Id > 0))
			{
				return;
			}

			var usedCategories = GetUsedElementCategories(SelectedSet.ALinkedModel);
			var dbCategories = CategoryTableAdapter.Instance.GetAll().Select(x => x as CSCategory);

			List<CSCategory> aCategories = [];

			foreach (var dbCategory in dbCategories)
			{
				if (usedCategories.Contains(dbCategory.ElementId))
				{
					aCategories.Add(new CSCategory()
					{
						Id = dbCategory.Id,
						ElementId = dbCategory.ElementId,
						Name = dbCategory.Name,
						IsSelected = false,
						Type = dbCategory.Type,
						Version = dbCategory.Version
					});
				}
			}

			SelectedSet.AElementCategories = [..aCategories];
		}

		public virtual void UpdateBElementCategories()
		{
			if (SelectedSet == null || !(SelectedSet.Id > 0))
			{
				return;
			}

			var usedCategories = GetUsedElementCategories(SelectedSet.BLinkedModel);
			var dbCategories = CategoryTableAdapter.Instance.GetAll().Select(x => x as CSCategory);

			List<CSCategory> bElementCategories = [];

			foreach (var dbCategory in dbCategories)
			{
				if (usedCategories.Contains(dbCategory.ElementId))
				{
					bElementCategories.Add(new CSCategory()
					{
						Id = dbCategory.Id,
						ElementId = dbCategory.ElementId,
						Name = dbCategory.Name,
						IsSelected = false,
						Type = dbCategory.Type,
						Version = dbCategory.Version
					});
				}
			}

			SelectedSet.BElementCategories = [.. bElementCategories];
		}

		#endregion
	}
}
