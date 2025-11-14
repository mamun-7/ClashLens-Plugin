using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class ReportContentTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static ReportContentTableAdapter _instance;

		public static ReportContentTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ReportContentTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public ReportContentTableAdapter()
		{
			TableName = "ReportContents";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"Name TEXT NOT NULL," +
				"Type TEXT)";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(ReportContent content)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Name", content.Name }
			};
			return Insert(paraDict);
		}

		public List<object> GetByType(string type = "General")
		{
			string condition = $"Type = '{type}'";

			return GetByCondition(condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			ReportContent content = null;

			if (reader != null)
			{
				content = new ReportContent()
				{
					Id = Convert.ToInt64(reader["Id"]),
					Name = reader["Name"].ToString(),
					Type = reader["Type"].ToString()
				};
			}

			return content;
		}

		#endregion
	}
}
