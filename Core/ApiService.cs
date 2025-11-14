using Architexor.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Architexor.Core
{
	public static class ApiService
	{
		public static string PostSync(string uri, string data)
		{
			if (uri.StartsWith("https"))
			{
				ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // SecurityProtocolType.Tls;
				ServicePointManager.Expect100Continue = true;
			}

			byte[] dataBytes = Encoding.UTF8.GetBytes(data);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.CookieContainer = new CookieContainer();
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.ContentLength = dataBytes.Length;
			request.Method = "POST";
			request.ContentType = "application/json; charset=utf-8";

			if (Constants.thisUser.Token != null && Constants.thisUser.Token != "")
			{
				request.Headers.Add("Authorization", "Bearer " + Constants.thisUser.Token);
			}
			else
			{
				//	TODO : Use constant
				request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("Architexor"));
			}

			using (Stream requestBody = request.GetRequestStream())
			{
				requestBody.Write(dataBytes, 0, dataBytes.Length);
			}

			using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using Stream stream = response.GetResponseStream();
			using StreamReader reader = new(stream);
			return reader.ReadToEnd();
		}

		public static string GetResponse(string uri, string target = "")
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.CookieContainer = new CookieContainer();
			request.Method = "GET";
			request.ContentType = "application/json; charset=utf-8";

			if (Constants.thisUser.Token != "")
			{
				request.Headers.Add("Authorization", "Bearer " + Constants.thisUser.Token);
			}
			else
			{
				//	TODO : Use constant
				request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("Architexor"));
			}
			//request.PreAuthenticate = true;
			HttpWebResponse response = request.GetResponse() as HttpWebResponse;
			using Stream responseStream = response.GetResponseStream();
			StreamReader reader = new(responseStream, Encoding.UTF8);
			if (target == "")
				return reader.ReadToEnd();
			else
			{
				var fileStream = File.Create(target);
				reader.BaseStream.CopyTo(fileStream);
				fileStream.Close();
				return "";
			}
		}

		public static async Task<string> PostAsync(string uri, string data)
		{
			if(uri.StartsWith("https"))
			{
				ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;  //	TLS
				ServicePointManager.Expect100Continue = true;
			}

			using var client = new HttpClient(new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			});

			var request = new HttpRequestMessage(new HttpMethod("POST"), uri)
			{
				Content = new StringContent(data, Encoding.UTF8, "application/json")
			};

			if(!string.IsNullOrEmpty(Constants.thisUser.Token))
			{
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Constants.thisUser.Token);
			}
			else
			{
				string basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("Architexor"));
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuth);
			}

			using HttpResponseMessage response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode(); //	Optional: throws if not 200-299
			return await response.Content.ReadAsStringAsync();
		}

        public static async Task<string> GetAsync(string uri)
        {
            if (uri.StartsWith("https"))
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;  //	TLS
                ServicePointManager.Expect100Continue = true;
            }

            using var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var request = new HttpRequestMessage(new HttpMethod("GET"), uri)
            {
                //Content = new StringContent(data, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(Constants.thisUser.Token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Constants.thisUser.Token);
            }
            else
            {
                string basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("Architexor"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuth);
            }

            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode(); //	Optional: throws if not 200-299
            return await response.Content.ReadAsStringAsync();
        }
    }
}
