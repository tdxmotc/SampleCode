using ASP.NET_Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string tokenUri = $"https://tdx.transportdata.tw";
        private readonly string apiUri = $"https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay";
        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            var accessToken = GetToken(tokenUri).Result;
            ViewBag.AccessToken = accessToken.access_token;

            var apiResponse = Get(GetParameters(), apiUri, accessToken.access_token).Result;
            ViewBag.ApiResponse = apiResponse;

            return View();
        }

        public async Task<AccessToken> GetToken(string requestUri)
        {
            string baseUrl = $"https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials"},
                { "client_id", "XXXXXXXXXX-XXXXXXXX-XXXX-XXXX" },
                { "client_secret", "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"}
            };

            var formData = new FormUrlEncodedContent(parameters);

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br,gzip");
            var response = await client.PostAsync(baseUrl, formData);
            
            var  responseContent = await DecompressResponse(response);

            return JsonConvert.DeserializeObject<AccessToken>(responseContent);
        }

        private Dictionary<string, string> GetParameters()
        {
            return new Dictionary<string, string>()
            {
                { $"$select","StationName" },
                { $"$filter",""},
                { $"$orderby",""},
                { $"$top","30"},
                { $"$skip",""},
                { $"health",""},
                { $"$format","JSON"},
            };
        }

        public async Task<string> DecompressResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Contains("br"))
            {
                using (var stream = new BrotliStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            else if(response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using (var stream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }                        
        }    


        public async Task<string> Get(Dictionary<string, string> parameters, string requestUri, string token)
        {
            var client = _clientFactory.CreateClient();

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "br,gzip");
            }
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");

            if (parameters.Any())
            {
                var strParam = string.Join("&", parameters.Where(d => !string.IsNullOrWhiteSpace(d.Value)).Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(requestUri, '?', strParam);
            }
            client.BaseAddress = new Uri(requestUri);

            var response = await client.GetAsync(requestUri).ConfigureAwait(false);

            var responseContent = await DecompressResponse(response);

            return responseContent;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
