using System;
using System.Collections;
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
using ClashSolver.UI.Models;

namespace ClashSolver.UI.UIComponents
{
	/// <summary>
	/// Interaction logic for SelectControl.xaml
	/// </summary>
	public partial class SelectControl : UserControl
	{

		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<SelectableItem>), typeof(SelectControl), new PropertyMetadata(null));

		public static readonly DependencyProperty ControlWidthProperty = DependencyProperty.Register("ControWidth", typeof(double), typeof(SelectControl), new PropertyMetadata(double.NaN));

		public static readonly DependencyProperty ControlHeightProperty = DependencyProperty.Register("ControlHeight", typeof(double), typeof(SelectControl), new PropertyMetadata(double.NaN));

		public ObservableCollection<SelectableItem> Items
		{
			get { return (ObservableCollection<SelectableItem>)GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		public double ControlWidth
		{
			get { return (double)GetValue(ControlWidthProperty); }
			set { SetValue(ControlWidthProperty, value); }
		}

		public double ControlHeight
		{
			get { return (double)GetValue(ControlHeightProperty); }
			set { SetValue(ControlHeightProperty, value); }
		}
		
		public SelectControl()
		{
			InitializeComponent();
			DataContext = this;
		}

		#region Event Handlers

		private void SelectAll_Click(object sender, RoutedEventArgs e)
		{
			foreach(var item in listBox.Items)
			{
				var selectableItem = item as SelectableItem;
				if (selectableItem != null)
				{
					selectableItem.IsSelected = true;
				}
			}
		}

		private void SelectInvert_Click(object sender, RoutedEventArgs e)
		{
			foreach (var item in listBox.Items)
			{
				if (item is SelectableItem selectableItem)
				{
					selectableItem.IsSelected = !selectableItem.IsSelected;
				}
			}
		}

		private void SelectNone_Click(object sender, RoutedEventArgs e)
		{
			foreach (var item in listBox.Items)
			{
				if(item is SelectableItem selectableItem)
				{
					selectableItem.IsSelected = false;
				}
			}
		}

		private void SelectCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var listBoxItem = FindParent<ListBoxItem>(checkBox);
			if (listBoxItem != null)
			{
				listBoxItem.IsSelected = true;
			}
		}

		private void SelectCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;
			var listBoxItem = FindParent<ListBoxItem>(checkBox);
			if (listBoxItem != null)
			{
				listBoxItem.IsSelected = true;
			}
		}

		private T FindParent<T>(DependencyObject child) where T : DependencyObject
		{
			DependencyObject parentObject = VisualTreeHelper.GetParent(child);
			if (parentObject == null) return null;

			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}
			else
			{
				return FindParent<T>(parentObject);
			}
		}
		#endregion

	}
}