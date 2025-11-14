using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Architexor.Core;
using ClashSolver.UI.Utils;
using System.Windows;

namespace ClashSolver.UI
{
	public class BaseTableAdapter
	{
		#region Properties

		protected static SQLiteConnection Connection { get; set; }

		protected string TableName { get; set; }

		protected string Query { get; set; }

		protected Dictionary<string, object> CommandParameters { get; set; }

		#endregion

		#region Constructors

		public BaseTableAdapter()
		{
			CreateConnection();

			CommandParameters = new Dictionary<string, object>();
		}

		#endregion

		#region Methods

		private void CreateConnection()
		{
			// Get database path
			string dbPath = Util.GetDBPath();

			try
			{
				Connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;New=True;Compress=True;", dbPath));
				//	Open the connection:
				Connection.Open();
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog($"BaseTableAdapter::CreateConnection => ", ex);
			}
		}

		public static void CreateTable(string createTableQuery)
		{
			try
			{
				using (SQLiteCommand command = new SQLiteCommand(createTableQuery, Connection))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog(createTableQuery, ex);
				throw ex;
			}

		}

		protected long Insert(Dictionary<string, object> paraDict)
		{
			// Construct the column and parameter placeholders
			string columns = string.Join(", ", paraDict.Keys);
			string parameters = string.Join(", ", paraDict.Keys.Select(k => "@" + k));

			// Construct the query
			Query = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters});";
			CommandParameters = paraDict;

			return ExecuteQuery();
		}

		public List<object> GetAll()
		{
			return GetByCondition();
		}

		public List<object> GetByProjectId(long id)
		{
			string condition = $"ProjectId={id}";
			return GetByCondition(condition);
		}

		public object GetById(long id)
		{
			string condition = $"Id={id}";
			List<object> res = GetByCondition(condition);

			if (res != null && res.Count > 0)
			{
				return res[0];
			}

			return null;
		}
		public object GetByName(string name)
		{
			string condition = $"Name='{name}'";
			List<object> res = GetByCondition(condition);

			if (res != null && res.Count > 0)
			{
				return res[0];
			}

			return null;
		}

		public object GetByElementId(long elementId)
		{
			string condition = $"ElementId={elementId}";
			List<object> res = GetByCondition(condition);

			if (res != null && res.Count > 0)
			{
				return res[0];
			}
			return null;
		}

		protected List<object> GetByCondition(string condition = "")
		{
			List<object> res = new List<object>();

			try
			{
				string query = $"SELECT * FROM {TableName}";

				if (!string.IsNullOrEmpty(condition))
				{
					query += $" WHERE {condition}";
				}

				using (SQLiteCommand cmd = new SQLiteCommand(query, Connection))
				{
					using (SQLiteDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var item = TryParse(reader);

							if (item != null)
							{
								res.Add(item);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("BaseTableAdapter::GetByCondition => ", ex);
			}

			return res;
		}

		protected long Update(Dictionary<string, object> paraDict, string condition)
		{
			CommandParameters.Clear();

			// Construct the column and parameter placeholders
			string columns = string.Join(", ", paraDict.Keys.Select(k => $"{k} = @{k}"));
			// Construct the query
			Query = $"UPDATE {TableName} SET {columns} WHERE {condition};";
			foreach (var para in paraDict)
			{
				CommandParameters.Add(para.Key, para.Value);
			}

			return ExecuteQuery();
		}

		public long DeleteByProjectId(long projectId)
		{
			Query = $"DELETE FROM {TableName} WHERE ProjectId = @ProjectId;";

			CommandParameters = new Dictionary<string, object>()
			{
				{ "ProjectId", projectId }
			};

			return ExecuteQuery();
		}

		public virtual long Delete(long id)
		{
			long res = -1;

			if(GetById(id) != null)
			{
				Query = $"DELETE FROM {TableName} WHERE Id = @Id;";
				CommandParameters = new Dictionary<string, object>()
				{
					{ "Id", id }
				};

				res = ExecuteQuery();
			}
			else
			{
				MessageBox.Show("The specified item does not exist.");
			}

			return res;
		}

		public long ExecuteQuery()
		{
			long res = -1;
			try
			{
				using (SQLiteCommand command = new SQLiteCommand(Query, Connection))
				{
					foreach (var commandParam in CommandParameters)
					{
						command.Parameters.AddWithValue("@" + commandParam.Key, commandParam.Value);
					}

					string query = command.CommandText;

					foreach (SQLiteParameter parameter in command.Parameters)
					{
						string parameterPlaceholder = parameter.ParameterName;
						string parameterValue = parameter.Value != null ? parameter.Value.ToString() : "NULL";

						// If the parameter value is a string, wrap it in single quotes
						if (parameter.DbType == System.Data.DbType.String || parameter.DbType == System.Data.DbType.DateTime)
						{
							parameterValue = $"'{parameterValue}'";
						}

						query = query.Replace(parameterPlaceholder, parameterValue);
					}


					res = command.ExecuteNonQuery();

					if (res > 0)
					{
						res = Connection.LastInsertRowId;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"The error is occurred in database => {ex}");
				TraceLogger.Instance.ExceptionLog("BaseTableAdapter::QueryResult => ", ex);
			}

			return res;
		}

		protected virtual object TryParse(SQLiteDataReader reader)
		{
			return null;
		}

		#endregion

	}
}
