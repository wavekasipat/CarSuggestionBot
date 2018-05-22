using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleEchoBot.Utils
{
    public class FaceAPI
    {
        public static async Task<JArray> GetFaceAPIJson(Stream contentStream)
        {
            var faceUrl = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=age,gender,smile,glasses,emotion,hair,makeup,facialHair";

            var req = new HttpRequestMessage(HttpMethod.Post, faceUrl);
            req.Headers.Add("Ocp-Apim-Subscription-Key", "2f6d6ac45ccc4b9c9d5a3a9bd5b72699");
            req.Content = new StreamContent(contentStream);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage resp = await httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var respStr = await resp.Content.ReadAsStringAsync();

            return JArray.Parse(respStr);
        }
    }
}