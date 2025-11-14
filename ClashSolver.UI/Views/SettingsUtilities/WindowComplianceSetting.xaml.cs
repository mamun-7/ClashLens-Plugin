#region Namespaces
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
using ClashSolver.UI.Controllers;
#endregion

namespace ClashSolver.UI.SettingsUtilities
{
	/// <summary>
	/// Interaction logic for ComplianceSettings.xaml
	/// </summary>
	public partial class WindowComplianceSetting : Window, IExternal
	{
		private readonly ComplianceSettingsUIController _formController = new ComplianceSettingsUIController();
		private bool _isDisposed = false;

		public WindowComplianceSetting()
		{
			InitializeComponent();

			DataContext = _formController;
		}

		public WindowComplianceSetting(ComplianceSettingsUIController formController)
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

		#region Event Handlers

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{

		}

		private void bntnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnCopilot_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Wait for next version");
		}

		#endregion
	}
}
