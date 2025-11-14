using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace ClashSolver.UI.ValidationResolution.CostDatabase
{
	/// <summary>
	/// Interaction logic for WndCostDatabase.xaml
	/// </summary>
	public partial class WndCostDatabase : Window, IExternal
	{
		private CostDatabaseUIController _formController = new CostDatabaseUIController();
		private bool _isDisposed = false;

		public WndCostDatabase(CostDatabaseUIController formController)
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
			return Visibility == System.Windows.Visibility.Visible;
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
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
