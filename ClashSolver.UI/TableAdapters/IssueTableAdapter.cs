using Architexor.Core;
using ClashSolver.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.Json;
using System.Windows.Media.Animation;

namespace ClashSolver.UI.TableAdapters
{
	public class IssueTableAdapter : BaseTableAdapter
	{
		#region Class Instance

		private static IssueTableAdapter _instance;

		public static IssueTableAdapter Instance
		{
			get
			{
				if (_instance == null)
					_instance = new IssueTableAdapter();
				return _instance;
			}
		}

		#endregion

		#region Constructors

		public IssueTableAdapter()
		{
			TableName = "Issues";

			string createTableQuery = $"CREATE TABLE IF NOT EXISTS {TableName} " +
				"(Id INTEGER PRIMARY KEY AUTOINCREMENT," +
				"ProjectId TEXT NOT NULL, " +
				"DetectionSetId TEXT NOT NULL," +
				"ElementIdA INTEGER NOT NULL, " +
				"ElementA TEXT NOT NULL, " +
				"CategoryA TEXT NOT NULL, " +
				"ElementIdB INTEGER NOT NULL, " +
				"ElementB TEXT NOT NULL, " +
				"LinkModelA INTEGER NOT NULL, " +
				"CategoryB TEXT NOT NULL, " +
				"LinkModelB INTEGER NOT NULL, " +
				"TagId INTEGER," +
				"ScopeBox INTEGER," +
				"Intersection TEXT, " +
				"Severity TEXT NOT NULL, " +
				"Status INTEGER NOT NULL, " +
				"AssignedBy TEXT, " +
				"ResolvedBy TEXT, " +
				"Description TEXT, " +
				"Visibility INTEGER, " +
				"AnalyzedAt TEXT NOT NULL, " +
				"AISolved INTEGER);";

			CreateTable(createTableQuery);
		}

		#endregion

		#region Methods

