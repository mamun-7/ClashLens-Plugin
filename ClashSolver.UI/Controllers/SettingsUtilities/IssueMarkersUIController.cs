using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using MahApps.Metro.Actions;

namespace ClashSolver.UI.Controllers
{
	public class IssueMarkersUIController : BaseUIController
	{
		#region Fields

		private bool _isShowClashMarkers = false;
		private ColorCoding _textColor = new ColorCoding();
		private ColorCoding _boxColor = new ColorCoding();
		private int _sizeText = 200;
		private int _sizeBox = 200;
		private MarkerType _markerType = MarkerType.Box;
		private bool _isDisplayClashId = false;
		private bool _isDisplayClashType = false;

		private MarkerSetting _setting = new MarkerSetting();


		private	ObservableCollection<SelectableItem> _SelectableItems = new ObservableCollection<SelectableItem>();

		#endregion

		#region Properties

		public bool IsShowClashMarkers
		{
			get { return _isShowClashMarkers; }
			set
			{
				_isShowClashMarkers = value;
				OnPropertyChanged(nameof(IsShowClashMarkers));
			}
		}

		public ColorCoding TextColor
		{
			get => _textColor;
			set
			{
				_textColor = value;
				OnPropertyChanged(nameof(TextColor));
			}
		}

		public int TextSize
		{
			get => _sizeText;
			set
			{
				_sizeText = value;
				OnPropertyChanged(nameof(TextSize));
			}
		}

		public int BoxSize
		{
			get => _sizeBox;
			set
			{
				_sizeBox = value;
				OnPropertyChanged(nameof(BoxSize));
			}
		}

		public ColorCoding BoxColor
		{
			get => _boxColor;
			set
			{
				_boxColor = value;
				OnPropertyChanged(nameof(BoxColor));
			}
		}

		public MarkerType MarkerType
		{
			get => _markerType;
			set
			{
				_markerType = value;
				OnPropertyChanged(nameof(MarkerType));
			}
		}

		public bool IsDisplayClashId 
		{
			get { return _isDisplayClashId; }
			set
			{
				_isDisplayClashId = value;
				OnPropertyChanged(nameof(IsDisplayClashId));
			}
		}

		public bool IsDisplayClashType
		{
			get { return _isDisplayClashType; }
			set
			{
				_isDisplayClashType = value;
				OnPropertyChanged(nameof(IsDisplayClashType));
			}
		}

		public ObservableCollection<SelectableItem> SelectableItems
		{
			get => _SelectableItems;
			set
			{
				_SelectableItems = value;
				OnPropertyChanged(nameof(SelectableItems));
			}
		}

		#endregion

		#region Constructors

		//	Test Constructor for UITest
		public IssueMarkersUIController()
		{
			SelectableItems = new ObservableCollection<SelectableItem>()
			{
				new SelectableItem() { Name = "Clash ID" },
				new SelectableItem() { Name = "Severity" },
				new SelectableItem() { Name = "Clash Type" }
			};

			_setting = SettingTableAdpater.Instance.GetAll().Cast<MarkerSetting>().FirstOrDefault();

			if (_setting == null)
			{
				_setting = new MarkerSetting();
			}
			else
			{
				IsShowClashMarkers = _setting.IsShowClashMarker;
				TextColor = new ColorCoding()
				{
					HighColor = _setting.TextHighColor,
					MediumColor = _setting.TextMediumColor,
					LowColor = _setting.TextLowColor
				};
				BoxColor = new ColorCoding()
				{
					HighColor = _setting.TextHighColor,
					MediumColor = _setting.TextMediumColor,
					LowColor = _setting.TextLowColor
				};
				TextSize = _setting.MarkerSize;
				BoxSize = _setting.BoxSize;
				MarkerType = _setting.MarkerType;
				IsDisplayClashId = _setting.IsDisplayClashId;
				IsDisplayClashType = _setting.IsDisplayClashType;
			}
		}

		#endregion

		public virtual bool Validate() 
		{
			if (TextSize < 100 || TextSize > 500)
			{
				MessageBox.Show("Marker Size should be between 100 and 500", "Error");

				return false;
			}
			return true; 
		}

		public override void OnOK()
		{
			OnSetMarkerSettings();
		}

		private void OnSetMarkerSettings()
		{
			_setting.IsShowClashMarker = IsShowClashMarkers;
			_setting.TextHighColor = TextColor.HighColor;
			_setting.TextMediumColor = TextColor.MediumColor;
			_setting.TextLowColor = TextColor.LowColor;
			_setting.MarkerSize = TextSize;
			_setting.BoxSize = BoxSize;
			_setting.MarkerType = MarkerType;
			_setting.IsDisplayClashId = IsDisplayClashId;
			_setting.IsDisplayClashType= IsDisplayClashType;

			if (SettingTableAdpater.Instance.GetAll().Count == 0)
			{
				SettingTableAdpater.Instance.Insert(_setting);
			}
			else
			{
				SettingTableAdpater.Instance.Update(_setting);
			}
		}
	}
}
