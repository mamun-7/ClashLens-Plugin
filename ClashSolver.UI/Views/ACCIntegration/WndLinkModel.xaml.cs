using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
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
using ClashSolver.UI.Controllers;
using ForgeAPI.Models;

namespace ClashSolver.UI.Views.ACCIntegration
{
	/// <summary>
	/// Interaction logic for Configuration.xaml
	/// </summary>
	public partial class WndLinkModel : Window, IExternal
	{
		private readonly LinkModelUIController _formController;
		private bool _isDisposed = false;

		public WndLinkModel()
		{
			InitializeComponent();
		}

		public WndLinkModel(LinkModelUIController controller)
		{
			InitializeComponent();
			_formController = controller;
			DataContext = _formController;
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
			EnableCommands(false);
		}

		private void EnableCommands(bool status)
		{

		}

		public void WakeUp(bool bFinish = false)
		{
			if (bFinish)
			{
				Close();
				return;
			}

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

		#region Event Handler
		private void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			_formController.UpdateFolders();
		}

		private void btnLink_Click(object sender, RoutedEventArgs e)
		{
			

		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#endregion

		private void treeFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var item = e.NewValue as ACCFolder;
			if (item != null)
			{
				_formController.SelectedFolder = item;
			}
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			_formController.GetProjects();
		}

		private void btnUpload_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnUpload();
        }
    }
}
