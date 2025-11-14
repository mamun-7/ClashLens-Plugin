using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class LinkModelTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static LinkModelTableAdapter _instance;

		public static LinkModelTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new LinkModelTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public LinkModelTableAdapter()
		{
			TableName = "LinkModels";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"Name TEXT NOT NULL," +
				"ProjectId INTEGER," +
				"ElementId INTEGER," +
				"Url TEXT," +
				"LinkInstanceId INTEGER," +
				"Discipline INTEGER," +
				"Description TEXT);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(LinkedModel model)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Name", model.Name },
				{ "ProjectId", model.ProjectId },
				{ "ElementId", model.ElementId },
				{ "LinkInstanceId", model.InstanceId },
				{ "Url", model.Url },
				{ "Discipline", (int)model.Discipline },
				{ "Description", model.Description }
			};

			return Insert(paraDict);
		}

		public object GetByElementId(long projectId, long elementId)
		{
			string condition = $"ProjectId={projectId} AND ElementId={elementId}";
			List<object> res = GetByCondition(condition);

			if (res != null && res.Count > 0)
			{
				return res[0];
			}
			return null;
		}

		public object GetByInstanceId(long projectId, long id)
		{
			string condition = $"ProjectId={projectId} AND LinkInstanceId = {id}";

			List<object> res = GetByCondition(condition);

			if(res != null && res.Count > 0)
			{
				return res[0];
			}

			return null;
		}

		public long Update(LinkedModel model)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", model.Id },
				{ "Discipline", (int)model.Discipline}
			};

			string condition = "Id=@Id";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			LinkedModel model = null;
			if (reader != null)
			{
				model = new LinkedModel()
				{
					Id = Convert.ToInt32(reader["Id"]),
					ProjectId = Convert.ToInt32(reader["ProjectId"]),
					Name = reader["Name"].ToString(),
					Url = reader["Url"].ToString(),
					ElementId = Convert.ToInt32(reader["ElementId"]),
					InstanceId = Convert.ToInt32(reader["LinkInstanceId"]),
					Discipline = (LinkDiscipline)Convert.ToInt32(reader["Discipline"]),
					Description = reader["Description"].ToString(),
				};
			}

			return model;
		}

		#endregion
	}
}
