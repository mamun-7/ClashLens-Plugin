using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class SystemToleranceTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static SystemToleranceTableAdapter _instance;

		public static SystemToleranceTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new SystemToleranceTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public SystemToleranceTableAdapter()
		{
			TableName = "SystemTolerance";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"System TEXT NOT NULL," +
				"Name TEXT NOT NULL," +
				"Description TEXT NOT NULL," +
				"High INTEGER NOT NULL," +
				"Medium INTEGER NOT NULL," +
				"Low INTEGER NOT NULL);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(SystemSpecificTolerance specific)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "System", specific.System },
				{ "Name", specific.Name },
				{ "Description", specific.Description },
				{ "High", specific.High },
				{ "Medium", specific.Medium },
				{ "Low", specific.Low },
			};

			return Insert(paraDict);
		}


		protected override object TryParse(SQLiteDataReader reader)
		{
			SystemSpecificTolerance systemTolerances = null;

			if (reader != null)
			{
				systemTolerances = new SystemSpecificTolerance()
				{
					Id = Convert.ToInt32(reader["Id"]),
					System = Convert.ToString(reader["System"]),
					Name = reader["Name"].ToString(),
					Description = reader["Description"].ToString(),
					High = Convert.ToInt32(reader["High"]),
					Medium = Convert.ToInt32(reader["Medium"]),
					Low = Convert.ToInt32(reader["Low"]),
				};
			}

			return systemTolerances;
		}

		#endregion
	}
}
