using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class ProjectTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static ProjectTableAdapter _instance;

		public static ProjectTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ProjectTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public ProjectTableAdapter()
		{
			TableName = "Projects";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"Name TEXT NOT NULL," +
				"UniqueId TEXT NOT NULL," +
				"Path TEXT NOT NULL," +
				"Version TEXT NOT NULL," +
				"ValidationTime TEXT);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(Project project)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Name", project.Name },
				{ "UniqueId", project.UniqueId },
				{ "Path", project.Path },
				{ "Version", project.Version }
			};

			return Insert(paraDict);
		}

		public object GetByUniqueId(string uniqueId)
		{
			string condition = $"UniqueId='{uniqueId}'";

			var res = GetByCondition(condition);

			if(res != null && res.Count > 0)
			{
				return res[0];
			}

			return null;
		}

		public long Update(Project project) 
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", project.Id },
				{ "UniqueId", project.UniqueId },
				{ "Name", project.Name },
				{ "Path", project.Path }
			};

			string condition = "Id=@Id OR UniqueId=@UniqueId";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			Project project = null;
			if (reader != null)
			{
				project = new Project()
				{
					Id = Convert.ToInt32(reader["Id"]),
					Name = reader["Name"].ToString(),
					UniqueId = reader["UniqueId"].ToString(),
					Path = reader["Path"].ToString(),
					Version = reader["Version"].ToString(),
					ValidationTime = reader["ValidationTime"].ToString()
				};
			}

			return project;
		}

		#endregion
	}
}
