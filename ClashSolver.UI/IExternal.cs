namespace ClashSolver.UI
{
	/// <summary>
	/// We used ExternalEvent to hand over the control between Revit and WinForms
	/// </summary>
	public interface IExternal
	{
		int GetRequestId();

		//	A private helper method to make a request
		//	and put the dialog to sleep at the same time.
		//	<remarks>
		//		It is expected that the process which executes the request 
		//		(the Idling helper in this particular case) will also
		//		wake the dialog up after finishing the execution.
		//	</remarks>
		void MakeRequest(int request);

		//	DozeOff -> disable all controls (but the Exit button)
		void DozeOff();

		void WakeUp(bool bFinish = false);

		void IClose();

		bool IVisible();

		bool IIsDisposed();

		void IShow();
	}
}
