using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ClashSolver.UI.Models;
using ClashSolver.UI.Utils;

namespace ClashSolver.UI.TableAdapters
{
	public class SettingTableAdpater : BaseTableAdapter
	{
		#region Class Instance

		private static SettingTableAdpater _instance;

		public static SettingTableAdpater Instance
		{
			get
			{
				if (_instance == null)
					_instance = new SettingTableAdpater();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public SettingTableAdpater()
		{
			TableName = "Settings";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"IsShowClashMarker INTEGER NOT NULL," +
				"TextColor TEXT NOT NULL," +
				"BoxColor TEXT NOT NULL," +
				"MarkerSize INTEGER NOT NULL," +
				"BoxSize INTEGER NOT NULL," +
				"MarkerType INTEGER NOT NULL," +
				"IsDisplayClashId INTEGER NOT NULL," +
				"IsDisplayClashType INTEGER NOT NULL);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(MarkerSetting setting)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "IsShowClashMarker", setting.IsShowClashMarker? 1 : 0 },
				{ "TextColor", $"{Util.ToArgb(setting.TextHighColor)},{Util.ToArgb(setting.TextMediumColor)},{Util.ToArgb(setting.TextLowColor)}" },
				{ "BoxColor", $"{Util.ToArgb(setting.BoxHighColor)},{Util.ToArgb(setting.BoxMediumColor)},{Util.ToArgb(setting.BoxLowColor)}"},
				{ "MarkerSize", setting.MarkerSize },
				{ "BoxSize", setting.BoxSize },
				{ "MarkerType", (int)setting.MarkerType },
				{ "IsDisplayClashId", setting.IsDisplayClashId? 1 : 0 },
				{ "IsDisplayClashType", setting.IsDisplayClashType? 1 : 0 }
			};

			return Insert(paraDict);
		}

		public long Update(MarkerSetting setting)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "IsShowClashMarker", setting.IsShowClashMarker? 1 : 0 },
				{ "TextColor", $"{Util.ToArgb(setting.TextHighColor)},{Util.ToArgb(setting.TextMediumColor)},{Util.ToArgb(setting.TextLowColor)}" },
				{ "BoxColor", $"{Util.ToArgb(setting.BoxHighColor)},{Util.ToArgb(setting.BoxMediumColor)},{Util.ToArgb(setting.BoxLowColor)}"},
				{ "MarkerSize", setting.MarkerSize },
				{ "BoxSize", setting.BoxSize },
				{ "MarkerType", (int)setting.MarkerType },
				{ "IsDisplayClashId", setting.IsDisplayClashId? 1 : 0 },
				{ "IsDisplayClashType", setting.IsDisplayClashType? 1 : 0 }
			};

			string condition = $"Id = {setting.Id}";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			MarkerSetting setting = null;
			if (reader != null)
			{
				setting = new MarkerSetting()
				{
					Id = Convert.ToInt32(reader["Id"]),
					IsShowClashMarker = Convert.ToInt16(reader["IsShowClashMarker"]) == 1,
					MarkerSize = Convert.ToInt32(reader["MarkerSize"]),
					BoxSize = Convert.ToInt32(reader["BoxSize"]),
					MarkerType = (MarkerType)Convert.ToInt32(reader["MarkerType"]),
					IsDisplayClashId = Convert.ToInt32(reader["IsDisplayClashId"]) == 1,
					IsDisplayClashType = Convert.ToInt32(reader["IsDisplayClashType"]) == 1
				};

				string textColor = reader["TextColor"].ToString();
				if (!string.IsNullOrEmpty(textColor))
				{
					string[] textColors = textColor.Split(',');
					if (textColors.Length == 3)
					{
						setting.TextHighColor = Util.FromArgb(Convert.ToInt32(textColors[0]));
						setting.TextMediumColor = Util.FromArgb(Convert.ToInt32(textColors[1]));
						setting.TextLowColor = Util.FromArgb(Convert.ToInt32(textColors[2]));
					}
				}

				string boxColor = reader["BoxColor"].ToString();
				if (!string.IsNullOrEmpty(boxColor))
				{
					string[] boxColors = boxColor.Split(',');
					if (boxColors.Length == 3)
					{
						setting.BoxHighColor = Util.FromArgb(Convert.ToInt32(boxColors[0]));
						setting.BoxMediumColor = Util.FromArgb(Convert.ToInt32(boxColors[1]));
						setting.BoxLowColor = Util.FromArgb(Convert.ToInt32(boxColors[2]));
					}
				}
			}

			return setting;
		}

		#endregion
	}
}
