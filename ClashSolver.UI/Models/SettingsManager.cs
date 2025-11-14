using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ClashSolver.UI.Models
{
	public class SettingsModel
	{
		public List<LinkedModel> LinkedModels { get; set; } = new List<LinkedModel>();
		public List<Collaborator> Collaborators { get; set; } = new List<Collaborator>();
	}

	// TODO Why need SettingsManager class?

	public static class SettingsManager
	{
		private static readonly string SettingsFilePath = "settings.json";
		public static SettingsModel SettingsModel { get; set; } = LoadSettings();

		public static void SaveSettings()//SettingsModel settings)
		{
			string json = JsonSerializer.Serialize(SettingsModel, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(SettingsFilePath, json);
		}

		public static SettingsModel LoadSettings()
		{
			//if(File.Exists(SettingsFilePath)) {
			//	string json = File.ReadAllText(SettingsFilePath);
			//	return JsonSerializer.Deserialize<SettingsModel>(json);
			//}
			//	Return default settings if no file exists
			return new SettingsModel();
		}
	}
}
