using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SimpleEchoBot.Models;
using SimpleEchoBot.Utils;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class SuggestDialog : IDialog<User>
    {
        private User user = new User();
        private int modelAttempts = 3;
        private int yearAttempts = 3;
        private int mobileAttempts = 3;
        private int likeAttemps = 2;

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
            var quiz = $"Do you like this car?";
            PromptDialog.Choice(context, this.OnLikeSelected, options, quiz, "Not a valid option", 3);
        }

        private async Task OnLikeSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                List<string> options = new List<string>() { "Yes", "No" };
                var quiz = "";
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        quiz = $"Glad to hear That, we offer an attractive discounts on exchange or Sell-in of Used Cars. Do you want to available the service?";
                        PromptDialog.Choice(context, this.OnSellInSelected, options, quiz, "Not a valid option", 3);
                        break;

                    case "No":
                        --likeAttemps;
                        if (likeAttemps > 0)
                        {
                            var message = context.MakeMessage();
                            message.Attachments = new List<Attachment>()
                            {
                                new HeroCard
                                {
                                    Title = this.user.suggestCar,
                                    Text = $"I suggest a {this.user.suggestCar2} for you.",
                                    Images = new List<CardImage> { new CardImage(this.user.suggestImage2) },
                                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Information", value: this.user.suggestUrl2) }
                                }.ToAttachment()
                            };
                            await context.PostAsync(message);

                            quiz = $"How about this car?";
                            PromptDialog.Choice(context, this.OnLikeSelected, options, quiz, "Not a valid option", 3);
                        }
                        else
                        {
                            await context.PostAsync($"You can send me a picture of car that you like, And I'll suggest a car that similar to that car.");
                            context.Wait(CarPhotoReceivedAsync);
                        }
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task OnSellInSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                List<string> options = new List<string>() { "Yes", "No" };
                var quiz = "";
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        await context.PostAsync("Please Enter the Model and Series of car (e.g. 'Honda Jazz').");
                        context.Wait(this.ModelReceivedAsync);
                        break;

                    case "No":
                        quiz = $"Can you provide your mobile number to me? I have a sales team that can offer a promotion about this car.";
                        PromptDialog.Choice(context, this.OnMobileSelected, options, quiz, "Not a valid option", 3);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task ModelReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var found = false;

            if (message.Text != null)
            {
                var model = message.Text.ToLower();
                if (model.Contains("brio amaze") || model.Contains("brioamaze"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Brio Amaze";
                    this.user.sellInOriginPrice = 577000;
                }
                else if (model.Contains("brio"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Brio";
                    this.user.sellInOriginPrice = 495000;
                }
                else if (model.Contains("jazz"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Jazz";
                    this.user.sellInOriginPrice = 670000;
                }
                else if (model.Contains("city"))
                {
                    found = true;
                    this.user.sellInCar = "Honda City";
                    this.user.sellInOriginPrice = 670000;
                }
                else if (model.Contains("mobilio"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Mobilio";
                    this.user.sellInOriginPrice = 699999;
                }
                else if (model.Contains("civic hatchback") || model.Contains("civichatchback"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Civic HATCHBACK";
                    this.user.sellInOriginPrice = 1169000;
                }
                else if (model.Contains("civic"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Civic";
                    this.user.sellInOriginPrice = 980000;
                }
                else if (model.Contains("hr-v") || model.Contains("hrv"))
                {
                    found = true;
                    this.user.sellInCar = "Honda HR-V";
                    this.user.sellInOriginPrice = 1050000;
                }
                else if (model.Contains("br-v") || model.Contains("brv"))
                {
                    found = true;
                    this.user.sellInCar = "Honda BR-V";
                    this.user.sellInOriginPrice = 790000;
                }
                else if (model.Contains("cr-v") || model.Contains("crv"))
                {
                    found = true;
                    this.user.sellInCar = "Honda CR-V";
                    this.user.sellInOriginPrice = 1549000;
                }
                else if (model.Contains("accord hybrid") || model.Contains("accordhybrid"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Accord Hybrid";
                    this.user.sellInOriginPrice = 1750000;
                }
                else if (model.Contains("accord"))
                {
                    found = true;
                    this.user.sellInCar = "Honda Accord";
                    this.user.sellInOriginPrice = 1445000;
                }
            }

            if (found)
            {
                await context.PostAsync($"Please Enter The year of purchase.");
                context.Wait(this.YearReceivedAsync);
            }
            else
            {
                --modelAttempts;
                if (modelAttempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. What is your Honda car (e.g. 'Honda Jazz')?");

                    context.Wait(this.ModelReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a Honda car."));
                }
            }
        }

        private async Task YearReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var curYear = DateTime.Now.Year;
            var oldestYear = curYear - 18;

            if (message.Text != null && Regex.IsMatch(message.Text.Trim(), @"\+?[0-9]") && int.Parse(message.Text.Trim()) <= curYear && int.Parse(message.Text.Trim()) >= oldestYear)
            {
                this.user.sellInYear = int.Parse(message.Text.Trim());
                var usedYears = curYear - this.user.sellInYear;
                var originPrice = this.user.sellInOriginPrice;
                this.user.sellInPrice = originPrice * (1 - ((27 + 4 * usedYears) / (decimal)100));

                var sellInPriceStr = this.user.sellInPrice.ToString("#,##0.00");

                List<string> options = new List<string>() { "Yes", "No" };
                var quiz = $"The best price for this car is {sellInPriceStr} B. Subjected to go up or down based on further details. Do you want to include this in your offer?";
                PromptDialog.Choice(context, this.OnSellInConfirmSelected, options, quiz, "Not a valid option", 3);
            }
            else
            {
                --yearAttempts;
                if (yearAttempts > 0)
                {
                    await context.PostAsync($"I'm sorry, I don't understand your reply. Please Enter The year of purchase (from {oldestYear} to {curYear}).");

                    context.Wait(this.YearReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a mobile number."));
                }
            }
        }

        private async Task OnSellInConfirmSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                List<string> options = new List<string>() { "Yes", "No" };
                var quiz = "";
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Yes":
                        await context.PostAsync("Please provide your mobile number to me, I have a sales team that can offer a promotion about this car.");
                        context.Wait(this.MobileReceivedAsync);
                        break;

                    case "No":
                        quiz = $"Can you provide your mobile number to me? I have a sales team that can offer a promotion about this car.";
                        PromptDialog.Choice(context, this.OnMobileSelected, options, quiz, "Not a valid option", 3);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
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
                        context.Wait(this.MobileReceivedAsync);
                        break;

                    case "No":
                        await context.PostAsync($"Oh, I'm sorry to hear that. You can chat to me again anytime.");
                        context.Done(this.user);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!"));
            }
        }

        private async Task MobileReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text != null && Regex.IsMatch(message.Text.Trim(), @"\+?[0-9]{10}"))
            {
                await context.PostAsync($"Thanks for your time. Our sales team will call you ASAP.");
                context.Done(this.user);
            }
            else
            {
                --mobileAttempts;
                if (mobileAttempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. What is your mobile number (e.g. '0891234567')?");

                    context.Wait(this.MobileReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a mobile number."));
                }
            }
        }

        public async Task CarPhotoReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];

                if (attachment.ContentType == "image/png" || attachment.ContentType == "image/jpeg")
                {
                    var contentStream = await ImageStream.GetImageStream(attachment.ContentUrl);

                    JObject json = await CustomVision.GetCustomVisionJson(contentStream);

                    // check is car
                    Boolean isCar = false;
                    foreach (var prediction in json["predictions"])
                    {
                        //var tag = prediction["tagName"];
                        //var percent = decimal.Parse(prediction["probability"].ToString()) * 100;
                        //var percentStr = percent.ToString("0.##");
                        //await context.PostAsync($"{tag} {percentStr}%");

                        var tag = prediction["tagName"].ToString();
                        var percent = decimal.Parse(prediction["probability"].ToString()) * 100;
                        if (tag == "car" && percent >= 50)
                        {
                            isCar = true;
                            break;
                        }
                    }

                    if (isCar)
                    {
                        foreach (var prediction in json["predictions"])
                        {
                            var tag = prediction["tagName"].ToString();
                            if (tag == "car")
                            {
                                continue;
                            }
                            else
                            {
                                SuggestCarConditionFromModel(tag);
                                var outMessage = context.MakeMessage();
                                outMessage.Attachments = new List<Attachment>()
                                {
                                    new HeroCard
                                    {
                                        Title = this.user.suggestCar,
                                        Text = $"This look like a {this.user.suggestCar3}.",
                                        Images = new List<CardImage> { new CardImage(this.user.suggestImage3) },
                                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Information", value: this.user.suggestUrl3) }
                                    }.ToAttachment()
                                };
                                await context.PostAsync(outMessage);

                                List<string> options = new List<string>() { "Yes", "No" };
                                var quiz = $"Glad to hear That, we offer an attractive discounts on exchange or Sell-in of Used Cars. Do you want to available the service?";
                                PromptDialog.Choice(context, this.OnSellInSelected, options, quiz, "Not a valid option", 3);

                                break;
                            }
                        }
                    }
                    else
                    {
                        await context.PostAsync($"I think this is not a car. Please send me a car photo.");
                        context.Wait(CarPhotoReceivedAsync);
                    }
                }
                else
                {
                    await context.PostAsync($"I don't understand this file. Please send me a car photo.");
                    context.Wait(CarPhotoReceivedAsync);
                }
            }
            else
            {
                await context.PostAsync($"I didn't see any photo.");
                context.Wait(CarPhotoReceivedAsync);
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
                    this.user.suggestCar2 = "Honda Brio";
                    this.user.suggestUrl2 = "https://www.honda.co.th/brio/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/1.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Brio";
                    this.user.suggestUrl = "https://www.honda.co.th/brio/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/1.png";
                    this.user.suggestCar2 = "Honda Brio Amaze";
                    this.user.suggestUrl2 = "https://www.honda.co.th/brioamaze-brio/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/2.png";
                }
            }
            else if (this.user.budget == this.user.BudgetOption2)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda Mobilio";
                    this.user.suggestUrl = "https://www.honda.co.th/th/mobilio/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/13.png";
                    this.user.suggestCar2 = "Honda Jazz";
                    this.user.suggestUrl2 = "https://www.honda.co.th/th/jazz/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/3.png";
                }
                else
                {
                    if (this.user.gender == "male")
                    {
                        this.user.suggestCar = "Honda City";
                        this.user.suggestUrl = "https://www.honda.co.th/th/city/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/5.png";
                        this.user.suggestCar2 = "Honda Jazz";
                        this.user.suggestUrl2 = "https://www.honda.co.th/th/jazz/";
                        this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/3.png";
                    }
                    else
                    {
                        this.user.suggestCar = "Honda Jazz";
                        this.user.suggestUrl = "https://www.honda.co.th/th/jazz/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/3.png";
                        this.user.suggestCar2 = "Honda City";
                        this.user.suggestUrl2 = "https://www.honda.co.th/th/city/";
                        this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/5.png";
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
                    this.user.suggestCar2 = "Honda HR-V";
                    this.user.suggestUrl2 = "https://www.honda.co.th/th/hrv";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/11.png";
                }
                else
                {
                    if (this.user.gender == "male")
                    {
                        this.user.suggestCar = "Honda Civic";
                        this.user.suggestUrl = "https://www.honda.co.th/civic/";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/7.png";
                        this.user.suggestCar2 = "Honda HR-V";
                        this.user.suggestUrl2 = "https://www.honda.co.th/th/hrv";
                        this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/11.png";
                    }
                    else
                    {
                        this.user.suggestCar = "Honda HR-V";
                        this.user.suggestUrl = "https://www.honda.co.th/th/hrv";
                        this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/11.png";
                        this.user.suggestCar2 = "Honda Civic";
                        this.user.suggestUrl2 = "https://www.honda.co.th/civic/";
                        this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/7.png";
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
                    this.user.suggestCar2 = "Honda Civic HATCHBACK";
                    this.user.suggestUrl2 = "https://www.honda.co.th/civichatchback/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/civic-hatchback-nav.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Civic HATCHBACK";
                    this.user.suggestUrl = "https://www.honda.co.th/civichatchback/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/civic-hatchback-nav.png";
                    this.user.suggestCar2 = "Honda Accord";
                    this.user.suggestUrl2 = "https://www.honda.co.th/th/accord/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/10.png";
                }
            }
            else if (this.user.budget == this.user.BudgetOption5)
            {
                if (this.user.married)
                {
                    this.user.suggestCar = "Honda CR-V";
                    this.user.suggestUrl = "http://www.honda.co.th/crv/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/12.png";
                    this.user.suggestCar2 = "Honda Accord Hybrid";
                    this.user.suggestUrl2 = "https://www.honda.co.th/th/accordhybrid/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/9.png";
                }
                else
                {
                    this.user.suggestCar = "Honda Accord Hybrid";
                    this.user.suggestUrl = "https://www.honda.co.th/th/accordhybrid/";
                    this.user.suggestImage = "https://www.honda.co.th/assets/template/assets/images/share/nav/9.png";
                    this.user.suggestCar2 = "Honda CR-V";
                    this.user.suggestUrl2 = "http://www.honda.co.th/crv/";
                    this.user.suggestImage2 = "https://www.honda.co.th/assets/template/assets/images/share/nav/12.png";
                }
            }
        }

        private void SuggestCarConditionFromModel(string model)
        {
            switch (model)
            {
                case "BR-V":
                    this.user.suggestCar3 = "Honda BR-V";
                    this.user.suggestUrl3 = "https://www.honda.co.th/th/brv/";
                    this.user.suggestImage3 = "https://www.honda.co.th/assets/template/assets/images/share/nav/18.png";
                    break;

                case "CITY":
                    this.user.suggestCar3 = "Honda City";
                    this.user.suggestUrl3 = "https://www.honda.co.th/th/city/";
                    this.user.suggestImage3 = "https://www.honda.co.th/assets/template/assets/images/share/nav/5.png";
                    break;

                case "CIVIC":
                    this.user.suggestCar3 = "Honda Civic";
                    this.user.suggestUrl3 = "https://www.honda.co.th/civic/";
                    this.user.suggestImage3 = "https://www.honda.co.th/assets/template/assets/images/share/nav/7.png";
                    break;

                case "CR-V":
                    this.user.suggestCar3 = "Honda CR-V";
                    this.user.suggestUrl3 = "http://www.honda.co.th/crv/";
                    this.user.suggestImage3 = "https://www.honda.co.th/assets/template/assets/images/share/nav/12.png";
                    break;

                case "HR-V":
                    this.user.suggestCar3 = "Honda HR-V";
                    this.user.suggestUrl3 = "https://www.honda.co.th/th/hrv";
                    this.user.suggestImage3 = "https://www.honda.co.th/assets/template/assets/images/share/nav/11.png";
                    break;
            }
        }
    }
}