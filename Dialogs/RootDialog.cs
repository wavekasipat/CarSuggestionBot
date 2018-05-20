using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Dialogs;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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