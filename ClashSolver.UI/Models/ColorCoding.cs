using System.Windows.Media;

namespace ClashSolver.UI.Models
{
	public class ColorCoding : BaseModel
	{
		#region Fields

		private Color _highColor = Colors.Red;
		private Color _mediumColor = Colors.Green;
		private Color _lowColor = Colors.Blue;

		#endregion

		#region Properties

		public Color HighColor
		{
			get { return _highColor; }
			set
			{
				_highColor = value;
				OnPropertyChanged(nameof(HighColor));
			}
		}
		public Color MediumColor
		{
			get { return _mediumColor; }
			set
			{
				_mediumColor = value;
				OnPropertyChanged(nameof(MediumColor));
			}
		}
		public Color LowColor
		{
			get { return _lowColor; }
			set
			{
				_lowColor = value;
				OnPropertyChanged(nameof(LowColor));
			}
		}

		#endregion
	}
}