		public long Insert(Issue issue)
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "ProjectId", issue.ProjectId },
				{ "DetectionSetId", issue.ClashDetectionSet.Id },
				{ "ElementIdA", issue.ElementIdA },
				{ "ElementA", issue.ElementA },
				{ "CategoryA", issue.CategoryA.Id },
				{ "ElementIdB", issue.ElementIdB },
				{ "ElementB", issue.ElementB },
				{ "LinkModelA", issue.LinkModelA.Id },
				{ "CategoryB", issue.CategoryB.Id },
				{ "LinkModelB", issue.LinkModelB.Id },
				{ "TagId", issue.TagId },
				{ "ScopeBox", issue.ScopeBox },
				{ "Intersection", issue.Intersection.ToString() },
				{ "Severity", issue.Severity },
				{ "Status", (int)issue.Status },
				{ "AssignedBy", issue.AssignedBy },
				{ "ResolvedBy", issue.ResolvedBy },
				{ "Visibility", (int)issue.Visibility },
				{ "Description", issue.Description },
				{ "AnalyzedAt", issue.AnalyzedAt },
				{ "AISolved", issue.IsAISolved? 1 : 0 },
			};

			return Insert(paraDict);
		}

		public List<object> GetByStatus(long projectId, string statuses)
		{
			string condition = $"ProjectId = {projectId} AND Status IN ({statuses})";

			return GetByCondition(condition);
		}

		public long GetTotalCount(FilterCriteria criteria)
		{
			string filterQuery = "";

			if (criteria.IsFilterBySet)
			{
				if (criteria.SelectedDetectionSets.Any())
				{
					string detectionSetsStr = string.Join(",", criteria.SelectedDetectionSets);
					filterQuery += $"AND DetectionSetId IN ({detectionSetsStr}) ";
				}

				if (criteria.CategoryAIds.Any())
				{
					string categoryAStr = string.Join(",", criteria.CategoryAIds);
					filterQuery += $"AND CategoryA IN ({categoryAStr}) ";
				}

				if (criteria.CategoryBIds.Any())
				{
					string categoryBStr = string.Join(",", criteria.CategoryBIds);
					filterQuery += $"AND CategoryB IN ({categoryBStr}) ";
				}

				if (criteria.Statuses.Any())
				{
					string statusStr = string.Join(",", criteria.Statuses);
					filterQuery += $"AND Status IN ({statusStr}) ";
				}

				if (criteria.Severities.Any())
				{
					string severityStr = string.Join(",", criteria.Severities);
					filterQuery += $"AND Severity IN ({severityStr}) ";
				}
			}

			Query = $"SELECT COUNT(*) FROM {TableName} WHERE ProjectId = @ProjectId {filterQuery};";


			long res = -1;

			using (SQLiteCommand command = new SQLiteCommand(Query, Connection))
			{
				command.Parameters.AddWithValue("@ProjectId", criteria.ProjectId);
				try
				{
					// Execute the query and retrieve the result
					res = (long)command.ExecuteScalar();
				}
				catch (Exception ex)
				{
					TraceLogger.Instance.ExceptionLog("IssueTableAdapter::GetTotalCount => ", ex);
				}
			}

			return res;
		}

		public List<object> GetByFilter(FilterCriteria criteria)
		{
			int countPerPage = Constants.ISSUE_COUNTER_PER_PAGE;
			string filterQuery = "";

			if (criteria.IsFilterBySet)
			{
				if (criteria.SelectedDetectionSets.Any())
				{
					string detectionSetsStr = string.Join(",", criteria.SelectedDetectionSets);
					filterQuery += $"AND DetectionSetId IN ({detectionSetsStr}) ";
				}
			}

			if (criteria.IsFilterByScope)
			{
				filterQuery += $"AND ScopeBox = {criteria.ScopeBoxId} ";
			}

			if(criteria.IsFilterByHeader)
			{ 
				if (criteria.CategoryAIds.Any())
				{
					string categoryAStr = string.Join(",", criteria.CategoryAIds);
					filterQuery += $"AND CategoryA IN ({categoryAStr}) ";
				}

				if (criteria.CategoryBIds.Any())
				{
					string categoryBStr = string.Join(",", criteria.CategoryBIds);
					filterQuery += $"AND CategoryB IN ({categoryBStr}) ";
				}

				if (criteria.Statuses.Any())
				{
					string statusStr = string.Join(",", criteria.Statuses);
					filterQuery += $"AND Status IN ({statusStr}) ";
				}

				if (criteria.Severities.Any())
				{
					string severityStr = "";

					foreach (var severity in criteria.Severities)
					{
						severityStr += $"'{severity}',";
					}

					filterQuery += $"AND Severity IN ({severityStr}) ";
				}
			}

			string condition = $"ProjectId = {criteria.ProjectId} {filterQuery} LIMIT {countPerPage} OFFSET {(criteria.PageNumber - 1) * countPerPage}";

			return GetByCondition(condition);
		}

		public Issue GetByElementId(long elementId1, long elementId2)
		{
			string condition = $"ElementIdA = {elementId1} AND ElementIdB = {elementId2}";

			List<object> res = GetByCondition(condition);

			if(res != null && res.Count > 0)
			{
				return res[0] as Issue;
			}
			
			return null;
		}

		public Issue GetByTagId(long tagId)
		{
			string condition = $"TagId = {tagId}";

			List<object> res = GetByCondition(condition);

			if(res != null && res.Count > 0)
			{
				return res[0] as Issue;
			}

			return null;
		}

		public List<long> GetCategories(string columnName)
		{
			List<long> res = new List<long>();

			string selectQuery = $"SELECT DISTINCT {columnName} FROM {TableName}";
			
			using (SQLiteCommand command = new SQLiteCommand(selectQuery, Connection))
			{
				using (SQLiteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						res.Add(Convert.ToInt32(reader[columnName]));
					}
				}
			}

			return res;
		}

		public List<string> GetFilterNames(string columnName)
		{
			List<string> res = new List<string> ();

			string selectQuery = $"SELECT DISTINCT {columnName} FROM {TableName}";

			using (SQLiteCommand command = new SQLiteCommand(selectQuery, Connection))
			{
				using (SQLiteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						res.Add(reader[columnName].ToString());
					}
				}
			}

			return res;
		}

		public long Update(Issue issue) 
		{
			var paraDict = new Dictionary<string, object>()
			{
				{ "Id", issue.Id },
				{ "Status", (int)issue.Status },
				{ "TagId", issue.TagId },
				{ "AssignedBy", issue.AssignedBy },
				{ "ResolvedBy", issue.ResolvedBy },
				{ "AISolved", issue.IsAISolved? 1 : 0 }
			};

			string condition = "Id=@Id";

			return Update(paraDict, condition);
		}

		protected override object TryParse(SQLiteDataReader reader)
		{
			try
			{
				var aCategoryId = Convert.ToInt32(reader["CategoryA"]);
				var bCategoryId = Convert.ToInt32(reader["CategoryB"]);

				var linkModelAId = Convert.ToInt32(reader["LinkModelA"]);
				var linkModelBId = Convert.ToInt32(reader["LinkModelB"]);

				return new Issue()
				{
					Id = Convert.ToInt32(reader["Id"]),
					ProjectId = Convert.ToInt32(reader["ProjectId"]),
					ElementA = reader["ElementA"].ToString(),
					ElementB = reader["ElementB"].ToString(),
					LinkModelA = LinkModelTableAdapter.Instance.GetById(linkModelAId) as LinkedModel,
					ElementIdA = Convert.ToInt32(reader["ElementIdA"]),
					ElementIdB = Convert.ToInt32(reader["ElementIdB"]),
					LinkModelB = LinkModelTableAdapter.Instance.GetById(linkModelBId) as LinkedModel,
					CategoryA = CategoryTableAdapter.Instance.GetById(aCategoryId) as CSCategory,
					CategoryB = CategoryTableAdapter.Instance.GetById(bCategoryId) as CSCategory,
					TagId = Convert.ToInt64(reader["TagId"]),
					ScopeBox = Convert.ToInt64(reader["ScopeBox"]),
					Intersection = Intersection.CreateInstance(reader["Intersection"].ToString()),
					Severity = reader["Severity"].ToString(),
					Status = (IssueStatus)Convert.ToInt32(reader["Status"]),
					AssignedBy = (LinkDiscipline)Convert.ToInt32(reader["AssignedBy"]),
					ResolvedBy = (LinkDiscipline)Convert.ToInt32(reader["ResolvedBy"]),
					Visibility = (Visibility)Convert.ToInt32(reader["Visibility"]),
					Description = reader["Description"].ToString(),
					AnalyzedAt = reader["AnalyzedAt"].ToString(),
					IsAISolved = Convert.ToInt32(reader["AISolved"]) == 1,
				};
			}
			catch (Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("IssueTableAdapter::TryParseIssue => ", ex);
				return null;
			}
		}

		#endregion
	}
}
