using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using ClashSolver.Resolver;
using ClashSolver.UI;
using ClashSolver.UI.Models;

namespace ClashSolver.Models
{
	public class Resolve : BaseModel
	{
		private int _no = 0;
		private ResolveType _type = ResolveType.None;
		private CSCategory _category;
		private long _targetId = 0;
		private List<long> _targetIds = new List<long>();
		private string _action = "";
		private Issue _issue;
		private string _description = "";
		private ResolveParameter _parameter = null;

		public int No
		{
			get => _no;
			set
			{
				_no = value;
			}
		}

		public ResolveType Type
		{
			get => _type;
			set => _type = value;
		}

		public CSCategory Category
		{
			get => _category;
			set
			{
				_category = value;
				OnPropertyChanged(nameof(Category));
			}
		}

		public string TargetName
		{
			get
			{
				return $"{Category.Name} - {TargetId}";
			}
		}

		public long TargetId
		{
			get => _targetId;
			set
			{
				_targetId = value;
				OnPropertyChanged(nameof(TargetId));
			}
		}

		public string Action
		{
			get => _action;
			set
			{
				_action = value;
				OnPropertyChanged(nameof(Action));
			}
		}

		public Issue Issue
		{
			get=> _issue;
			set
			{
				_issue = value;
				OnPropertyChanged(nameof(Issue));
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

		public ResolveParameter Parameter
		{
			get => _parameter;
			set
			{
				_parameter = value;
				OnPropertyChanged(nameof(Parameter));
			}
		}
	}
	public class ResolveParameter : BaseModel
	{

	}
	
	public class MoveResolveParameter : ResolveParameter
	{
		private double _x = 0;
		private double _y = 0;
		private double _z = 0;

		public double X
		{
			get => _x;
			set
			{
				_x = value;
				OnPropertyChanged(nameof(X));
			}
		}

		public double Y
		{
			get => _y;
			set
			{
				_y = value;
				OnPropertyChanged(nameof(Y));
			}
		}

		public double Z
		{
			get => _z;
			set
			{
				_z = value;
				OnPropertyChanged(nameof(Z));
			}
		}
	}

	public class OpeningResolveParameter: ResolveParameter
	{
		private OpeningHostType _type = OpeningHostType.None;

		private Solid _intersection;

		public OpeningHostType Type
		{
			get => _type;
			set => _type = value;
		}

		public Solid Intersection
		{
			get => _intersection;
			set => _intersection = value;
		}
	}
}
