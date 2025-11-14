using Architexor.Core;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoUpdater
{
	public partial class MainForm : Form
	{
		private Thread threadUpdate = null;
		public static int STATUS = 0;

		private delegate void SetTextCallback(string text);

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			//	Read Setting
			try
			{
				string sSettings = File.ReadAllText("settings.ini");
				string[] settings = sSettings.Split('\n');
				foreach (string setting in settings)
				{
					string name = setting.Split('=')[0], value = setting.Split('=')[1];
					switch (name)
					{
						case "API_ENDPOINT":
							//API_ENDPOINT = value;
							break;
						default:
							break;
					}
				}
			}
			catch (Exception) { }

			threadUpdate = new Thread(new ThreadStart(AutoUpdate));
			threadUpdate.Start();
		}

		private void AutoUpdate()
		{
			string url = Assembly.GetExecutingAssembly().Location;
			url = url.Substring(0, url.LastIndexOf("\\")) + "\\";

#if REVIT2019
			int nRevitVersion = 2019;
#elif REVIT2020
			int nRevitVersion = 2020;
#elif REVIT2021
			int nRevitVersion = 2021;
#elif REVIT2022
			int nRevitVersion = 2022;
#elif REVIT2023
			int nRevitVersion = 2023;
#elif REVIT2024
			int nRevitVersion = 2024;
#elif REVIT2025
			int nRevitVersion = 2025;
#elif REVIT2026
			int nRevitVersion = 2026;
#elif REVIT2027
			int nRevitVersion = 2027;
#elif REVIT2028
			int nRevitVersion = 2028;
#elif REVIT2029
			int nRevitVersion = 2029;
#elif REVIT2030
			int nRevitVersion = 2030;
#endif

			while (true)
			{
				if (STATUS == 0)
				{
					FileInfo file = new(url + "ClashSolver.dll");
					if (IsFileLocked(file))
					{
						Thread.Sleep(1000);
					}
					else
					{
						STATUS = 1;
						ChangeStatus("Checking Update...");
					}
				}
				else if (STATUS == 1)
				{
					RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\" + Constants.BRAND, true);
					if (key == null)
					{
						key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\" + Constants.BRAND, true);
					}
					if (key != null)
					{
						Constants.thisUser.Id = key.GetValue("UserId") != null ? key.GetValue("UserId").ToString() : "";
						Constants.thisUser.FirstName = key.GetValue("FirstName") != null ? key.GetValue("FirstName").ToString() : "";
						Constants.thisUser.LastName = key.GetValue("LastName") != null ? key.GetValue("LastName").ToString() : "";
						Constants.thisUser.Email = key.GetValue("Email") != null ? key.GetValue("Email").ToString() : "";

						string sRes = "";
						try
						{
							JObject jObj = new JObject();
							jObj.Add("email", Constants.thisUser.Email);

							sRes = ApiService.PostSync(Constants.API_ENDPOINT + "user", jObj.ToString());
							jObj = JObject.Parse(sRes);
							Constants.thisUser.Token = jObj.GetValue("token").ToString();
						}
						catch (Exception e)
						{
							MessageBox.Show("Can not connect to server. Please contact developer. Error: " + e.Message);
							break;
						}

						List<UpdateHelper.AddInFile> files = UpdateHelper.GetFileList();

						Invoke((MethodInvoker)delegate
						{
							pbUpdate.Maximum = files.Count;
							pbUpdate.Value = 0;
						});

						foreach (UpdateHelper.AddInFile aif in files)
						{
							Invoke((MethodInvoker)delegate
							{
								pbUpdate.Value++;
							});
							ChangeStatus("Updating " + aif.Name + "...");

							//	Check if needs
							bool bNeed = false;
							if (!File.Exists(url + aif.Name))
								bNeed = true;
							else
							{
								if (aif.Versions.ContainsKey(nRevitVersion))
								{
									//	Check file version
									if (aif.Name.EndsWith(".dll"))
									{
										FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(url + aif.Name);
										_ = new Version(fvi.FileVersion).CompareTo(new Version(aif.Versions[nRevitVersion])) < 0 ? bNeed = true : bNeed = false;
									}
									else
									{
										string sChecksum;
										using (var md5 = MD5.Create())
										{
											using (var stream = File.OpenRead(url + aif.Name))
											{
												byte[] hash = md5.ComputeHash(stream);
												sChecksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
											}
										}
										if (aif.Versions[nRevitVersion] != sChecksum && aif.Versions[nRevitVersion] != "")
											bNeed = true;
									}
								}
								else
								{
									bNeed = false;
								}
							}
							if (!bNeed)
							{
								continue;
							}

							//	Download and replace
							string ext = aif.Name.Substring(aif.Name.Length - 3);
							string backup = url + aif.Name.Substring(0, aif.Name.Length - 4) + "_bak." + ext;
							string data = ApiService.GetResponse(Constants.API_ENDPOINT + "file/download?version=" + nRevitVersion + "&name=" + aif.Name, backup);

							//	Compare with the original file
							if (File.Exists(url + aif.Name))
							{
								File.Delete(url + aif.Name);
							}
							File.Move(backup, url + aif.Name);
						}
						STATUS = 2;
						break;
					}
					else
					{
						MessageBox.Show("This tool is available for registered users only.");
						break;
					}
				}
			}
			Invoke((MethodInvoker)delegate
			{
				// close the form on the forms thread
				Close();
			});
		}

		private void ChangeStatus(string sText)
		{
			if (lblStatus.InvokeRequired)
			{
				SetTextCallback d = new(ChangeStatus);
				Invoke(d, new object[] { sText });
			}
			else
			{
				lblStatus.Text = sText;
			}
		}

		protected virtual bool IsFileLocked(FileInfo file)
		{
			try
			{
				using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
				stream.Close();
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}

			//file is not locked
			return false;
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			threadUpdate.Abort();
		}
	}
}
