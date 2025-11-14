
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ClashSolver.UI.Models
{
	public class Issue : BaseModel
	{
		#region Fields

		private int _id = 0;
		private int _no = 0;
		private long _elementIdA = 0;
		private long _elementIdB = 0;
		private string _elementA = "";
		private string _elementB = "";
		private LinkedModel _linkModelA = new LinkedModel();
		private LinkedModel _linkModelB = new LinkedModel();
		private CSCategory _categoryA;
		private CSCategory _categoryB;
		private string _location = "";
		private string _serverity = "";
		private ClashDetectionSet _detectionSet = new ClashDetectionSet();
		private long _detectionSetId = 0;
		private IssueStatus _status = IssueStatus.Open;
		private Visibility _visibility = Visibility.Shown;
		private bool _isAISolved = false;
		private string _resolveStatus = "Resolve";
		private string _description = "";
		private string _analyzedAt = "";
		private string _resolvedAt = "";

		// Resolve
		private ResolveMethod _resolveMethod = ResolveMethod.None;
		private LinkDiscipline _assignedBy = LinkDiscipline.None;
		private LinkDiscipline _resolvedBy = LinkDiscipline.None;
		private string _resolveResult = "";

		#endregion

		#region Properties

		public int Id
		{
			get => _id;
			set
			{
				_id = value;
				OnPropertyChanged(nameof(Id));
				OnPropertyChanged(nameof(Name));
			}
		}

		public int No
		{
			get => _no;
			set
			{
				_no = value;
				OnPropertyChanged(nameof(No));
			}
		}

		public string Name
		{
			get
			{
				return $"{Id}";
			}
		}

		public long ProjectId { get; set; }

		public CSCategory CategoryA
		{
			get => _categoryA;
			set
			{
				_categoryA = value;
				OnPropertyChanged(nameof(CategoryA));
			}
		}
		public CSCategory CategoryB
		{
			get => _categoryB;
			set
			{
				_categoryB = value;
				OnPropertyChanged(nameof(CategoryB));
			}
		}

		public long ElementIdA
		{
			get => _elementIdA;
			set 
			{
				_elementIdA = value;
				OnPropertyChanged(nameof(ElementA));
			} 
		}

		public long ElementIdB
		{	get => _elementIdB;
			set
			{
				_elementIdB = value;
				OnPropertyChanged(nameof(ElementB));
			}
		}

		public string ElementA
		{
			get => _elementA;
			set
			{
				_elementA = value;
				OnPropertyChanged(nameof(ElementA));
			}
		}

		public string ElementB
		{
			get => _elementB;
			set
			{
				_elementB = value;
				OnPropertyChanged(nameof(ElementB));
			}
		}

		public LinkedModel LinkModelA
		{
			get => _linkModelA;
			set
			{
				_linkModelA = value;
				OnPropertyChanged(nameof(LinkModelA));
			}
		}

		public LinkedModel LinkModelB
		{
			get => _linkModelB;
			set
			{
				_linkModelB = value;
				OnPropertyChanged(nameof(LinkModelB));
			}
		}

		public string Severity
		{
			get => _serverity;
			set
			{
				_serverity = value;
				OnPropertyChanged(nameof(Severity));
			}
		}

		public ClashDetectionSet ClashDetectionSet
		{
			get => _detectionSet;
			set
			{
				_detectionSet = value;
				OnPropertyChanged(nameof(ClashDetectionSet));
			}
		}

		public long DetectionSetId
		{
			get => _detectionSetId;
			set
			{
				_detectionSetId = value;
			}
		}

		/// <summary>
		/// Status of issue 1:Open, 2:PendingApproval, 3:UnderReview, 4:Closed
		/// </summary>
		public IssueStatus Status
		{
			get => _status;
			set
			{
				_status = value;
				OnPropertyChanged(nameof(Status));
			}
		}

		public string ResolveStatus
		{
			get => _resolveStatus;
			set
			{
				_resolveStatus = value;
				OnPropertyChanged(nameof(ResolveStatus));
			}
		}

		public Visibility Visibility
		{
			get => _visibility;
			set
			{
				_visibility = value;
				OnPropertyChanged(nameof(Visibility));
			}
		}

		public bool IsAISolved
		{
			get => _isAISolved;
			set
			{
				_isAISolved = value;
				OnPropertyChanged(nameof(IsAISolved));
			}
		}

		public string Description
		{
			get => _description;
			set
			{
				_description = value;
				OnPropertyChanged(nameof(Description));
			}
		}

		public string AnalyzedAt
		{
			get => _analyzedAt;
			set
			{
				_analyzedAt = value;
				OnPropertyChanged(nameof(AnalyzedAt));
			}
		}

		public string ResolvedAt
		{
			get => _resolvedAt;
			set
			{
				_resolvedAt = value;
				OnPropertyChanged(nameof(ResolvedAt));
			}
		}

		public ResolveMethod ResolveMethod
		{
			get => _resolveMethod;
			set
			{
				_resolveMethod = value;
				OnPropertyChanged(nameof(ResolveMethod));
			}
		}

		public string ResolveResult
		{
			get => _resolveResult;
			set
			{
				_resolveResult = value;
				OnPropertyChanged(nameof(ResolveResult));
			}
		}

		public LinkDiscipline AssignedBy
		{
			get => _assignedBy;
			set
			{
				_assignedBy = value;
				OnPropertyChanged(nameof(AssignedBy));
			}
		}

		public LinkDiscipline ResolvedBy
		{
			get => _resolvedBy;
			set
			{
				_resolvedBy = value;
				OnPropertyChanged(nameof(ResolvedBy));
			}
		}

		public long TagId { get;set; }

		public long CopyElementIdA { get; set; } = 0;

		public long CopyElementIdB { get; set; } = 0;

		public long ScopeBox { get; set; } = -1;

		public Intersection Intersection { get; set; }
		#endregion
	}

	public class Intersection
	{
		/// <summary>	
		/// Min point of bounding box of solid
		/// </summary>
		public Vector3D Center { get; set; }

		/// <summary>	
		/// Min point of bounding box of solid
		/// </summary>
		public Vector3D Min { get;set; }

		/// <summary>
		/// Max point of bounding box of solid
		/// </summary>
		public Vector3D Max { get; set; }

		/// <summary>
		/// The direction of intersection solid
		/// </summary>
		public Vector3D Direction { get; set; }

		/// <summary>
		/// Width of intersection box along local x axis
		/// </summary>
		public double Width 
		{ 
			get
			{
				return Max.X - Min.X;
			} 
		}

		/// <summary>
		/// Depth of intersection box along local y axis
		/// </summary>
		public double Depth
		{
			get
			{
				return Max.Y - Min.Y;
			}
		}

		/// <summary>
		/// Height of intersection box along local z axis
		/// </summary>
		public double Height
		{
			get
			{
				return Max.Z - Min.Z;
			}
		}

		/// <summary>
		/// Create new Intersection using data from database
		/// </summary>
		/// <param name="data">data obtained from database</param>
		/// <returns></returns>
		public static Intersection CreateInstance(string data)
		{
			Intersection intersection = new Intersection();
			JObject jobj = JsonConvert.DeserializeObject<JObject>(data);

			if (jobj["Center"] != null)
			{
				var center = jobj["Center"].ToString().Split(',').Select(x => Convert.ToDouble(x)).ToList();
				intersection.Center = new Vector3D(center[0], center[1], center[2]);
			}
			
			if (jobj["Direction"] != null)
			{
				var direction = jobj["Direction"].ToString().Split(',').Select(x => Convert.ToDouble(x)).ToList();
				intersection.Direction = new Vector3D(direction[0], direction[1], direction[2]);
			}
			if (jobj["Min"] != null)
			{
				var min = jobj["Min"].ToString().Split(',').Select(x => Convert.ToDouble(x)).ToList();
				intersection.Min = new Vector3D(min[0], min[1], min[2]);
			}
			if (jobj["Max"] != null)
			{
				var max = jobj["Max"].ToString().Split(',').Select(x => Convert.ToDouble(x)).ToList();
				intersection.Max = new Vector3D(max[0], max[1], max[2]);
			}
			//if (jobj["Radius"] != null)
			//{
			//	intersection.Radius = Convert.ToDouble(jobj["Radius"]);
			//}

			return intersection;
		}

		public override string ToString()
		{
			JObject jobj = new JObject();

			if (Center != null)
				jobj["Center"] = $"{Center.X},{Center.Y},{Center.Z}";
			if (Direction != null)
				jobj["Direction"] = $"{Direction.X},{Direction.Y},{Direction.Z}";
			if (Min != null)
				jobj["Min"] = $"{Min.X},{Min.Y},{Min.Z}";
			if (Max != null)
				jobj["Max"] = $"{Max.X},{Max.Y},{Max.Z}";

			return JsonConvert.SerializeObject(jobj);
		}
	}
}
