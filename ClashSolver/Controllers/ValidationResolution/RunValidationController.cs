using System.Collections.Generic;
using Autodesk.Revit.DB;
using Architexor.Core;
using ClashSolver.Utils;
using ClashSolver.UI.TableAdapters;
using ClashSolver.Request;
using ClashSolver.UI.Models;
using System.Linq;
using System.Reflection;
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using ClashSolver.UI;

namespace ClashSolver.Controllers
{
	public class RunValidationController : Controller
	{
		#region Fields

		private List<LinkedModel> _models = new List<LinkedModel>();
		private List<Issue> _issues = new List<Issue>();
		private List<ClashDetectionSet> _detectionSets = new List<ClashDetectionSet>();

		#endregion

		#region Properties

		public List<LinkedModel> Models
		{
			get => _models;
			set => _models = value;
		}

		public List<Issue> Issues
		{
			get => _issues;
			set => _issues = value;
		}

		public List<ClashDetectionSet> DetectionSets
		{
			get => _detectionSets;
			set => _detectionSets = value;
		}

		public static Family Marker = null;

		#endregion

		#region Initialization

		public override bool Initialize()
		{
			//	Read Linked Models from Revit document
			Document doc = GetDocument();

			_models = RevitHelper.GetLinkedProjectsFromDB(Application.thisApp.Project.Id);

			return true;
		}

		#endregion

		#region Request Handlers

		public override bool ProcessRequest(ClashSolverRequestId reqId)
		{
			bool bFinish = false;
			Document doc = GetDocument();

			switch(reqId)
			{
				case ClashSolverRequestId.None:
					return bFinish;
				case ClashSolverRequestId.RunValidation:
					break;
				case ClashSolverRequestId.ReviewIssues:
					break;
				case ClashSolverRequestId.UpdateIssues:
					if(Issues.Count == 0)
					{
						TaskDialog.Show("Notice", "There are no conflicting elements.");
					}
					else
					{
						CreateClashTags();
						Application.thisApp.DoRequest(m_uiApp, reqId);
					}
					break;
				default:
					break;
			}

			return bFinish;
		}

		#endregion

		#region Methods

		public List<long> GetElementsFromCategory(long categoryId)
		{
			Document doc = GetDocument();

			return RevitHelper.GetElementsByCategoryId(doc, categoryId);
		}

		private void CreateClashTags()
		{
			Document doc = GetDocument();

			if (!CheckFamily(doc, Constants.MARKER_FAMILY_NAME))
				return;

			try
			{
				using (Transaction trans = new Transaction(doc, "Delete existing tags"))
				{
					trans.Start();

					List<ElementId> existingIds = [];

					FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
									.WhereElementIsNotElementType();

					// Get all the elements inside the scope box
					List<ElementId> scopeBoxes = Issues.Where(x => x.ScopeBox > 0).Distinct(new ScopeCompare()).Select(x => new ElementId(x.ScopeBox)).ToList();

					if(scopeBoxes.Any())
					{
						foreach(var id in scopeBoxes)
						{
							var scopeBox = doc.GetElement(new ElementId(Issues[0].ScopeBox));

							if(scopeBox == null)
								continue;

							BoundingBoxXYZ bbox = scopeBox.get_BoundingBox(null);

							if (bbox == null)
								return;

							// Use BoundingBoxIsInsideFilter to efficiently filter elements inside the scope box
							var outline = new Outline(bbox.Min, bbox.Max);
							BoundingBoxIsInsideFilter filter = new BoundingBoxIsInsideFilter(outline);
							collector.WherePasses(filter);

							foreach (Element element in collector)
							{
								if (element is FamilyInstance instance)
								{
									if (instance.Symbol.FamilyName == Constants.MARKER_FAMILY_NAME)
									{
										existingIds.Add(instance.Id);
									}
								}
							}
						}
					}
					else
					{
						foreach (Element element in collector)
						{
							if (element is FamilyInstance instance)
							{
								if (instance.Symbol.FamilyName == Constants.MARKER_FAMILY_NAME)
								{
									existingIds.Add(instance.Id);
								}
							}
						}
					}

					doc.Delete(existingIds);

					trans.Commit();
				}


				using (Transaction trans = new Transaction(doc, "Create Clash Tags"))
				{
					trans.Start();

					var familySymbol = GetFamilySymbol(doc, Marker);

					if (!familySymbol.IsActive)
					{
						familySymbol.Activate();
						doc.Regenerate();
					}

					List<Vector3D> locations = new List<Vector3D>();

					foreach (var issue in Issues)
					{
						var center = issue.Intersection.Center;

						if (locations.Contains(center))
						{
							continue;
						}

						locations.Add(center);

						XYZ location = new XYZ(center.X, center.Y, center.Z);

						FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, StructuralType.NonStructural);

						if (instance == null)
							continue;

						// Rotate 45 along z axis
						XYZ origin = location;
						XYZ axisStart = origin;
						XYZ axisEnd = origin + XYZ.BasisZ; // Rotation around Z-axis (vertical)

						Line rotationAxis = Line.CreateBound(axisStart, axisEnd);
						double angleRadians = -Math.PI / 4; // 45 degrees

						ElementTransformUtils.RotateElement(doc, instance.Id, rotationAxis, angleRadians);

						issue.TagId = instance.Id.Value;

						long id = IssueTableAdapter.Instance.Insert(issue);

						// Update instance parameters of clash marker
						double boxHeight = issue.Intersection.Max.Z - issue.Intersection.Min.Z;
						double boxWidth = issue.Intersection.Max.Y - issue.Intersection.Min.Y;

						Parameter param = instance.LookupParameter("Clash ID");
						param?.Set($"CL {id}");

						param = instance.LookupParameter("Clash Type");
						param?.Set("Hard");

						param = instance.LookupParameter("Severity");
						param?.Set(issue.Severity);

						if(!IsBarStyleBox(issue.Intersection.Min, issue.Intersection.Max))
						{
							param = instance.LookupParameter("Top Offset");
							double topOffset = Util.IUToMm(boxHeight) / 2;
							param?.Set(Util.MmToIU(topOffset + 150));

							param = instance.LookupParameter("Side Offset");
							double sideOffset = Util.IUToMm(boxWidth) / 2;
							param?.Set(Util.MmToIU(sideOffset + 150));

						}
					}

					trans.Commit();

					RevitHelper.UpdateMarkers(doc);
				}
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", $"An error occurred while creating the tag : {ex}");
			}
		}

