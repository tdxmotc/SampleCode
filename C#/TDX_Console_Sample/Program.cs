using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TDX_Console_Sample
{
    internal static class Program
    {
        // ✅ 範例中統一集中管理端點與環境變數名稱，避免散落在程式各處。
        private const string TokenEndpoint = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
        private const string ApiEndpoint = "https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$select=StationName&$top=30&$format=JSON";
        private const string ClientIdEnvName = "TDX_CLIENT_ID";
        private const string ClientSecretEnvName = "TDX_CLIENT_SECRET";

        private static async Task Main(string[] args)
        {
            // ✅ Best Practice：不要在程式內硬編碼機密資料，改由環境變數取得。
            string clientId = GetRequiredEnvironmentVariable(ClientIdEnvName);
            string clientSecret = GetRequiredEnvironmentVariable(ClientSecretEnvName);

            // ✅ 啟用 Brotli/GZip 解壓縮，避免手動解壓縮與遺漏內容。
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip
            };

            // ✅ HttpClient 建議重用，本範例在主流程中建立並於結束時釋放。
            using var httpClient = new HttpClient(httpClientHandler)
            {
                // ✅ 避免無限等待，這裡設定整體請求逾時。
                Timeout = TimeSpan.FromSeconds(30)
            };

            // ✅ 設定 Accept Header，讓伺服器回傳 JSON。
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // ✅ 以取消權杖控制整體流程時間，避免網路異常造成無限等待。
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            Console.WriteLine("取得存取權杖 (Access Token)...");
            AccessToken token = await RequestAccessTokenAsync(httpClient, clientId, clientSecret, cts.Token);

            Console.WriteLine("呼叫 TDX API 取得資料...");
            string apiResponse = await GetLiveTrainDelayAsync(httpClient, token.AccessTokenValue, cts.Token);

            Console.WriteLine("\nAPI 回應內容:");
            Console.WriteLine(apiResponse);
        }

        // ✅ 取得 Access Token：採用 async/await 避免阻塞執行緒。
        private static async Task<AccessToken> RequestAccessTokenAsync(
            HttpClient httpClient,
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken)
        {
            // ✅ 用表單內容傳送授權資料。
            using var formData = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = formData
            };

            // ✅ 發送請求並確認狀態碼，失敗時直接丟出例外以利排查。
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Token 回應內容:");
            Console.WriteLine(responseJson);

            // ✅ JSON 反序列化並檢查結果，避免 NullReference。
            return JsonConvert.DeserializeObject<AccessToken>(responseJson)
                   ?? throw new InvalidOperationException("無法解析 Token 回應，請確認回應內容是否正確。");
        }

        // ✅ 呼叫 TDX API：在 request 中加入 Bearer Token。
        private static async Task<string> GetLiveTrainDelayAsync(
            HttpClient httpClient,
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        // ✅ 避免環境變數漏設造成難以追蹤的錯誤，直接在此處檢查。
        private static string GetRequiredEnvironmentVariable(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"找不到必要環境變數：{name}。請先設定後再執行。");
            }

            return value;
        }
    }

    // ✅ 對應 Access Token 回傳結構，使用 PascalCase 並透過 JsonProperty 對應欄位名稱。
    public sealed class AccessToken
    {
        [JsonProperty("access_token")]
        public string AccessTokenValue { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("not-before-policy")]
        public int NotBeforePolicy { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}
