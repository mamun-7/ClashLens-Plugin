using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.Views.SettingsUtilities;

namespace ClashSolver.UI.SettingsUtilities
{
	/// <summary>
	/// Interaction logic for ClashSettings.xaml
	/// </summary>
	public partial class WindowClashSetting : Window, IExternal
	{

		private readonly ClashSettingsUIController _formController;
		private bool _isDisposed = false;


		private bool _isCategoryAFocused = false;
		private bool _isCategoryBFocused = false;

		public WindowClashSetting(ClashSettingsUIController formController)
		{
			InitializeComponent();

			_formController = formController;
			_formController.Initialize();
			DataContext = _formController;
			
			WakeUp();

			Closed += OnClosed;

			listClashDetectionSet.SelectedItem = _formController.SelectedSet;

			// Subscribe to the SelectionChanged evnets
			cmbCompareCategoryA.SelectionChanged += cmbCompareCategoryA_SelectionChanged;
			cmbCompareCategoryB.SelectionChanged += cmbCompareCategoryB_SelectionChanged;
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


		#region Event handlers

		private void btnNew_Click(object sender, RoutedEventArgs e)
		{
			InputDialog inputDialog = new InputDialog()
			{
				Owner = this
			};
			if (inputDialog.ShowDialog() == true)
			{
				string newName = inputDialog.InputName;

				if(!string.IsNullOrEmpty(newName))
				{
					_formController.OnAdd(newName);
				}
			}

		}

		private void btnDuplicate_Click(object sender, RoutedEventArgs e)
		{
			int nIndex = listClashDetectionSet.SelectedIndex;
			if (nIndex < 0) return;
			_formController.OnDuplicate(nIndex);
		}

		private void btnRename_Click(object sender, RoutedEventArgs e)
		{
			int nIndex = listClashDetectionSet.SelectedIndex;
			if (nIndex < 0) return;
			// Getting the curently selected ListBoxItem
			// Note that the Listbox must have
			// IsSynchronizedWithCurrentItem set to True for this to work
			ListBoxItem listBoxItem = listClashDetectionSet.ItemContainerGenerator.ContainerFromIndex(nIndex) as ListBoxItem;

			// Getting the ContentPresenter of myListBoxItem
			ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);

			// Finding textBox from the DataTemplate that is set on that contentpresenter
			DataTemplate dataTemplate = contentPresenter.ContentTemplate;
			TextBox textBox = dataTemplate.FindName("txtName", contentPresenter) as TextBox;
			textBox.IsReadOnly = false;
			textBox.SelectAll();
		}

		private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
		{
			if (obj == null) return null;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is childItem)
				{
					{
						return (childItem)child;
					}
				}
				else
				{
					childItem childOfChild = FindVisualChild<childItem>(child);
					if (childOfChild != null)
						return childOfChild;
				}
			}

			return null;
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			int nIndex = listClashDetectionSet.SelectedIndex;
			if (nIndex < 0) return;

			_formController.OnRemove();
		}

		private void txtName_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox temp = sender as TextBox;

			_formController.OnRename(temp.Text);
		}

		private void btnClearAll_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
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
			MessageBox.Show("Wait for next version");
		}

		private void txtName_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				TextBox textBox = sender as TextBox;
				textBox.IsReadOnly = true;
			}
		}

		private void txtName_LostFocus(object sender, RoutedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			textBox.IsReadOnly = true;
		}

		private void cmbCompareCategoryA_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_isCategoryAFocused)
			{
				_formController.UpdateAElementCategories();
			}

			_isCategoryAFocused = true;
		}

		private void cmbCompareCategoryB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(_isCategoryBFocused)
			{
				_formController.UpdateBElementCategories();
			}

			_isCategoryBFocused = true;
		}

		private void chkIncludeLink_Checked(object sender, RoutedEventArgs e)
		{
			_formController.UpdateLinkModels();
		}

		#endregion

		private void listClashDetectionSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Unsubscribe from the SelectionChanged events
			cmbCompareCategoryA.SelectionChanged -= cmbCompareCategoryA_SelectionChanged;
			cmbCompareCategoryB.SelectionChanged -= cmbCompareCategoryB_SelectionChanged;

			// Change the selected set
			_formController.SelectedSet = (ClashDetectionSet)listClashDetectionSet.SelectedItem;

			// Resubscribe to the SelectionChanged events
			cmbCompareCategoryA.SelectionChanged += cmbCompareCategoryA_SelectionChanged;
			cmbCompareCategoryB.SelectionChanged += cmbCompareCategoryB_SelectionChanged;
		}
	}
}
