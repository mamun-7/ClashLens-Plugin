using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForgeAPI.Models;
using System.IO;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using Autodesk.Forge.Core;
using Autodesk.ModelDerivative.Model;


namespace ForgeAPI.Helpers
{
	/// <summary>
	/// Helper class for APS (Autodesk Platform Services) related operations.
	/// </summary>
	public class APSHelper
	{
		private const string BaseUrl = "https://developer.api.autodesk.com";
		private const string TokenEndpoint = "/authentication/v2/token";

		private static APSHelper _instance = null;

		/// <summary>
		/// Gets the singleton instance of the APSHelper class.
		/// </summary>
		public static APSHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new APSHelper();
				}
				return _instance;
			}
		}

		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}

		public async Task<TokenData> GetAccessTokenAsync(string clientId, string clientSecret, string scopes = "data:read data:write data:create")
		{
			// Combine clientId and clientSecret into a Base64-encoded string
			string credentials = Base64Encode($"{clientId}:{clientSecret}");

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				var content = new StringContent($"grant_type=client_credentials&scope={scopes}", Encoding.UTF8, "application/x-www-form-urlencoded");

				var response = await client.PostAsync($"{BaseUrl}{TokenEndpoint}", content);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to authenticate: {response.ReasonPhrase}");
				}

				var jsonResponse = await response.Content.ReadAsStringAsync();
				var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

				string accessToken = tokenData.GetProperty("access_token").ToString();
				double expiresIn = Convert.ToDouble(tokenData.GetProperty("expires_in").ToString());

				return new TokenData(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));
			}
		}

		public async Task<List<ACCHub>> ExploreACCAsync(string accessToken)
		{
			List<ACCHub> hubs = new List<ACCHub>();

			try
			{
				// Get all hubs
				var hubItems = await GetHubsAsync(accessToken);
				foreach (var hubItem in hubItems)
				{
					List<ACCProject> projects = new List<ACCProject>();

					// Get projects in each hub
					var projectItems = await GetProjectsAsync(accessToken, hubItem["id"].ToString());
					foreach (var projectData in projectItems)
					{
						// Get the root folder of the project
						var rootFolderId = projectData["relationships"]["rootFolder"]["data"]["id"].ToString();

						ACCProject project = new ACCProject()
						{
							Id = projectData["id"].ToString(),
							Name = projectData["attributes"]["name"].ToString(),
							RootFolderId = rootFolderId
							//Folders = await ExploreFolderAsync(accessToken, projectData["id"].ToString(), rootFolderId, "\t\t")
						};

						projects.Add(project);
					}

					ACCHub hub = new ACCHub()
					{
						Id = hubItem["id"].ToString(),
						Name = hubItem["attributes"]["name"].ToString(),
						Projects = projects
					};

					hubs.Add(hub);
				}
			}
			catch (Exception ex)
			{

			}

			return hubs;
		}

		private async Task<JArray> GetHubsAsync(string accessToken)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync($"{BaseUrl}/project/v1/hubs");
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to get Hubs: {response.ReasonPhrase}");
				}

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);
				return (JArray)json["data"];
			}
		}

		private async Task<JArray> GetProjectsAsync(string accessToken, string hubId)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync($"{BaseUrl}/project/v1/hubs/{hubId}/projects"); if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to get Projects: {response.ReasonPhrase}");
				}

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);
				return (JArray)json["data"];
			}
		}

		public async Task<List<ACCFolder>> ExploreFolderAsync(string accessToken, string projectId, string folderId, string indent)
		{
			List<ACCFolder> folders = new List<ACCFolder>();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync($"{BaseUrl}/data/v1/projects/{projectId}/folders/{folderId}/contents"); 
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to get Folders: {response.ReasonPhrase}");
				}

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);

				foreach (var item in json["data"])
				{
					var type = item["type"].ToString();

					List<ACCFolder> children = new List<ACCFolder>();
					List<ACCFolder> files = new List<ACCFolder>();
					// If the item is a folder, recursively explore it
					if (type == "folders")
					{
						children = await ExploreFolderAsync(accessToken, projectId, item["id"].ToString(), indent + "\t");

						ACCFolder folder = new ACCFolder()
						{
							Id = item["id"].ToString(),
							Name = item["attributes"]["name"].ToString(),
							Type = item["type"].ToString(),
							SubFolders = children
						};

						folders.Add(folder);
					}
					// If the item is a file, print its details
					else if (type == "items")
					{
						// Optionally,, you can fetch additional details about the file
						files = await GetFileDetailAsync(accessToken, projectId, item["id"].ToString(), indent + "\t");

						foreach (var file in files)
						{
							folders.Add(file);
						}

					}

				}
			}

			return folders;
		}

		private async Task<List<ACCFolder>> GetFileDetailAsync(string accessToken, string projectId, string fileId, string indent)
		{
			List<ACCFolder> files = new List<ACCFolder>();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync($"{BaseUrl}/data/v1/projects/{projectId}/items/{fileId}");
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to get File Details: {response.ReasonPhrase}");
				}

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);

				var fileDetails = json["data"];

				ACCFolder file = new ACCFolder()
				{
					Id = fileDetails["id"].ToString(),
					Name = fileDetails["attributes"]["displayName"].ToString(),
					Type = fileDetails["type"].ToString(),
					CreateAt = fileDetails["attributes"]["createTime"].ToString(),
					LastModifiedAt = fileDetails["attributes"]["lastModifiedTime"].ToString()
				};

				files.Add(file);
			}

			return files;
		}

		public async Task<bool> IsFileLockedAsync(string accessToken, string projectId, string fileId)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var response = await client.GetAsync($"{BaseUrl}/data/v1/projects/{projectId}/items/{fileId}");
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to check whether the file is locked: {response.ReasonPhrase}");
				}

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);

				return json["data"]["attributes"]["hidden"]?.ToString() == "true";
			}
		}

		public async Task<string> UploadFileAsync(string accessToken, string projectId, string folderId, string filePath)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

				var fileName = System.IO.Path.GetFileName(filePath);
				var uploadUrl = $"{BaseUrl}/oss/v2/buckets/{projectId}/objects/{fileName}";

				var fileBytes = System.IO.File.ReadAllBytes(filePath);
				using (var content = new ByteArrayContent(fileBytes))
				{
					content.Headers.Add("Content-Type", "application/octet-stream");
					var response = await client.PostAsync(uploadUrl, content);

					if (response.IsSuccessStatusCode)
					{
						var res = await response.Content.ReadAsStringAsync();
						var json = JObject.Parse(res);

						return json["objectId"].ToString();
					}
				}

				Console.WriteLine($"File '{fileName}' uploaded successfully.");

				return "";
			}
		}
	}
}
