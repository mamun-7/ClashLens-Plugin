using ClashSolver.UI.Models;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace ClashSolver.UI.Controllers
{
	public class ManageTeamUIController : BaseUIController
	{
		#region Fields

		#endregion

		#region Properties

		public ObservableCollection<LinkedModel> LinkedModels { get; set; }

		#endregion

		#region Constructors

		//	Test Constructor for UITest
		public ManageTeamUIController()
		{

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
