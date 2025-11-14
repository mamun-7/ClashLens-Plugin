using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Architexor.Core
{
	public static class UpdateHelper
	{
		//	Related to check update feature
		public struct AddInFile
		{
			public string Name;
			public Dictionary<int, string> Versions;
		}

		public static List<AddInFile> GetFileList()
		{
			List<AddInFile> files = new();

			try
			{
				string sRes = ApiService.GetResponse(Constants.API_ENDPOINT + "file");

				JArray jArr = JArray.Parse(sRes);
				foreach (JObject obj in jArr)
				{
					AddInFile aif = new AddInFile()
					{
						Name = obj.GetValue("name").ToString(),
						Versions = new Dictionary<int, string>()
					};
					if (obj.ContainsKey("versions"))
					{
						JObject versions = (JObject)obj.GetValue("versions");
						for (int version = 2019; version < 2030; version++)
						{
							if (versions.ContainsKey(version.ToString()))
							{
								aif.Versions[version] = versions.GetValue(version.ToString()).ToString();
							}
						}
					}

					files.Add(aif);
				}
			}
			catch (Exception)
			{
				//MessageBox.Show("");
			}

			return files;
		}

		public static bool CheckForUpdate(int nRevitVersion)
		{
			string url = Assembly.GetExecutingAssembly().Location;
			url = url.Substring(0, url.LastIndexOf("\\")) + "\\";
			bool bNeed = false;

			try
			{
				List<AddInFile> files = GetFileList();

				foreach (AddInFile aif in files)
				{
					//	Check if needs
					if (!File.Exists(url + aif.Name))
					{
						MessageBox.Show(aif.Name + " needs to be downloaded.");
						bNeed = true;
					}
					else
					{
						//	Check file version
						if (aif.Name.EndsWith(".dll"))
						{
							FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(url + aif.Name);
							_ = new Version(fvi.FileVersion).CompareTo(new Version(aif.Versions[nRevitVersion])) < 0 ? bNeed = true : bNeed = false;
							if (bNeed)
								MessageBox.Show(aif.Name + " " + fvi.FileVersion + " needs to be updated to " + aif.Versions[nRevitVersion] + ".");
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

							if (bNeed)
								MessageBox.Show(aif.Name + " needs to be updated.");
						}
					}
					if (bNeed)
					{
						//	Need to update
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			if (bNeed)
			{
				
			}
			return bNeed;
		}
	}
}
