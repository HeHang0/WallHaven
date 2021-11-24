using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WallHaven.WallHavenClient
{
    public class WallHavenRequest : IWallHavenRequest
    {
        private readonly Config wallHavenConfig;
        public WallHavenRequest(Config config)
        {
            wallHavenConfig = config;
        }

        public async Task<WallHavenResponse> GetWallpaper(string wallpaperId, string url = "", string token = "")
        {
            var responseJson = await GetAsync($"w/{wallpaperId}", url, token);
            var responseObject = !string.IsNullOrWhiteSpace(responseJson) ? JsonConvert.DeserializeObject<WallHavenResponse>(responseJson) : null;
            return responseObject;
        }

        public async Task<WallHavenResponse> Search(string searchParams, string url = "", string token = "")
        {
            var responseJson = await GetAsync($"search/{searchParams}", url, token);
            var responseObject = !string.IsNullOrWhiteSpace(responseJson) ? JsonConvert.DeserializeObject<WallHavenResponse>(responseJson) : null;
            return responseObject;
        }

        private Task<string> GetAsync(string searchParams, string url ="", string token = "")
        {
            if (string.IsNullOrWhiteSpace(url)) url = wallHavenConfig.BaseUrl;
            if (string.IsNullOrWhiteSpace(token)) token = wallHavenConfig.APIKey;
            return Task.Run(() =>
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.Replace("https", "http") + searchParams);
                    request.Method = "GET";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        request.Headers.Add("X-API-Key", token);
                    }
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36 Edg/95.0.1020.53";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    string retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();

                    return retString;
                }
                catch (Exception)
                {
                    return "";
                }
            });
        }
    }
}