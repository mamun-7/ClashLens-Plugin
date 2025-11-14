using ForgeAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClashSolver.UI.TableAdapters
{
	public class ACCUserTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static ACCUserTableAdapter _instance;

		public static ACCUserTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ACCUserTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public ACCUserTableAdapter()
		{
			TableName = "ACCUsers";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"ClientId TEXT NOT NULL," +
				"ClientSecret TEXT NOT NULL," +
				"AccessToken TEXT NOT NULL," +
				"ExpiresAt TEXT NOT NULL," +
				"IsLogin INTEGER NOT NULL)";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(Auth auth)
		{
			var exist = GetByClientId(auth.ClientId);

			if (exist != null)
			{
				return Update(auth);
			}

			var paraDict = new Dictionary<string, object>()
				{
					{ "ClientId", auth.ClientId },
					{ "ClientSecret", auth.ClientSecret },
					{ "AccessToken", auth.AccessToken },
					{ "ExpiresAt", auth.ExpiresAt.ToString() },
					{ "IsLogin", auth.IsLogin ? 1 : 0 }
				};

			return Insert(paraDict);
		}

		public Auth GetByClientId(string clientId)
		{
			string condition = $"ClientId = '{clientId}'";
			var res = GetByCondition(condition);

			if (res != null && res.Count > 0)
			{
				return res[0] as Auth;
			}

			return null;
		}

		public Auth GetLoginUser()
		{
			string condition = $"IsLogin = 1;";
			var res = GetByCondition(condition);

			if(res != null && res.Count > 0)
			{
				return res[0] as Auth;
			}

			return null;
		}

		public long Update(Auth auth)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", auth.Id },
				{ "AccessToken", auth.AccessToken },
				{ "ExpiresAt", auth.ExpiresAt.ToString() },
				{ "IsLogin", auth.IsLogin ? 1 : 0 }
			};
			string contidition = $"Id=@Id";

			return Update(paraDict, contidition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			Auth auth = null;
			if (reader != null)
			{
				auth = new Auth()
				{
					Id = Convert.ToInt32(reader["Id"]),
					ClientId = reader["ClientId"].ToString(),
					ClientSecret = reader["ClientSecret"].ToString(),
					AccessToken = reader["AccessToken"].ToString(),
					ExpiresAt = DateTime.Parse(reader["ExpiresAt"].ToString()),
					IsLogin = reader["IsLogin"].ToString() == "1"
				};
			}
			return auth;
		}

		#endregion
	}
}
