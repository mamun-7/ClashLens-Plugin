using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClashSolver.UI.TableAdapters;
using ForgeAPI.Helpers;
using ForgeAPI.Models;

namespace ClashSolver.UI.Controllers
{
	public class LinkModelUIController : BaseUIController
	{
		#region Fields

		private ObservableCollection<ACCProject> _projects = new ObservableCollection<ACCProject>();
		private ACCProject _selectedProject = new ACCProject();
		private ObservableCollection<ACCFolder> _folders = new ObservableCollection<ACCFolder>();
		private ACCFolder _selectedFolder = new ACCFolder();
		private bool _isLoading = false;

		#endregion

		#region Properties

		public ObservableCollection<ACCProject> Projects
		{
			get
			{
				return _projects;
			}
			set
			{
				_projects = value;
				OnPropertyChanged(nameof(Projects));
			}
		}

		public ACCProject SelectedProject
		{
			get
			{
				return _selectedProject;
			}
			set
			{
				_selectedProject = value;
				OnPropertyChanged(nameof(SelectedProject));
			}
		}

		public ObservableCollection<ACCFolder> Folders
		{
			get
			{
				return _folders;
			}
			set
			{
				_folders = value;
				OnPropertyChanged(nameof(Folders));
			}
		}

		public ACCFolder SelectedFolder
		{
			get
			{
				return _selectedFolder;
			}
			set
			{
				_selectedFolder = value;
				OnPropertyChanged(nameof(SelectedFolder));
			}
		}

		public Auth Auth { get; set; }

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				_isLoading = value;
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		#endregion

		#region Constructors

		public LinkModelUIController()
		{
			Initialize();
		}

		public void Initialize()
		{
			Auth = ACCUserTableAdapter.Instance.GetLoginUser() as Auth;

			//GetProjects();

			UpdateFolders();
		}

		#endregion

		#region Event Handlers

		public async void GetProjects()
		{
			IsLoading = true;

			List<ACCProject> temp = new List<ACCProject>();

			var hubs = await APSHelper.Instance.ExploreACCAsync(Auth.AccessToken);

			IsLoading = false;
			foreach (var hub in hubs)
			{
				foreach (var project in hub.Projects)
				{
					temp.Add(project);
				}
			}

			Projects = [.. temp];

			if(Projects.Count  > 0)
			{
				SelectedProject = Projects[0];
			}

		}

		public async void UpdateFolders()
		{
			if (SelectedProject == null || !SelectedProject.IsValid())
			{
				return;
			}

			IsLoading = true;

			var folders = await APSHelper.Instance.ExploreFolderAsync(Auth.AccessToken, SelectedProject.Id, SelectedProject.RootFolderId, "\t\t");
			Folders = new ObservableCollection<ACCFolder>(folders.Where(x => x.Name == "Project Files"));

			IsLoading = false;
		}

		public void OnLink()
		{
			if(SelectedFolder == null)
			{
				return;
			}
		}

		public async void OnUpload()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = openFileDialog.FileName;
				string fileName = Path.GetFileName(filePath);

				if (SelectedProject == null || !SelectedProject.IsValid() || SelectedFolder == null)
				{
					MessageBox.Show("Please select a valid project and folder.");
					return;
				}

				IsLoading = true;

				try
				{
					byte[] fileContent = File.ReadAllBytes(filePath);
					var result = await APSHelper.Instance.UploadFileAsync(Auth.AccessToken, SelectedProject.Id, SelectedFolder.Id, filePath);

					if (!string.IsNullOrEmpty(result))
					{
						MessageBox.Show("File uploaded successfully.");
						UpdateFolders(); // Refresh the folder contents
					}
					else
					{
						MessageBox.Show("File upload failed.");
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"An error occurred: {ex.Message}");
				}
				finally
				{
					IsLoading = false;
				}
			}
		}

		#endregion

		#region Helper Methods

		public bool IsLinkValid()
		{
			return SelectedFolder.Type == "items" && SelectedFolder.Name.Split('.').Last() == "rvt";
		}

		#endregion
	}
}
