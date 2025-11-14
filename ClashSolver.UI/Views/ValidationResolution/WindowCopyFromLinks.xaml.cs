using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ClashSolver.UI.ValidationResolution.CopyFromLinks
{
	/// <summary>
	/// Interaction logic for WindowCopyFromLinks.xaml
	/// </summary>
	public partial class WindowCopyFromLinks : Window, IExternal
	{
		private readonly CopyFromLinksUIController _formController;
		private bool _isDisposed = false;

		public WindowCopyFromLinks(CopyFromLinksUIController controller)
		{
			InitializeComponent();

			_formController = controller;
			DataContext = _formController;

			Initialize();

			WakeUp();

			Closed += OnClosed;
		}

		public void Initialize()
		{

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

		#region Event Handlers

		private void listImportedLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int nIndex = listImportedLinks.SelectedIndex;

			if (nIndex < 0)
				return;

			_formController.SetElementCategories(nIndex);
		}

		private void btnChooseAll_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnChooseAll();
		}

		private void btnChoose_Click(object sender, RoutedEventArgs e)
		{

			//foreach(var item in listCategoriesFrom.SelectedItems)
			//{
			//	int nIndex = listCategoriesFrom.Items.IndexOf(item);

			_formController.OnChoose(listCategoriesFrom.SelectedIndex);
			//}
		}

		private void btnDechoose_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnDeChoose(listCategoriesTo.SelectedIndex);
		}

		private void btnDechooseAll_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnDeChooseAll();
		}

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnReset();
		}

		private void btnManageLink_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnRun_Click(object sender, RoutedEventArgs e)
		{
			_formController.OnOK();

			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnCopilot_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Please wait for the next version.");
		}

		#endregion

	}
}
