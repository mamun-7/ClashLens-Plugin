using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Architexor.Core;
using ClashSolver.UI;
using ClashSolver.UI.Models;

namespace ClashSolver.Utils
{
	public class ClashResolutionItem
	{
		public ResolveType Type { get; set; }
		public string Target { get; set; }  //	Element ID
		public string Description { get; set; } //	Explanation of the action
		public Dictionary<string, object> Parameters { get; set; } = new();

		public override string ToString()
		{
			return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
		}
	}

	public class ChatGPTService
	{
		private static readonly string _apiKey = "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";
		private readonly HttpClient _httpClient;
		private string _systemPrompt = "";

		public ChatGPTService()
		{
			_systemPrompt = @"You are an BIM expert assistant helping to resolve clashes in a Revit plugin.
			Suggest some suggestions to resolve the clash based on the user prompt. The response should be recognizable by the Revit plugin.
			The response is the array of resolutions.
			One resolution should have 4 properties.
			'description': string - description of the resolution
			'target': integer - target element id
			'action': string - like Move, Reroute ...
			'parameter': object - additional parameters based on 'action' property.
				Move => { 'offset': vector }";

			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://api.openai.com/v1/")
			};
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
		}

		public async Task<ChatGPTResponse> GetClashResolutionAsync(string userInput)
		{
			var requestPayload = new
			{
				model = "gpt-4-0613",
				messages = new[]
				{
					new { role = "system", content = _systemPrompt },
					new { role = "user", content = userInput }
				},
			};

			try
			{
				var response = await _httpClient.PostAsync(
					"chat/completions",
					new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json")
				);

				var responseString = await response.Content.ReadAsStringAsync();

				response.EnsureSuccessStatusCode();

				var responseObject = JsonSerializer.Deserialize<ChatGPTResponse>(responseString);

				return responseObject;
			}
			catch(Exception ex)
			{
				TraceLogger.Instance.ExceptionLog("ChatGPTService::GetClashResolutionAsync => ", ex);
				return null;
			}
		}

		private static int EstimateTokenCount(List<ChatMessage> messages)
		{
			int tokenCount = 0;
			foreach (var msg in messages)
			{
				//	Simple estimation: 1 token per 4 characters
				tokenCount += msg.content.Length / 4;
			}
			return tokenCount;
		}
	}

	//	Strongly-typed classes for deserialization
	public class ChatGPTResponse
	{
		public List<Choice> choices { get; set; }
	}

	public class Choice
	{
		public int index { get; set; }

		public ChatMessage message { get; set; }
	}

	public class ChatMessage
	{
		public string role { get; set; }
		public string content { get; set; }
	}

	public class ChatContent
	{
		public string priority { get; set; }

		public string description { get; set; }

		public long target { get; set; }

		public string action { get; set; }

		public ChatContentParam parameter { get; set; }
	}

	public class ChatContentParam
	{
		public XYZParam offset { get; set; }
	}

	public class XYZParam
	{
		public double x { get; set; }

		public double y { get; set; }

		public double z { get; set; }
	}
}
