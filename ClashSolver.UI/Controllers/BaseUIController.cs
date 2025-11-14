using ClashSolver.UI.Models;
using System.ComponentModel;

namespace ClashSolver.UI.Controllers
{
	public class BaseUIController: INotifyPropertyChanged
	{
		#region Properties

		public Project Project { get; set; }

		public bool IsValid { get; set; }

		#endregion

		#region IExternal implementation

		public virtual int GetRequestId()
		{
			return 0;
		}

		public virtual void MakeRequest(int request)
		{

		}

		public virtual void WakeUp(bool bFinish = false)
		{

		}

		#endregion

		#region Controller implementation

		public virtual void OnOK()
		{

		}

		public void OnCancel()
		{

		}

		public virtual string GetProjectId()
		{
			return "";
		}

		public virtual string GetProjectName()
		{
			return "";
		}

		public virtual string GetProjectVersion()
		{
			return "2025";
		}

		#endregion

		#region INotifyPropertyChanged interface implementation

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
