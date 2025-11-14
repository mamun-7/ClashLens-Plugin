using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAPI.Models
{
	public class ACCHub
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public List<ACCProject> Projects { get; set; }
	}

	public class ACCProject
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string RootFolderId { get; set; }	

		public List<ACCFolder> Folders { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(RootFolderId);
		}
	}

	public class ACCFolder
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string CreateAt { get; set; }

		public string LastModifiedAt { get; set; }

		public List<ACCFolder> SubFolders { get; set; }
	}
}
