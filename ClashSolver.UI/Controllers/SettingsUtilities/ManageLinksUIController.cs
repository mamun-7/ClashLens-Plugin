using ClashSolver.UI.Models;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace ClashSolver.UI.Controllers
{
	public class ManageLinksUIController : BaseUIController
	{
		#region Fields

		protected double _longitude;
		protected double _latitude;
		protected double _angle;

		#endregion

		#region Properties

		public double Longitude
		{
			get => _longitude;
			set
			{
				_longitude = value;
				OnPropertyChanged(nameof(Longitude));
			}
		}
		public double Latitude
		{
			get => _latitude;
			set
			{
				_latitude = value;
				OnPropertyChanged(nameof(Latitude));
			}
		}
		public double Angle
		{
			get => _angle;
			set
			{
				_angle = value;
				OnPropertyChanged(nameof(Angle));
			}
		}

		public ObservableCollection<LinkedModel> LinkedModels { get; set; }

		#endregion

		#region Constructors

		//	Test Constructor for UITest
		public ManageLinksUIController()
		{
#if UITEST
			_longitude = -71.0567398071289;
			_latitude = 42.3586616516113;
			_angle = 0;

			LinkedModels = new ObservableCollection<LinkedModel>();
			LinkedModels.Add(new LinkedModel()
			{
				No = 1,
				Name = "Test",
				Url = "D:\\Test.rvt",
				Discipline = "Architecture",
				Description = "Test Architecture model"
			});
#endif
		}

		#endregion

		#region Event Handlers

		public override void OnOK()
		{

		}

		public void OnAdd()
		{


		}

		public void OnRemove(int nIndex)
		{
			if (nIndex < 0)
				return;

			LinkedModels.RemoveAt(nIndex);
			UpdateRowNumbers();
		}

		public void OnMoveUp(int nIndex)
		{
			if (nIndex > 0)
			{
				LinkedModels.Move(nIndex, nIndex - 1);
				UpdateRowNumbers();
			}
		}

		public void OnMoveDown(int nIndex)
		{
			if (nIndex < 0)
				return;

			if (nIndex < LinkedModels.Count - 1)
			{
				LinkedModels.Move(nIndex, nIndex + 1);
				UpdateRowNumbers();
			}
		}

		public void OnBrowse(int nIndex)
		{
			if (nIndex < 0)
				return;

			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Revit Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
				Title = "Select Linked Model"
			};

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				LinkedModels[nIndex].Url = openFileDialog.FileName;
			}
		}

		#endregion

		#region Helper Methods

		public void UpdateRowNumbers()
		{
			for (int i = 0; i < LinkedModels.Count; i++)
			{
				LinkedModels[i].No = i + 1;
			}
		}

		#endregion
	}
}
