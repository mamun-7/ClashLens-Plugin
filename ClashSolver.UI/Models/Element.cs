using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClashSolver.UI.Models
{
	public class CSElement : ClashElement
	{
		/// <summary>
		/// Id of Clash Detection Set
		/// </summary>
		public long Set { get; set; }

		/// <summary>
		/// The element id of scope box where the element is contained
		/// </summary>
		public long ScopeBox { get; set; }
	}

	public class ClashElement
	{
		/// <summary>
		/// Element Id
		/// </summary>
		[JsonPropertyName("id")]
		public long Id { get; set; }

		/// <summary>
		/// Name of element
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Discipline of element
		/// </summary>
		public string Discipline { get; set; }

		/// <summary>
		/// Category of element
		/// </summary>
		[JsonPropertyName("category_id")]
		public long CategoryId { get; set; }

		/// <summary>
		/// Category of element
		/// </summary>
		[JsonPropertyName("link_model_id")]
		public long LinkModelId { get; set; }

	}
}
