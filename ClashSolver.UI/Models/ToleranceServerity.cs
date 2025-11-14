
namespace ClashSolver.UI.Models
{
	public class SeverityLevel: BaseModel
	{
		#region Fields

		private double _high = 0;
		private double _medium = 0;
		private double _low = 0;

		#endregion

		#region Properties

		public double High
		{
			get { return _high; }
			set
			{
				_high = value;
				OnPropertyChanged(nameof(High));
			}
		}

		public double Medium
		{
			get { return _medium; }
			set
			{
				_medium = value;
				OnPropertyChanged(nameof(Medium));
			}
		}

		public double Low
		{
			get { return _low; }
			set
			{
				_low = value;
				OnPropertyChanged(nameof(Low));
			}
		}

		#endregion
	}
}
