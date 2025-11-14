using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

using ClashSolver.UI.Controllers;
using ClashSolver.UI.TableAdapters;
using ClashSolver.UI.Models;
using Components;

namespace ClashSolver.UI.Views.ValidationResolution
{
	/// <summary>
	/// Interaction logic for WindowQuickDetection.xaml
	/// </summary>
	public partial class WindowQuickDetection : Window, IExternal
	{
		private readonly QuickDetectionUIController _formController;

		private bool _isDisposed = false;

		private CancellationTokenSource _cancellaionTokenSource;

		public WindowQuickDetection()
		{
			InitializeComponent();
		}

		public WindowQuickDetection(QuickDetectionUIController controller)
		{
			InitializeComponent();

			_formController = controller;
			DataContext = _formController;

			WakeUp();

			Closed += OnClosed;
		}

		#region IExternal interface implementation
		private void OnClosed(object sender, EventArgs e)
		{
			_isDisposed = true;
		}

		public int GetRequestId()
		{
			return _formController.GetRequestId();
		}

		public void MakeRequest(int request)
		{
			_formController.MakeRequest(request);

			DozeOff();
		}

		public void DozeOff()
		{

		}


		public void WakeUp(bool bFinish = false)
		{
			if (bFinish)
			{
				Close();
				return;
			}

			_formController.WakeUp(bFinish);
		}

		public void IClose()
		{
			if (!_isDisposed)
			{
				Close();
				_isDisposed = true;
			}
		}

		public bool IVisible()
		{
			return Visibility == System.Windows.Visibility.Visible;
		}

		public bool IIsDisposed()
		{
			return _isDisposed;
		}

		public void IShow()
		{
			if (!_isDisposed)
			{
				ShowDialog();
			}
		}
		#endregion

		#region Event Handlers

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			_formController.DeleteIssues();

			Close();

			ProgressWindow progressWindow = new();
			progressWindow.Title = "Clash Detection";
			progressWindow.Closed += ProgressWindow_Closed;

			_cancellaionTokenSource = new CancellationTokenSource();

			Task.Run(() => DoWork(_cancellaionTokenSource.Token, progressWindow), _cancellaionTokenSource.Token);

			if (!progressWindow.ShowDialog().Value)
			{
				_formController.UpdateIssues();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void DoWork(CancellationToken cancellationToken, ProgressWindow progressWindow)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					// Get the Total Count of all the selected detectionsets
					int totalCount = _formController.GetTotalCount();

					progressWindow.MinValue = 0;
					progressWindow.MaxValue = totalCount;

					// Analyze the project and get the issues
					int temp = 0;
					var timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm");

					for (int i = 0; i < totalCount; i++)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}

						progressWindow.Percentage = ++temp;
						progressWindow.Description = "Processing " + temp + " of " + totalCount + " Elements";

						List<Issue> issues = _formController.FindClash(i);
						if (issues != null) 
						{
							foreach (var issue in issues)
							{
								issue.AnalyzedAt = timestamp;

								IssueTableAdapter.Instance.Insert(issue);
							}
						}
					}

					_formController.UpdateIssues();

					this.Dispatcher.Invoke(new Action(() => {
						progressWindow.Close();
					}));
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Task was canceled.");
			}
			finally
			{
				Console.WriteLine("Cleaning up resources");
			}

		}

		private void ProgressWindow_Closed(object sender, EventArgs e)
		{
			// Signal the cancellation
			_cancellaionTokenSource?.Cancel();
		}

		#endregion
	}
}
