using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
using ColumnVisibility = System.Windows.Visibility;

namespace ClashSolver.UI.UIComponents
{
	/// <summary>
	/// Interaction logic for WindowColumnVisibility.xaml
	/// </summary>
	public partial class WindowColumnVisibility : Window
	{
		public List<ColumnInfo> Columns { get; set; }

		public WindowColumnVisibility(IEnumerable<DataGridColumn> columns)
		{
			InitializeComponent();

			Columns = columns.Select(c => new ColumnInfo()
			{
				Header = c.Header?.ToString(),
				IsVisible = c.Visibility == ColumnVisibility.Visible,
				Column = c
			}).ToList();
			ColumnsList.ItemsSource = Columns;
		}

		private void OnOK_Click(object sender, RoutedEventArgs e)
		{
			foreach(var col in Columns)
			{
				col.Column.Visibility = col.IsVisible ? ColumnVisibility.Visible : ColumnVisibility.Collapsed;
			}
			DialogResult = true;
			Close();
		}
	}

	public class ColumnInfo
	{
		public string Header { get; set; }
		public bool IsVisible { get; set; }
		public DataGridColumn Column { get; set; }
	}
}
