using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class SuggestDialog : IDialog<User>
    {
        private User user = new User();
        private int attempts = 3;

        public SuggestDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            SuggestCarCondition();

            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>()
            {
                new HeroCard
                {
                    Title = this.user.suggestCar,
                    Text = $"I suggest a {this.user.suggestCar} for you.",
                    Images = new List<CardImage> { new CardImage(this.user.suggestImage) },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Information", value: this.user.suggestUrl) }
                }.ToAttachment()
            };
            await context.PostAsync(message);

            List<string> options = new List<string>() { "Yes", "No" };
            var quiz = $"Can you provide your mobile number to me? I have a sales team that can offer a promotion about this car.";
            PromptDialog.Choice(context, this.OnMobileSelected, options, quiz, "Not a valid option", 3);

            //context.Done(this.user);
            //context.Wait(this.MessageReceivedAsync);
        }

        private async Task OnMobileSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        await context.PostAsync("What is your mobile number (e.g. '0891234567')?");
                        context.Wait(this.MessageReceivedAsync);
                        break;

                    case "No":
                        await context.PostAsync($"Oh, I'm sorry to hear that. You can chat to me again anytime.");
                        context.Done(this.user);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!");
                context.Wait(this.MessageReceivedAsync);
            }
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            
            if (message.Text != null && Regex.IsMatch(message.Text.Trim(), @"\+?[0-9]{10}"))
            {
                await context.PostAsync($"Thanks for your time. Our sales team will call you ASAP.");
                context.Done(this.user);
            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. What is your mobile number (e.g. '0891234567')?");

                    context.Wait(this.MessageReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a mobile number."));
                }
            }
        }

        private void SuggestCarCondition()
        {
            if (this.user.budget == this.user.BudgetOption1)
            {
                if (this.user.gender == "male")
                {
                    this.user.suggestCar = "Honda Brio Amaze";
                    this.user.suggestUrl = "https://www.honda.co.th/brioamaze-brio/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/2.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Brio";
                    this.user.suggestUrl = "https://www.honda.co.th/brio/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/1.png";
                }
            }
            else if (this.user.budget == this.user.BudgetOption2)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda Mobilio";
                    this.user.suggestUrl = "https://www.honda.co.th/th/mobilio/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/13.png";
                }
                else
                {
                    if (this.user.gender == "male")
                    {
                        this.user.suggestCar = "Honda City";
                        this.user.suggestUrl = "https://www.honda.co.th/th/city/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/5.png";
                    }
                    else
                    {
                        this.user.suggestCar = "Honda Jazz";
                        this.user.suggestUrl = "https://www.honda.co.th/th/jazz/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/3.png";
                    }
                }
            }
            else if (this.user.budget == this.user.BudgetOption3)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda BR-V";
                    this.user.suggestUrl = "https://www.honda.co.th/th/brv/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/18.png";
                }
                else
                {
                    if (this.user.gender == "male")
                    {
                        this.user.suggestCar = "Honda Civic";
                        this.user.suggestUrl = "https://www.honda.co.th/civic/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/7.png";
                    }
                    else
                    {
                        this.user.suggestCar = "Honda HR-V";
                        this.user.suggestUrl = "https://www.honda.co.th/th/hrv";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/11.png";
                    }
                }
            }
            else if (this.user.budget == this.user.BudgetOption4)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda Accord";
                    this.user.suggestUrl = "https://www.honda.co.th/th/accord/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/10.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Civic HATCHBACK";
                    this.user.suggestUrl = "https://www.honda.co.th/civichatchback/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/civic-hatchback-nav.png";
                }
            }
            else if (this.user.budget == this.user.BudgetOption5)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda CR-V";
                    this.user.suggestUrl = "http://www.honda.co.th/crv/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/12.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Accord Hybrid";
                    this.user.suggestUrl = "https://www.honda.co.th/th/accordhybrid/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/9.png";
                }
            }
        }
    }
}