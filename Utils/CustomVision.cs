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
            //var visionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/6d9b0926-3a0b-4f62-9405-327877242046/image?iterationId=9a5b88ec-8201-4dca-b1a2-82885f4ad15b";
            var visionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/97c679ea-74a3-4fef-a91e-d8c11c0e3224/image?iterationId=61faef74-4d73-46d9-9948-2bb8104a8a63";

            var req = new HttpRequestMessage(HttpMethod.Post, visionUrl);
            //req.Headers.Add("Prediction-Key", "601c086f64724b308f7c03aeb911d4d5");
            req.Headers.Add("Prediction-Key", "3b65c225ab224e898e836fb71b64413c");
            req.Content = new StreamContent(contentStream);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage resp = await httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var respStr = await resp.Content.ReadAsStringAsync();

            return JObject.Parse(respStr);
        }
    }
}