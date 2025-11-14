using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClashSolver.UI.Models;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.IO;
using System.Windows.Media;
using System.Web.ModelBinding;

namespace ClashSolver.UI.Utils
{
	public class Util
	{
		public static string GetDBPath()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string dbPath = Path.Combine(appDataPath, "IntelliBIM", "IntelliBIM.db");
			// Ensure the directory exists
			Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

			return dbPath;
		}

		public static int ToArgb(Color color)
		{
			return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
		}

		public static Color FromArgb(int argb)
		{
			return Color.FromArgb(
				(byte)((argb >> 24) & 0xFF),
				(byte)((argb >> 16) & 0xFF),
				(byte)((argb >> 8) & 0xFF),
				(byte)(argb & 0xFF));
		}
	}

	public static class ModelSpecificUtil
	{
		public static Dictionary<string, object> GetDBFields(BaseModel element)
		{
			var res = element.GetType()
				.GetProperties()
				.Where(prop => Attribute.IsDefined(prop, typeof(CategoryAttribute)) &&
							((CategoryAttribute)Attribute.GetCustomAttribute(prop, typeof(CategoryAttribute))).Category == "Database")
				.ToDictionary(
					prop => prop.Name,
					prop => prop.GetValue(element)
				);

			return res;
		}
	}
}
