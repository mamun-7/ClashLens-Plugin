using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Components
{
	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window, INotifyPropertyChanged
	{

		private string _message = "";
		private string _description = "";
		private int _minValue = 0;
		private int _maxValue = 100;
		private int _percentage = 0;

		public string Message 
		{
			get => _message;
			set
			{
				_message = value;
				OnPropertyChanged(nameof(Message));
			}
		}

		public string Description
		{ 
			get => _description;
			set 
			{
				_description = value;
				OnPropertyChanged(nameof(Description));
			}
		}

		public int MaxValue
		{
			get => _maxValue;
			set
			{
				_maxValue = value;
				OnPropertyChanged(nameof(MaxValue));
			}
		}

		public int Percentage
		{
			get => _percentage;
			set
			{
				_percentage = value;
				OnPropertyChanged(nameof(Percentage));
			}
		}

		public int MinValue
		{
			get => _minValue;
			set
			{
				_minValue = value;
				OnPropertyChanged(nameof(MinValue));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ProgressWindow()
		{
			InitializeComponent();

			this.DataContext = this;

			//SetMinimum(0);
			//SetMaximum(100);
		}

		// Notify when a property has changed
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		//public void SetMessage(string message)
		//{
		//	if (lblMessage.InvokeRequired)
		//	{
		//		lblMessage.Invoke(new Action(() => lblMessage.Text = message));
		//	}
		//	else
		//	{
		//		lblMessage.Text = message;
		//	}
		//}

		//public void SetDescription(string description)
		//{
		//	if (lblDescription.InvokeRequired)
		//	{
		//		lblDescription.Invoke(new Action(() => lblDescription.Text = description));
		//	}
		//	else
		//	{
		//		lblDescription.Text = description;
		//	}
		//}

		//public void SetMinimum(int minimum)
		//{
		//	progressBar.Minimum = minimum;
		//}

		//public void SetMaximum(int maximum)
		//{
		//	progressBar.Maximum = maximum;
		//}

		//public void SetProgress(int value)
		//{
		//	progressBar.Value = value;
		//}

		//public void UpdateProgress(int value)
		//{
		//	if (progressBar.InvokeRequired)
		//	{
		//		progressBar.Invoke(new Action(() => progressBar.Value = value));
		//	}
		//	else
		//	{
		//		progressBar.Value = value;
		//	}
		//}
	}
}
