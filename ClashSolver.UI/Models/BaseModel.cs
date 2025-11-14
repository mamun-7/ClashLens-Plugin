using System.ComponentModel;
using System.Reflection;

namespace ClashSolver.UI.Models
{
	public class BaseModel : INotifyPropertyChanged
	{
		#region Fields

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region INotifyPropertyChanged implementation

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public object GetPropertyValue(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				return null;

			PropertyInfo property = GetType().GetProperty(propertyName);
			return property?.GetValue(this, null);
		}

		#endregion
	}
}
