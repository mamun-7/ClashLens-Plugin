using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeAPI.Models
{
	public class TranslationStatus
	{
		public string Status { get; set; }
		public string Progress { get; set; }
		public	 IEnumerable<string> Messages { get; set; }

		public TranslationStatus(string status, string progress, IEnumerable<string> messages)
		{
			Status = status;
			Progress = progress;
			Messages = messages;
		}
	}
}
