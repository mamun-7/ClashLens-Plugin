using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClashSolver.UI.SettingsUtilities
{
	/// <summary>
	/// Interaction logic for ManageLinks.xaml
	/// </summary>
	public partial class WndManageLinks : Window, IExternal
	{
		private readonly ManageLinksUIController _formController;
		private bool _isDisposed = false;

		public WndManageLinks(ManageLinksUIController formController)
		{
			InitializeComponent();

			_formController = formController;
			DataContext = _formController;

			WakeUp();
			 
			Closed += OnClosed;
		}

		#region IExternal interface implementation
		private void OnClosed(object sender, EventArgs e)
		{
			_isDisposed = true;
		}

		public int GetRequestId()
		{
			return _formController.GetRequestId();
		}

		public void MakeRequest(int request)
		{
			_formController.MakeRequest(request);

			DozeOff();
		}

		public void DozeOff()
		{
			//if (IsLoaded && Visibility == Visibility.Visible)
			//{
			//	Hide();
			//}
			EnableCommands(false);
		}

		private void EnableCommands(bool status)
		{
			//foreach (Control ctrl in Controls)
			//{
			//	ctrl.Enabled = status;
			//}
			//if (!status)
			//{
			//	btnCancel.Enabled = true;
			//}
		}

		public void WakeUp(bool bFinish = false)
		{
			if(bFinish)
			{
				Close();
				return;
			}
			//if(!_isDisposed && Visibility != Visibility.Visible)
			//{
			//	Show();
			//	Activate();
			//}
			_formController.WakeUp(bFinish);
		}

		public void IClose()
		{
			if (!_isDisposed)
			{
				Close();
				_isDisposed = true;
			}
		}

		public bool IVisible()
		{
			//return Visibility == Visibility.Visible;
			return true;
		}

		public bool IIsDisposed()
		{
			return _isDisposed;
		}

		public void IShow()
		{
			if (!_isDisposed)
			{
				ShowDialog();
			}
		}
		#endregion

		#region Event handlers
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			Close();
			_formController.OnOK();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnAdd();
		}

		private void btnRemove_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnRemove(ConsultantLinksGrid.SelectedIndex);
		}

		private void btnUp_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnMoveUp(ConsultantLinksGrid.SelectedIndex);
		}

		private void btnDown_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnMoveDown(ConsultantLinksGrid.SelectedIndex);
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnBrowse(ConsultantLinksGrid.SelectedIndex);
		}
		private void cmbDiscipline_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cmb = sender as ComboBox;

			if (cmb == null) return;

			if (cmb.DataContext is LinkedModel linkedModel)
			{
				linkedModel.Discipline = (LinkDiscipline)(cmb.SelectedIndex + 1);
			}
		}
		#endregion

	}
}
