using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.UI;
using ClashSolver.UI;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using ClashSolver.UI.UIComponents;

namespace ClashSolver.Forms
{
	/// <summary>
	/// Interaction logic for IssueDocPanel.xaml
	/// </summary>
	public partial class IssueDockPanel : Page, IDockablePaneProvider
	{
		#region Fields

		ReviewIssuesUIController _formController = new ReviewIssuesUIController();

		#endregion

		#region Initialization

		public IssueDockPanel(ReviewIssuesUIController formController)
		{
			InitializeComponent();

			_formController = formController;
			DataContext = _formController;
		}

		#endregion

		#region IDockablePaneProvider Implementation

		public void SetupDockablePane(DockablePaneProviderData data)
		{
			data.FrameworkElement = this;
		}

		#endregion

		#region Event Handler

		private void btnSelAll_Click(object sender, RoutedEventArgs e)
		{
			gridIssues.SelectAll();
		}

		private void btnSelNone_Click(object sender, RoutedEventArgs e)
		{
			gridIssues.SelectedItems.Clear();
		}

		private void btnSelInvert_Click(object sender, RoutedEventArgs e)
		{
			// Get a copy of the currently selected items
			var selectedItems = new List<object>((IEnumerable<object>)gridIssues.SelectedItems);

			// Clear the current selection
			gridIssues.UnselectAll();

			// Iterate through all items in the DataGrid
			foreach (var item in gridIssues.Items)
			{
				// Toggle the selection state
				if (!selectedItems.Contains(item))
				{
					gridIssues.SelectedItems.Add(item); // Select previously unselected items
				}
				else
				{
					gridIssues.SelectedItems.Remove(item); // Unselect previously selected items
				}
			}
		}

		private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int nIndex = cmbStatus.SelectedIndex;
			IssueStatus status = (IssueStatus)nIndex;

			_formController.FilterByStatus(status);
		}

		private void gridIssues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var issue = gridIssues.SelectedItem as Issue;

			if (issue != null)
			{
				_formController.HighlightClash(issue);
			}
		}

		private async void btnResolution_Click(object sender, RoutedEventArgs e)
		{
			bool res = false;

			var issue = gridIssues.SelectedItem as Issue;

			if (issue != null)
			{
				_formController.HighlightClash(issue);
				res = await _formController.ResolveIssueAsync(issue);
			}
		}

		private void btnPrevious_Click(object sender, RoutedEventArgs e)
		{
			int temp = _formController.CurrentPageNumber;
			if (temp > 1)
				_formController.CurrentPageNumber = temp - 1;
		}

		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			int temp = _formController.CurrentPageNumber;
			if (temp < _formController.PageCount)
				_formController.CurrentPageNumber = temp + 1;
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			_formController.Reset();
		}

		private void btnClearAll_Click(object sender, RoutedEventArgs e)
		{
			_formController.ClearAll();
		}

		private void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			_formController.CurrentPageNumber = 1;
		}

		private void cmbIssueStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;

			if (cmb == null)
				return;

			if (cmb.DataContext is Issue issue)
			{
				issue.Status = (IssueStatus)(cmb.SelectedIndex + 1);
				IssueTableAdapter.Instance.Update(issue);
			}
		}

		private void cmbAssignTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;

			if (cmb == null)
				return;

			if (cmb.DataContext is Issue issue)
			{
				issue.AssignedBy = (LinkDiscipline)(cmb.SelectedIndex + 1);
				IssueTableAdapter.Instance.Update(issue);
			}
		}

		private void btnReport_Click(object sender, RoutedEventArgs e)
		{
			_formController.Report();
		}

		private void btnImport_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			_formController.FilterByHeader();

			_formController.Update();

			// Close the Popup
			var button = sender as Button;
			var popup = GetParentPopup(button);
			if (popup != null)
			{
				popup.IsOpen = false;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			// Close the Popup
			var button = sender as Button;
			var popup = GetParentPopup(button);
			if (popup != null)
			{
				popup.IsOpen = false;
			}
		}

		private Popup GetParentPopup(DependencyObject child)
		{
			while (child != null)
			{
				if (child is Popup popup)
				{
					return popup;
				}
				child = LogicalTreeHelper.GetParent(child);
			}
			return null;
		}

		private void DataGridIssue_LostFocus(object sender, RoutedEventArgs e)
		{
			((SolidColorBrush)gridIssues.Resources["SelectionColorKey"]).Color = SystemColors.HighlightColor;
		}

		private void DataGridIssue_GotFocus(object sender, RoutedEventArgs e)
		{
			((SolidColorBrush)gridIssues.Resources["SelectionColorKey"]).Color = SystemColors.HighlightColor;
		}

		private void DataGridIssue_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			((SolidColorBrush)gridIssues.Resources["SelectionColorKey"]).Color = SystemColors.HighlightColor;
		}

		private void DataGridIssues_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				Console.WriteLine("DataGrid got focus");
			}
			else
			{
				((SolidColorBrush)gridIssues.Resources["SelectionColorKey"]).Color = Colors.DarkBlue;
			}
		}
		#endregion

		private void ShowHideColumns_Click(object sender, RoutedEventArgs e)
		{
			//var dialog = new WindowColumnVisibility(gridIssues.Columns);
			//dialog.Owner = Window.GetWindow(this);
			//dialog.ShowDialog();
		}
	}
	public class DataGridHelper
	{
		public static bool GetAutoFocus(DependencyObject obj) => (bool)obj.GetValue(AutoFocusProperty);
		public static void SetAutoFocus(DependencyObject obj, bool value) => obj.SetValue(AutoFocusProperty, value);

		public static readonly DependencyProperty AutoFocusProperty =
				DependencyProperty.RegisterAttached("AutoFocus", typeof(bool), typeof(DataGridHelper),
						new PropertyMetadata(false, OnAutoFocusChanged));

		private static void OnAutoFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is DataGrid dataGrid && (bool)e.NewValue)
			{
				dataGrid.SelectionChanged += (s, _) =>
				{
					if (dataGrid.SelectedItem != null)
					{
						dataGrid.Dispatcher.BeginInvoke(new Action(() =>
						{
							// Ensure the DataGrid itself has focus
							dataGrid.Focus();

							// Get the selected row
							DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem);
							if (row != null)
							{
								// Simulate a full row selection like mouse click
								row.Focus();
								Keyboard.Focus(row); // Ensures deep blue selection
							}
						}), System.Windows.Threading.DispatcherPriority.Render);
					}
				};
			}
		}
	}
}
