using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TDX_Console_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            AccessToken token = new AccessToken();

            using (HttpClient httpClient = new HttpClient())
            {

                HttpContent formData = new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "client_credentials"),
                            new KeyValuePair<string, string>("client_id", "XXXXXXXXXX-XXXXXXXX-XXXX-XXXX"),
                            new KeyValuePair<string, string>("client_secret", "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"),
                        }
                    );
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var response = httpClient.PostAsync("https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token", formData).Result;
                var responseStr = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Token:");
                Console.WriteLine(responseStr);

                token = JsonConvert.DeserializeObject<AccessToken>(responseStr);
            }

            Console.WriteLine("\n\r");

            Console.WriteLine("api資料:");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {token.access_token}");
                var apiResponse = client.GetStringAsync("https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON").Result;
                Console.WriteLine(apiResponse);
            }
        }
    }

    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string token_type { get; set; }
        public int notbeforepolicy { get; set; }
        public string scope { get; set; }
    }
}
