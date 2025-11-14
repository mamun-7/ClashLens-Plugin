
using System.Collections.Generic;

namespace ClashSolver.UI.Models
{
	public class LinkedModel : SelectableItem
	{
		#region Fields

		private int _no;
		private long _projectId;
		private long _elementId;
		private string _url = "";
		private LinkDiscipline _discipline = LinkDiscipline.None;
		private string _assignee = "";
		private string _description = "";
		private long _instanceId = 0;

		#endregion

		#region Properties

		public int No
		{
			get => _no;
			set
			{
				_no = value;
				OnPropertyChanged(nameof(No));
			}
		}

		public long ProjectId
		{
			get => _projectId;
			set
			{
				_projectId = value;
			}
		}

		public long ElementId
		{
			get => _elementId;
			set
			{
				_elementId = value;
			}
		}

		public string Url
		{
			get => _url;
			set
			{
				_url = value;
				OnPropertyChanged(nameof(Url));
			}
		}

		public LinkDiscipline Discipline
		{
			get => _discipline;
			set
			{
				_discipline = value;
				OnPropertyChanged(nameof(Discipline));
			}
		}

		public string DisciplineStr
		{
			get => _discipline.ToString();
		}

		public string Assignee
		{
			get => _assignee;
			set
			{
				_assignee = value;
				OnPropertyChanged(nameof(Assignee));
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

		public long InstanceId
		{
			get => _instanceId;
			set
			{
				_instanceId = value;
			}
		}

		// Store the used families in the linked model
		public List<long> UsedCategories { get; set; } = [];

		#endregion
	}
}
