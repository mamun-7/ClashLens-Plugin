using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;

namespace ClashSolver.UI.TableAdapters
{
	public class DetectionSetTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static DetectionSetTableAdapter _instance;

		public static DetectionSetTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new DetectionSetTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public DetectionSetTableAdapter()
		{
			TableName = "DetectionSets";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"No INTEGER, " +
				"ProjectId INTEGER," +
				"Name TEXT NOT NULL, " +
				"SelectedCategoryA TEXT, " +
				"SelectedCategoryB TEXT, " +
				"BLinkInstanceId INTEGER, " +
				"GlobalTolerance real, " +
				"IsDynamicOnSize INTEGER, " +
				"IsSystemSpecific INTEGER, " +
				"HighSeverity real, " +
				"MediumSeverity real, " +
				"LowSeverity real);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(ClashDetectionSet set)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "ProjectId", set.ProjectId },
				{ "Name", set.Name },
				{ "SelectedCategoryA", set.GetSelectedCategoryA() },
				{ "SelectedCategoryB", set.GetSelectedCategoryB() },
				{ "BLinkInstanceId", set.BlinkInstanceId },
				{ "GlobalTolerance", set.GlobalTolerance },
				{ "IsDynamicOnSize", set.IsDynamicOnSize },
				{ "IsSystemSpecific", set.IsSystemSpecific },
				{ "HighSeverity", set.SeverityLevel.High },
				{ "MediumSeverity", set.SeverityLevel.Medium },
				{ "LowSeverity", set.SeverityLevel.Low },
			};

			return Insert(paraDict);
		}

		public long Update(ClashDetectionSet set) 
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", set.Id },
				{ "ProjectId", set.ProjectId },
				{ "Name", set.Name },
				{ "SelectedCategoryA", set.GetSelectedCategoryA() },
				{ "SelectedCategoryB", set.GetSelectedCategoryB() },
				{ "BLinkInstanceId", set.BlinkInstanceId },
				{ "GlobalTolerance", set.GlobalTolerance },
				{ "IsDynamicOnSize", set.IsDynamicOnSize },
				{ "IsSystemSpecific", set.IsSystemSpecific },
				{ "HighSeverity", set.SeverityLevel.High },
				{ "MediumSeverity", set.SeverityLevel.Medium },
				{ "LowSeverity", set.SeverityLevel.Low },
			};

			string condition = "Id=@Id";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			ClashDetectionSet set = null;

			if (reader != null)
			{
				ObservableCollection<CSCategory> aCategorySelectors = new ObservableCollection<CSCategory>();
				string aSelectedCategory = reader != null && reader["SelectedCategoryA"] != DBNull.Value ? Convert.ToString(reader["SelectedCategoryA"]) : "";
				List<string> aSelectedCategories = string.IsNullOrEmpty(aSelectedCategory) ? new List<string>() : [.. aSelectedCategory.Split(',')];

				ObservableCollection<CSCategory> bCategorySelectors = new ObservableCollection<CSCategory>();
				string bSelectedCategory = reader != null && reader["SelectedCategoryB"] != DBNull.Value ? Convert.ToString(reader["SelectedCategoryB"]) : "";
				List<string> bSelectedCategories = string.IsNullOrEmpty(bSelectedCategory) ? new List<string>() : [.. bSelectedCategory.Split(',')];

				var categories = CategoryTableAdapter.Instance.GetAll().Cast<CSCategory>().ToList();

				categories.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));

				foreach (var obj in categories)
				{
					if (obj is CSCategory category)
					{
						category.IsSelected = aSelectedCategories.Contains(category.Id.ToString());
						aCategorySelectors.Add(category);

						CSCategory bCategory = new CSCategory()
						{
							Id = category.Id,
							ElementId = category.ElementId,
							Name = category.Name,
							IsSelected = bSelectedCategories.Contains(category.Id.ToString())
						};

						bCategorySelectors.Add(bCategory);
					}
				}

				set = new ClashDetectionSet()
				{
					Id = Convert.ToInt32(reader["Id"]),
					ProjectId = Convert.ToInt32(reader["ProjectId"]),
					Name = Convert.ToString(reader["Name"]),
					AElementCategories = aCategorySelectors,
					BElementCategories = bCategorySelectors,
					BlinkInstanceId = Convert.ToInt32(reader["BLinkInstanceId"]),
					GlobalTolerance = Convert.ToDouble(reader["GlobalTolerance"]),
					IsDynamicOnSize = Convert.ToInt32(reader["IsDynamicOnSize"]) == 1,
					IsSystemSpecific = Convert.ToInt32(reader["IsSystemSpecific"]) == 1,
					SeverityLevel = new SeverityLevel()
					{
						High = Convert.ToDouble(reader["HighSeverity"]),
						Medium = Convert.ToDouble(reader["MediumSeverity"]),
						Low = Convert.ToDouble(reader["LowSeverity"]),
					}
				};

				set.IsIncludeLink = set.BlinkInstanceId > 0;
			}

			return set;
		}

		#endregion
	}
}
