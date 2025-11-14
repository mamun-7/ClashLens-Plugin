
namespace ClashSolver.UI.Models
{
	public class Settings : BaseModel
	{
		#region Fields
		// Opening Settings
		private bool _isCreateVerticalOpening = false;
		private bool _isCreateHorizontalOpening = false;
		private double _minOpeningSize = 30;
		private double _minDiameterToRect = 300;
		private double _minDistanceToJoin = 100;
		private double _minOpeningSlope = 45;
		private RoundupType _roundupType = RoundupType.None;
		private bool _isCreateSlopeOpening = false;

		// Reroute Settings
		private Direction _rerouteDir = Direction.Up;
		#endregion

		#region Properties

		public Issue Issue { get; set; }

		# region Opening Settings
		/// <summary>
		/// When the pipe and host element is intersected vertically, check whether create opening or not.
		/// </summary>
		public bool IsCreateVerticalOpening
		{
			get { return _isCreateVerticalOpening; }
			set { _isCreateVerticalOpening = value; OnPropertyChanged(nameof(IsCreateVerticalOpening)); }
		}

		/// <summary>
		/// When the pipe and host element is intersected horizontally, check whether create opening or not.
		/// </summary>
		public bool IsCreateHorizontalOpening
		{
			get { return _isCreateHorizontalOpening; }
			set { _isCreateHorizontalOpening = value; OnPropertyChanged(nameof(IsCreateHorizontalOpening)); }
		}

		/// <summary>
		/// When the intersection size of opening is smaller than this value, opening is ignored.
		/// </summary>
		public double MinOpeningSize
		{
			get { return _minOpeningSize; }
			set { _minOpeningSize = value; OnPropertyChanged(nameof(MinOpeningSize)); }
		}

		/// <summary>
		/// When the intersection size of opening is bigger than this value, opening shape should be rectangular.
		/// </summary>
		public double MinDiameterToRect
		{
			get { return _minDiameterToRect; }
			set { _minDiameterToRect = value; OnPropertyChanged(nameof(MinDiameterToRect)); }
		}

		/// <summary>
		/// Minumum distance to should be join each other for nearest elements.
		/// </summary>
		public double MinDistanceToJoin
		{
			get { return _minDistanceToJoin; }
			set { _minDistanceToJoin = value; OnPropertyChanged(nameof(MinDistanceToJoin)); }
		}

		/// <summary>
		///  When the angle between pipe and host element is bigger than this value, can't create opening.
		/// </summary>
		public double MinOpeningSlope
		{
			get { return _minOpeningSlope; }
			set { _minOpeningSlope = value; OnPropertyChanged(nameof(MinOpeningSlope)); }
		}

		/// <summary>
		/// Check whether create opening slanting and vertically between pipe and host element.
		/// </summary>
		public bool IsCreateSlopeOpening
		{
			get { return _isCreateSlopeOpening; }
			set { _isCreateSlopeOpening = value; OnPropertyChanged(nameof(IsCreateSlopeOpening)); }
		}

		/// <summary>
		/// The correctness of opening size
		/// </summary>
		public RoundupType RoundupType
		{
			get { return _roundupType; }
			set { _roundupType = value; OnPropertyChanged(nameof(RoundupType)); }
		}
		#endregion

		#region Rerouting Settings
		public Direction ReRouteDirection
		{
			get => _rerouteDir;
			set
			{
				_rerouteDir = value;
				OnPropertyChanged(nameof(ReRouteDirection));
			}
		}
		#endregion

		#endregion
	}
}
