using System.IO;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Linq;

namespace ClashSolver.Utils
{
	public class ResourceHelper
	{
		public static BitmapImage GetEmbeddedImage(string resourceName)
		{
			//	Get project name
			var assembly = Assembly.GetExecutingAssembly();
			resourceName = "ClashSolver.Resources." + resourceName;

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly.");

				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.StreamSource = stream;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.EndInit();
				return image;
			}
		}

		public static DataGridColumnHeader GetColumnHeader(DataGrid dataGrid, string columnName)
		{
			// Find the column by header name
			var column = dataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == columnName);
			if (column == null) return null;

			// Get the column headers presenter
			var columnHeadersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(dataGrid);
			if (columnHeadersPresenter == null) return null;

			// Loop through visual children to find the corresponding DataGridColumnHeader
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(columnHeadersPresenter); i++)
			{
				var child = VisualTreeHelper.GetChild(columnHeadersPresenter, i);
				if (child is DataGridColumnHeader header && header.Column == column)
				{
					return header;
				}
			}
			return null;
		}

		public static Point? GetColumnHeaderLocation(DataGrid dataGrid, string columnName)
		{
			// Get the column by header name
			var column = dataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == columnName);
			if (column == null) return null;

			// Find the column headers presenter
			var columnHeadersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(dataGrid);
			if (columnHeadersPresenter == null) return null;

			// Find the DataGridColumnHeader
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(columnHeadersPresenter); i++)
			{
				var child = VisualTreeHelper.GetChild(columnHeadersPresenter, i);
				if (child is DataGridColumnHeader header && header.Column == column)
				{
					// Transform the header location relative to the DataGrid
					return header.TransformToAncestor(dataGrid).Transform(new Point(0, 0));
				}
			}
			return null;
		}

		private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
		{
			if (parent == null) return null;

			int count = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T typedChild)
				{
					return typedChild;
				}
				var result = FindVisualChild<T>(child);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}
	}
}
