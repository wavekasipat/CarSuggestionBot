using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleEchoBot.Models;
using SimpleEchoBot.Dialogs;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private User user = new User();

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            await this.SendWelcomeMessageAsync(context);

            //if (message.Attachments != null && message.Attachments.Count > 0)
            //{
            //    var attachment = message.Attachments[0];

            //    if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
            //    {
            //        var contentStream = await ImageStream.GetImageStream(attachment.ContentUrl);

            //        JObject json = await CustomVision.GetCustomVisionJson(contentStream);

            //        foreach (var prediction in json["predictions"])
            //        {
            //            var tag = prediction["tagName"];
            //            var percent = decimal.Parse(prediction["probability"].ToString()) * 100;
            //            var percentStr = percent.ToString("0.##");

            //            await context.PostAsync($"{tag} {percentStr}%");
            //        }
            //    }
            //    else
            //    {
            //        await context.PostAsync($"I don't understand this file. Please send me a picture");
            //    }
            //}
            //else
            //{
            //    await context.PostAsync($"{this.count++}: You said {message.Text}");
            //    context.Wait(MessageReceivedAsync);
            //}
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { "Yes", "No" }, "Hi, Do you want to know which car model will suit your life style? ", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case "Yes":
                        context.Call(new FaceDialog(this.user), this.FaceDialogResumeAfter);
                        break;

                    case "No":
                        await context.PostAsync($"Oh, I'm sorry to hear that. You can chat to me again anytime.");
                        context.Wait(this.MessageReceivedAsync);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task FaceDialogResumeAfter(IDialogContext context, IAwaitable<User> result)
        {
            try
            {
                this.user = await result;
                context.Call(new SuggestDialog(this.user), this.SuggestDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendWelcomeMessageAsync(context);
            }
        }

        private async Task SuggestDialogResumeAfter(IDialogContext context, IAwaitable<User> result)
        {
            try
            {
                this.user = await result;
                context.Wait(this.MessageReceivedAsync);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendWelcomeMessageAsync(context);
            }
        }
    }
}