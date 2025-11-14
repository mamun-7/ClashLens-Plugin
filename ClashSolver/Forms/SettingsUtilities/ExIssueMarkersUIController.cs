using System.Text.Json.Serialization;
using Autodesk.Revit.UI;
using ClashSolver.Controllers;
using ClashSolver.Request;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using System.Text.Json;
using ClashSolver.UI;
using DocumentFormat.OpenXml.Presentation;
using System.Diagnostics;
using ClashSolver.UI.TableAdapters;

namespace ClashSolver.Forms.Controllers
{
	public class ExIssueMarkersUIController : IssueMarkersUIController
	{
		#region Fields

		private ClashSolverRequestHandler m_Handler;
		private ExternalEvent m_ExEvent;

		#endregion

		#region Properties

		public ClashSolverRequestHandler Handler { get => m_Handler; }
		public ClashSolverRequestId LastRequestId { get; set; } = ClashSolverRequestId.None;

		#endregion

		#region Constructors

		public ExIssueMarkersUIController(ClashSolverRequestId reqId, UIApplication uiApp)
		{
			//	A new handler to handle request posting by the dialog
			m_Handler = new ClashSolverRequestHandler(reqId, uiApp);

			//	External Event for the dialog to use (to post requests)
			m_ExEvent = ExternalEvent.Create(m_Handler);

			//	Initialize Data Context
			IssueMarkersController ins = m_Handler.Instance as IssueMarkersController;
			ins.Initialize();

			if (Application.thisApp.Setting != null)
			{
				MarkerSetting setting = Application.thisApp.Setting;
				IsShowClashMarkers = setting.IsShowClashMarker;
				TextColor = new ColorCoding()
				{
					HighColor = setting.TextHighColor,
					MediumColor = setting.TextMediumColor,
					LowColor = setting.TextLowColor
				};
				TextSize = setting.MarkerSize;
				MarkerType = (MarkerType)setting.MarkerType;
			}
			WakeUp();
		}

		#endregion

		#region IExternal implementation
		public override void MakeRequest(int request)
		{
			LastRequestId = (ClashSolverRequestId)request;

			m_Handler.Request.Make(LastRequestId);
			m_ExEvent.Raise();
		}

		public override void WakeUp(bool bFinish = false)
		{
		}

		public override int GetRequestId()
		{
			if (m_Handler == null)
			{
				return (int)ClashSolverRequestId.None;
			}
			return (int)m_Handler.RequestId;
		}
		#endregion

		#region Controller implementation
		public override bool Validate()
		{
			if(TextSize < 100 || TextSize > 500)
			{
				TaskDialog.Show("Error", "Marker Size should be between 100 and 500");

				return false;
			}

			return true;
		}

		public override void OnOK()
		{
			OnSetMarkerSettings();

			MakeRequest((int)ClashSolverRequestId.MarkerSetting);
		}

		private void OnSetMarkerSettings()
		{
			if (Application.thisApp.Setting == null)
				Application.thisApp.Setting = new MarkerSetting();

			MarkerSetting setting = Application.thisApp.Setting;

			setting.IsShowClashMarker = IsShowClashMarkers;
			setting.TextHighColor = TextColor.HighColor;
			setting.TextMediumColor = TextColor.MediumColor;
			setting.TextLowColor = TextColor.LowColor;
			setting.MarkerSize = TextSize;
			setting.BoxSize = BoxSize;
			setting.MarkerType = MarkerType;
			setting.IsDisplayClashId = IsDisplayClashId;
			setting.IsDisplayClashType = IsDisplayClashType;

			if (SettingTableAdpater.Instance.GetAll().Count == 0)
			{
				SettingTableAdpater.Instance.Insert(setting);
			}
			else
			{
				SettingTableAdpater.Instance.Update(setting);
			}
		}
		#endregion
	}
}
