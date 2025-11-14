using System.Threading;

namespace ClashSolver.Request
{
	//	A list of requests the dialog has available
	public enum ClashSolverRequestId : int
	{
		None = 0,

		// Validation & Resolution
		CopyFromLinks = 0x01,
		CreateLinkInstance,
		CopyElements,


		QuickDetection,
		RunValidation,
		PlaceMarkers,

		ReviewIssues = 0x10,
		IssueReport,
		IssueStatus,
		IssueVisibility,
		HighlightClash,
		ResetIssues,
		ComplianceHealthReport,

		CopyClashElements,

		CreateTags,
		FilterTags,
		UpdateIssues,
		AIResolve,
		ResolveIssue,
		ManualResolution,

		// Settings & Utilities
		ManageLinks,
		ManageLinksClosed,
		ClashSettings,
		ComplianceSettings,
		IssueMarkers,
		MarkerSetting,
		CostDatabase,
		ManageTeam,

		// ACC Integration
		Configuration,
		LinkModel,
		SyncModel,

		// Help & About
		Help,
		Tutorials,
		About,

		License,
		RequestValidate,
		RequestLicense
	}

	//	A class around a variable holding the current request.
	//	<remarks>
	//		Access to it is made thread-safe, even though we don't necessarily
	//		need it if we always disable the dialog between individual requests.
	//	</remarks>
	public class ClashSolverRequest
	{
		// Storing the value as a plain Int makes using the interlocking mechanism simpler
		private int m_request = (int)ClashSolverRequestId.None;

		//  Take - The Idling handler calls this to obtain the latest request. 
		//  <remarks>
		//      This is not a getter! It takes the request and replaces it
		//      with 'None' to indicate that the request has been "passed on".
		//  </remarks>
		public ClashSolverRequestId Take()
		{
			return (ClashSolverRequestId)Interlocked.Exchange(ref m_request, (int)ClashSolverRequestId.None);
		}

		//  Make - The Dialog calls this when the user presses a command button there. 
		//  <remarks>
		//      It replaces any older request previously made.
		//  </remarks>
		public void Make(ClashSolverRequestId request)
		{
			Interlocked.Exchange(ref m_request, (int)request);
		}
	}
}
