using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Architexor.Core;
using ClashSolver.UI.Models;
using ClosedXML.Excel;

namespace ClashSolver.Utils
{
	public class ExcelAdapter
	{
		public static void ExportToExcel(List<string> headers, List<Issue> issues, string filePath)
		{
			try
			{
				using (var workbook = new XLWorkbook())
				{
					var worksheet = workbook.Worksheets.Add("Clash Report");

					// Headers
					for (int col = 0; col < headers.Count; col++)
					{
						worksheet.Cell(1, col + 1).Value = headers[col];
					}

					for (int row = 0; row < issues.Count; row++)
					{
						var issue = issues[row];

						for (int col = 0; col < headers.Count; col++)
						{
							worksheet.Cell(row + 2, col + 1).Value = issue.GetPropertyValue(headers[col]).ToString();
						}
					}

					// Save
					workbook.SaveAs(filePath);
				}
			}
			catch(Exception ex)
			{
				TraceLogger.Instance.ExceptionLog($"ExcelExporter::ExportToExcel", ex);
			}
		}

		public static List<Issue> ImportFromExcel(string filePath)
		{
			List<Issue> issues = new List<Issue>();

			try
			{
				using (var workbook = new XLWorkbook(filePath))
				{
					var worksheet = workbook.Worksheets.First();
					var rows = worksheet.RowsUsed().Skip(1); // Skip header row

					// Get column headers
					var headers = worksheet.Row(1).Cells().Select(x => x.Value.ToString()).ToList();

					foreach (var row in rows)
					{
						Issue issue = new Issue();

						for (int col = 0; col < headers.Count; col++)
						{
							string header = headers[col];
							var cellValue = row.Cell(col + 1).Value;

							// Use reflection to set the property value
							PropertyInfo property = typeof(Issue).GetProperty(header);
							if (property != null && !cellValue.IsBlank)
							{
								object value = Convert.ChangeType(cellValue, property.PropertyType);
								property.SetValue(issue, value);
							}
						}

						issues.Add(issue);
					}
				}
			}
			catch(Exception ex)
			{
				TraceLogger.Instance.ExceptionLog($"ExcelExporter::ImportFromExcel", ex);
				return null;
			}

			return issues;
		}
	}
}
