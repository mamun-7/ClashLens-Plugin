using ClashSolver.Request;
using ClashSolver.UI.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using ClashSolver.Utils;
using ClashSolver.Forms.ValidationResolution;
using ClashSolver.UI.ValidationResolution.ReviewIssues;
using System.Collections.ObjectModel;

namespace ClashSolver.Controllers
{
	public class CostDatabaseController : Controller
	{

		public override bool Initialize()
		{
			return true;
		}

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.IssueReport:
					break;
				default:
					break;
			}

			return bFinish;
		}
	}
}
