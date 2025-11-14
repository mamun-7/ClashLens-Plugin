using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class CategoryTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static CategoryTableAdapter _instance;

		public static CategoryTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new CategoryTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public CategoryTableAdapter()
		{
			TableName = "Categories";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"ElementId INTEGER NOT NULL," +
				"Name TEXT NOT NULL," +
				"Version TEXT NOT NULL);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(CSCategory category)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "ElementId", category.ElementId },
				{ "Name", category.Name },
				{ "Version", category.Version }
			};

			return Insert(paraDict);
		}

		public long Update(CSCategory category) 
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", category.Id },
				{ "Name", category.Name },
			};

			string condition = "Id=@Id";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			CSCategory category = null;
			if (reader != null)
			{
				category = new CSCategory()
				{
					Id = Convert.ToInt32(reader["Id"]),
					ElementId = Convert.ToInt32(reader["ElementId"]),
					Name = reader["Name"].ToString(),
					Version = reader["Version"].ToString(),
				};
			}

			return category;
		}

		#endregion
	}
}
