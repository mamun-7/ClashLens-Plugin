using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
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
using System.Xml.Linq;
using ClashSolver.UI.Controllers;
using ClashSolver.UI.Models;
using ClashSolver.UI.TableAdapters;
using Components;

namespace ClashSolver.UI.ValidationResolution.RunValidation
{
	/// <summary>
	/// Interaction logic for RunValidation.xaml
	/// </summary>
	public partial class WindowRunValidation : Window, IExternal
	{
		private readonly RunValidationUIController _formController;

		private bool _isDisposed = false;

		private CancellationTokenSource _cancellaionTokenSource;

		private List<Issue> _totalIssues = new List<Issue>();

		protected WindowRunValidation()
		{
			InitializeComponent();
		}

		public WindowRunValidation(RunValidationUIController controller)
		{
			InitializeComponent();

			_formController = controller;
			DataContext = _formController;

			Initialize();

			WakeUp();

			Closed += OnClosed;
		}

		private void Initialize()
		{
			selScopes.Items = _formController.Scopes;
			selLevels.Items = _formController.Levels;
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
		private void btnReset_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnRun_Click(object sender, RoutedEventArgs e)
		{
			Close();

			_totalIssues.Clear();
			if (!_formController.RetrieveElementsToBeAnalyzed())
			{
				return;
			}

			ProgressWindow progressWindow = new()
			{
				Title = "Clash Detection"
			};
			progressWindow.Closed += ProgressWindow_Closed;

			_cancellaionTokenSource = new CancellationTokenSource();

			Task.Run(() => DoWork(_cancellaionTokenSource.Token, progressWindow), _cancellaionTokenSource.Token);

			if (!progressWindow.ShowDialog().Value)
			{
				return;
			}
		}

		private void DoWork(CancellationToken cancellationToken, ProgressWindow progressWindow)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var sets = _formController.Sets.Where(x => x.IsSelected).ToList();

					// Get the Total Count of all the IsSelected detectionsets
					var elements = _formController.Elements;

					progressWindow.MinValue = 0;
					progressWindow.MaxValue = elements.Count;

					// Analyze the project and get the issues
					int temp = 0;

					foreach (var set in sets)
					{
						if (cancellationToken.IsCancellationRequested)
							break;

						progressWindow.Message = set.Name;

						foreach (var element in elements.Where(x => x.Set == set.Id))
						{
							if (cancellationToken.IsCancellationRequested)
								break;

							try
							{
								temp++;
								progressWindow.Percentage = temp;
								progressWindow.Description = $"Processing {temp} of {elements.Count} Elements";
								var issues = _formController.FindClash(element);

								if(issues.Count > 0)
								{
									_totalIssues.AddRange(issues);
								}
							}
							catch (Exception ex)
							{
								continue;
							}
						}

					}

					//if(_totalIssues.Count == 0)
					//{
					//	MessageBox.Show("There are no conflicting elements.", "Information");
					//}

					// Filter Total Issues by removing items where ElementIdA and ELementIdB are simply swapped.
					var uniquePairs = new HashSet<(long, long)>();
					_totalIssues = _totalIssues.Where(issue =>
					{
						// Create a normalized pair(min, max) so (A,B) and (B,A) are the same.
						var pair = issue.ElementIdA < issue.ElementIdB
							? (issue.ElementIdA, issue.ElementIdB)
							: (issue.ElementIdB, issue.ElementIdA);

						// Only add if not already present
						if (uniquePairs.Contains(pair))
							return false;
						uniquePairs.Add(pair);
						return true;
					})
					.ToList();

					_formController.UpdateIssues(_totalIssues);

					this.Dispatcher.Invoke(new Action(() =>
					{
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

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnCopilot_Click(object sender, RoutedEventArgs e)
		{

		}
		#endregion
	}
}
