using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];

                if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        //// Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                        //if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                        //    && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                        //{
                        //    var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        //}

                        var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                        var contentStream = await responseMessage.Content.ReadAsStreamAsync();

                        var visionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/6d9b0926-3a0b-4f62-9405-327877242046/image?iterationId=9a5b88ec-8201-4dca-b1a2-82885f4ad15b";

                        var req = new HttpRequestMessage(HttpMethod.Post, visionUrl);
                        req.Headers.Add("Prediction-Key", "601c086f64724b308f7c03aeb911d4d5");
                        //req.Headers.Add("Content-Type", "application/octet-stream");
                        req.Content = new StreamContent(contentStream);

                        using (HttpResponseMessage resp = await httpClient.SendAsync(req))
                        {
                            resp.EnsureSuccessStatusCode();

                            var respStr = await resp.Content.ReadAsStringAsync();

                            await context.PostAsync($"{respStr}");

                            JObject json = JObject.Parse(respStr);

                            foreach (var prediction in json["predictions"])
                            {
                                var tag = prediction["tagName"];
                                var percent = decimal.Parse(prediction["probability"].ToString()) * 100;

                                await context.PostAsync($"{tag} {percent}%");
                            }

                        }

                    }
                }
                else
                {
                    await context.PostAsync($"What did you sent to me");
                }

            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }

    }
}