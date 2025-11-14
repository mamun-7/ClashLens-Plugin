using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;

namespace ClashSolver.UI
{
	public class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;

			return value.ToString().Equals(parameter.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool isChecked && isChecked && parameter != null)
			{
				return Enum.Parse(targetType, parameter.ToString());
			}
			return DependencyProperty.UnsetValue;
		}
	}

	public class EnumDescriptionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());
			var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DisplayAttribute));
			return attribute == null ? value.ToString() : attribute.Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || targetType == null || !targetType.IsEnum)
				return null;

			foreach (var field in targetType.GetFields())
			{
				var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
				if (attribute != null && attribute.Name == value.ToString())
				{
					return Enum.Parse(targetType, field.Name);
				}
			}

			return Enum.Parse(targetType, value.ToString());
		}
	}

}
