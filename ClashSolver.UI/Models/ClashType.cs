
namespace ClashSolver.UI.Models
{
	public class ClashType : BaseModel
	{
		#region Fields

		private bool _isHardClash = true;
		private bool _isSoftClash = false;
		private bool _isWorkflowClash = false;

		#endregion

		#region Properties

		public bool IsHardClash
		{
			get { return _isHardClash; }
			set
			{
				_isHardClash = value;
				OnPropertyChanged(nameof(IsHardClash));
			}
		}

		public bool IsSoftClash
		{
			get { return _isSoftClash; }
			set
			{
				_isSoftClash = value;
				OnPropertyChanged(nameof(IsSoftClash));
			}
		}

		public bool IsWorkflowClash
		{
			get { return _isWorkflowClash; }
			set
			{
				_isWorkflowClash = value;
				OnPropertyChanged(nameof(IsWorkflowClash));
			}
		}

		#endregion
	}
}
