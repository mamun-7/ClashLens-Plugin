using System;
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
using Architexor.Core;
using ClashSolver.UI.Controllers;

namespace ClashSolver.UI.Views.ACCIntegration
{
	/// <summary>
	/// Interaction logic for WndConfiguration.xaml
	/// </summary>
	public partial class WndConfiguration : Window, IExternal
	{
		private readonly ConfigurationUIController _formController;
		private bool _isDisposed = false;

		public WndConfiguration()
		{
			InitializeComponent();
		}

		public WndConfiguration(ConfigurationUIController controller)
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

		private async void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (_formController.IsLoggedin)
			{
				_formController.OnLogout();
			}
			else
			{
				await _formController.OnLogin();
			}
    }

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
