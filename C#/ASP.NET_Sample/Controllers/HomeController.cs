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
            var response = await client.PostAsync(baseUrl, formData);

            var responseContent = await response.Content.ReadAsStringAsync();

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


        public async Task<string> Get(Dictionary<string, string> parameters, string requestUri, string token)
        {
            var client = _clientFactory.CreateClient();

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
            }
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");

            if (parameters.Any())
            {
                var strParam = string.Join("&", parameters.Where(d => !string.IsNullOrWhiteSpace(d.Value)).Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(requestUri, '?', strParam);
            }
            client.BaseAddress = new Uri(requestUri);

            var response = await client.GetAsync(requestUri).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync();

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
