using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SimpleEchoBot.Models;
using SimpleEchoBot.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class FaceDialog : IDialog<User>
    {
        private User user = new User();
        private int attempts = 3;

        public FaceDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Firstly, I want to see your face. You can take your photo and send to Me.");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];

                if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
                {
                    var contentStream = await ImageStream.GetImageStream(attachment.ContentUrl);

                    JArray json = await FaceAPI.GetFaceAPIJson(contentStream);

                    if (json.Count > 0)
                    {
                        var face = json[0];
                        this.user.gender = face["faceAttributes"]["gender"].ToString();
                        this.user.age = decimal.Parse(face["faceAttributes"]["age"].ToString());
                        
                        await context.PostAsync($"Oh, I see you. You are a {this.user.gender}.");

                        context.Done(this.user);
                    }
                    else
                    {
                        --attempts;
                        await context.PostAsync($"I can't see anyone in this photo.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    --attempts;
                    await context.PostAsync($"I can't read this file, Please send me a photo.");
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                --attempts;
                await context.PostAsync($"Please send me a photo.");
                context.Wait(MessageReceivedAsync);
            }

            if (attempts <= 0)
            {
                context.Fail(new TooManyAttemptsException("Message was not a valid photo."));
            }
        }
    }
}