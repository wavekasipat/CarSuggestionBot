using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleEchoBot.Utils
{
    public class CustomVision
    {
        public static async Task<JObject> GetCustomVisionJson(Stream contentStream)
        {
            var visionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/6d9b0926-3a0b-4f62-9405-327877242046/image?iterationId=9a5b88ec-8201-4dca-b1a2-82885f4ad15b";

            var req = new HttpRequestMessage(HttpMethod.Post, visionUrl);
            req.Headers.Add("Prediction-Key", "601c086f64724b308f7c03aeb911d4d5");
            req.Content = new StreamContent(contentStream);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage resp = await httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var respStr = await resp.Content.ReadAsStringAsync();

            return JObject.Parse(respStr);
        }
    }
}