		/// <summary>
		/// Check if the bounding box is bar type or solid.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="thresholdFactor"></param>
		/// <returns></returns>
		private bool IsBarStyleBox(Vector3D min, Vector3D max, double thresholdFactor = 5.0)
		{
			double x = Math.Abs(max.X - min.X);
			double y = Math.Abs(max.Y - min.Y);
			double z = Math.Abs(max.Z - min.Z);

			return	x > thresholdFactor * y && x > thresholdFactor * z ||
					y > thresholdFactor * z && y > thresholdFactor * z ||
					z > thresholdFactor * y && z > thresholdFactor * x;
		}

		private bool CheckFamily(Document doc, string name)
		{
			List<Family> families1 = new List<Family>(
							new FilteredElementCollector(doc)
								.WhereElementIsNotElementType()
								.OfClass(typeof(Family))
								.Where(ins => ins.Name == name)
								.ToList()
								.Cast<Family>()
								);
			List<Family> families = families1;

			if (families.Count > 0)
			{
				Marker = families[0];
				return true;
			}

			Transaction trans = new Transaction(doc, "Load Family");
			trans.Start("Load 3D Marker Family");
			try
			{
				string url = Assembly.GetExecutingAssembly().Location;
				url = url.Substring(0, url.LastIndexOf("\\")) + "\\";

				bool bRet = Marker != null || doc.LoadFamily(url + name + ".rfa", out Marker);
				if (bRet)
				{
					trans.Commit();
					return true;
				}

				trans.RollBack();
				return false;
			}
			catch (Exception)
			{
				trans.RollBack();
				TaskDialog.Show("Error", "Can not find the family. Please contact the developer");
				return false;
			}
		}

		private FamilySymbol GetFamilySymbol(Document doc, Family family)
		{
			ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();

			foreach (ElementId eId in familySymbolIds)
			{
				FamilySymbol familySymbol = doc.GetElement(eId) as FamilySymbol;
				if (familySymbol != null && !familySymbol.IsActive)
					familySymbol.Activate();
				return familySymbol;
			}
			return null;
		}

		#endregion
	}
	class ScopeCompare : IEqualityComparer<Issue>
	{
		public bool Equals(Issue x, Issue y)
		{
			return x.ScopeBox == y.ScopeBox;
		}

		public int GetHashCode(Issue obj)
		{
			return obj.Id.GetHashCode() * obj.ScopeBox.GetHashCode();
		}
	}
}
