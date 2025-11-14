using ClashSolver.UI.Models;

namespace ClashSolver.UI.Controllers
{
	public class ResolveUIController : BaseUIController
	{
		#region Fields

		Settings _settings = new Settings();

		#endregion

		#region Properties

		public Settings Settings
		{
			get { return _settings; }
			set { _settings = value; OnPropertyChanged(nameof(Settings)); }
		}

		public virtual void OnSelectTarget(){}

		#endregion
	}
}
