using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors.Media;

namespace ClashSolver.UI.Models
{
	public class MarkerSetting : BaseModel
	{
		public long Id { get; set; }

		// Issue marker setting
		/// <summary>
		/// Whether show marker in the project or not.
		/// </summary>
		public bool IsShowClashMarker { get; set; } = true;

		/// <summary>
		/// Color of high clashed marker
		/// </summary>
		public Color TextHighColor { get; set; } = Colors.Yellow;

		/// <summary>
		/// Color of medium clashed marker
		/// </summary>
		public Color TextMediumColor { get; set; } = Colors.Green;

		/// <summary>
		/// Color of low clashed marker
		/// </summary>
		public Color TextLowColor { get; set; } = Colors.Blue;

		/// <summary>
		/// Size of marker
		/// </summary>
		public int MarkerSize { get; set; } = 100;

		/// <summary>
		/// Size of marker
		/// </summary>
		public int BoxSize { get; set; } = 100;

		/// <summary>
		/// Color of high clashed marker
		/// </summary>
		public Color BoxHighColor { get; set; } = Colors.Yellow;

		/// <summary>
		/// Color of medium clashed marker
		/// </summary>
		public Color BoxMediumColor { get; set; } = Colors.Green;

		/// <summary>
		/// Color of low clashed marker
		/// </summary>
		public Color BoxLowColor { get; set; } = Colors.Blue;

		/// <summary>
		/// Shape of marker - bubble, box
		/// </summary>
		public MarkerType MarkerType { get; set; } = MarkerType.Box;

		/// <summary>
		/// Display information
		/// </summary>
		public bool IsDisplayClashId { get; set; } = true;

		/// <summary>
		/// Display information
		/// </summary>
		public bool IsDisplayClashType { get; set; } = false;
	}
}
