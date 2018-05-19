using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleEchoBot.Utils
{
    public class ImageStream
    {
        public static async Task<Stream> GetImageStream(string url)
        {
            HttpClient httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(url);

            return await responseMessage.Content.ReadAsStreamAsync();
        }
    }
}