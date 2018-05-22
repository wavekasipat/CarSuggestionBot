using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SimpleEchoBot.Models;
using SimpleEchoBot.Utils;
using System;
using System.Collections.Generic;
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
            await context.PostAsync($"Firstly, I want to see you. You can take your photo and send to Me.");
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

                        if (this.user.gender == "male")
                        {
                            this.user.genderThai = "ท่านหมื่น";
                        }
                        else
                        {
                            this.user.genderThai = "แม่หญิง";
                        }

                        List<string> options = new List<string>() { "Yes", "No" };
                        var quiz = $"Oh, I see you. You are a {this.user.gender} right?";
                        PromptDialog.Choice(context, this.OnGenderSelected, options, quiz, "Not a valid option", 3);
                    }
                    else
                    {
                        --attempts;
                        await context.PostAsync($"I can't see anyone in this photo, Please send me a photo.");
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

        private async Task OnGenderSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        break;

                    case "No":
                        if (this.user.gender == "male")
                        {
                            this.user.gender = "female";
                        }
                        else
                        {
                            this.user.gender = "male";
                        }
                        await context.PostAsync($"I'm sorry with that, i'll remember you are a {this.user.gender}.");
                        break;
                }

                List<string> options = new List<string>() {
                    this.user.BudgetOption1,
                    this.user.BudgetOption2,
                    this.user.BudgetOption3,
                    this.user.BudgetOption4,
                    this.user.BudgetOption5,
                };
                var quiz = $"I want to know your range of budget to buy a new car. (in thai baht)";
                PromptDialog.Choice(context, this.OnBudgetSelected, options, quiz, "Not a valid option", 3);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task OnBudgetSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.user.budget = await result;

                List<string> options = new List<string>() { "Yes", "No" };
                var quiz = $"Are you married?";
                PromptDialog.Choice(context, this.OnMarriedSelected, options, quiz, "Not a valid option", 3);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task OnMarriedSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        this.user.married = true;

                        List<string> options = new List<string>() { "Yes", "No" };
                        var quiz = $"Do you have a kid?";
                        PromptDialog.Choice(context, this.OnKidsSelected, options, quiz, "Not a valid option", 3);
                        break;

                    case "No":
                        this.user.married = false;
                        context.Done(this.user);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task OnKidsSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        this.user.kids = true;
                        break;

                    case "No":
                        this.user.kids = false;
                        break;
                }
                context.Done(this.user);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }
    }
}