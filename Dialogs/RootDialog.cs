using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SimpleEchoBot.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        protected int count = 1;

        private string gender;
        private int age;
        private string budget;
        private Boolean married;
        private Boolean kids;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];

                if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
                {
                    var contentStream = await ImageStream.GetImageStream(attachment.ContentUrl);

                    JObject json = await CustomVision.GetCustomVisionJson(contentStream);

                    foreach (var prediction in json["predictions"])
                    {
                        var tag = prediction["tagName"];
                        var percent = decimal.Parse(prediction["probability"].ToString()) * 100;
                        var percentStr = percent.ToString("0.##");

                        await context.PostAsync($"{tag} {percentStr}%");
                    }
                }
                else
                {
                    await context.PostAsync($"I don't understand this file. Please send me a picture");
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