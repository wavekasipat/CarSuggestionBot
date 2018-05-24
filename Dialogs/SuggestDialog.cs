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
        private int likeAttemps = 2;
        private int modelAttempts = 3;
        private int yearAttempts = 3;
        private int mobileAttempts = 3;
        private int nameAttempts = 3;

        List<string> yesNoOptions = new List<string>() { "ใช่", "ไม่" };

        public SuggestDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            SuggestCarCondition();

            this.user.likedCar = this.user.suggestCar;
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>()
            {
                new HeroCard
                {
                    Title = this.user.suggestCar,
                    //Text = $"I suggest a {this.user.suggestCar} for you.",
                    Text = $"จากข้อมูลของ{this.user.genderThai} และรูปถ่ายที่ได้ ออเจ้าเป็น {this.user.genderThai} อายุ {this.user.age} ปี {this.user.makeupStr} {this.user.smileStr} {this.user.angerStr} รถยนต์ที่เหมาะกับออเจ้าที่สุดคือ {this.user.suggestCar}",
                    Images = new List<CardImage> { new CardImage(this.user.suggestImage) },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ข้อมูล", value: this.user.suggestUrl) }
                }.ToAttachment()
            };
            await context.PostAsync(message);
            
            //var quiz = $"Do you like this car?";
            var quiz = $"{this.user.genderThai} ชอบรถคันนี้หรือไม่เจ้าคะ (หากไม่ ข้าจักแนะนำรถที่ใกล้เคียงแก่{this.user.genderThai}หนา)";
            PromptDialog.Choice(context, this.OnLikeSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
        }

        private async Task OnLikeSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var quiz = "";
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "ใช่":
                        //quiz = $"Glad to hear That, we offer an attractive discounts on exchange or Sell-in of Used Cars. Do you want to available the service?";
                        quiz = $"ข้ายินดียิ่งนักที่{this.user.genderThai}สนใจรถของข้า ข้ามีโปรโมชั่นพิเศษ รถเก่าแลกรถใหม่ {this.user.genderThai}สนใจข้อเสนอพิเศษนี้หรือไม่";
                        PromptDialog.Choice(context, this.OnSellInSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                        break;

                    case "ไม่":
                        --likeAttemps;
                        if (likeAttemps > 0)
                        {
                            this.user.likedCar = this.user.suggestCar2;
                            var message = context.MakeMessage();
                            message.Attachments = new List<Attachment>()
                            {
                                new HeroCard
                                {
                                    Title = this.user.suggestCar2,
                                    Text = $"คันนี้ล่ะเจ้าคะ",
                                    Images = new List<CardImage> { new CardImage(this.user.suggestImage2) },
                                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ข้อมูล", value: this.user.suggestUrl2) }
                                }.ToAttachment()
                            };
                            await context.PostAsync(message);

                            quiz = $"{this.user.genderThai}ชอบรถคันนี้หรือไม่เจ้าคะ";
                            PromptDialog.Choice(context, this.OnLikeSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                        }
                        else
                        {
                            await context.PostAsync($"หาก{this.user.genderThai}มีรถคันโปรด ได้โปรดนำรูปรถส่งมาให้ข้าเถิดเจ้าค่ะ");
                            context.Wait(CarPhotoReceivedAsync);
                        }
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
            }
        }

        private async Task OnSellInSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "ใช่":
                        await context.PostAsync($"ระบุรุ่นรถของ{this.user.genderThai}ด้วยเถิดหนา (เช่น Honda Jazz เฉพาะรถของ Honda)");
                        context.Wait(this.ModelReceivedAsync);
                        break;

                    case "ไม่":
                        await context.PostAsync($"ข้าขอเบอร์โทรติดต่อของ{this.user.genderThai}ด้วยนะเจ้าคะ ข้าจักให้คนของข้าติดต่อกลับไป พร้อมกับข้อเสนอที่น่าสนใจโดยเร็ว (เช่น ‘0891234567’)");
                        context.Wait(this.MobileReceivedAsync);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
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
                await context.PostAsync($"ระบุปีที่ซื้อรถคันเก่าของ{this.user.genderThai}ด้วยเถิด");
                context.Wait(this.YearReceivedAsync);
            }
            else
            {
                --modelAttempts;
                if (modelAttempts > 0)
                {
                    await context.PostAsync($"ข้าไม่เข้าใจ โปรดระบุรุ่นรถของ{this.user.genderThai}ด้วยเถิดหนา (เช่น Honda Jazz เฉพาะรถของ Honda)");
                    context.Wait(this.ModelReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("ข้อความที่ออเจ้าส่งมาไม่ใช่รถของ Honda นะเจ้าคะ"));
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
                
                var quiz = $"ราคาที่เหมาะสมกับรถของ{this.user.genderThai} คือ {sellInPriceStr} บาท ราคานี้สามารถเพิ่มหรือลดลงตามข้อมูลที่ได้จาก{this.user.genderThai}เพิ่มเติม ออเจ้าสนใจข้อเสนอนี้หรือไม่ ";
                PromptDialog.Choice(context, this.OnSellInConfirmSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
            }
            else
            {
                --yearAttempts;
                if (yearAttempts > 0)
                {
                    await context.PostAsync($"ข้าไม่เข้าใจ โปรดระบุปีที่ซื้อรถคันเก่าของ{this.user.genderThai}ด้วยเถิด (ตั้งแต่ {oldestYear} ถึง {curYear}).");

                    context.Wait(this.YearReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("ข้อความที่ออเจ้าส่งมาไม่ใช่ปีที่ถูกต้องนะเจ้าคะ"));
                }
            }
        }

        private async Task OnSellInConfirmSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                //switch (optionSelected)
                //{
                //    case "ใช่":
                //        await context.PostAsync($"ข้าขอเบอร์โทรติดต่อของ{this.user.genderThai}ด้วยนะเจ้าคะ ข้าจักให้คนของข้าติดต่อกลับไป พร้อมกับข้อเสนอที่น่าสนใจโดยเร็ว (เช่น ‘0891234567’)");
                //        context.Wait(this.MobileReceivedAsync);
                //        break;

                //    case "ไม่":
                //        quiz = $"ข้าขอเบอร์โทรติดต่อของ{this.user.genderThai}ได้หรือไม่ ข้าจักให้คนของข้าติดต่อกลับไป พร้อมกับข้อเสนอที่น่าสนใจโดยเร็ว";
                //        PromptDialog.Choice(context, this.OnMobileSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                //        break;
                //}

                await context.PostAsync($"ข้าขอเบอร์โทรติดต่อของ{this.user.genderThai}ด้วยนะเจ้าคะ ข้าจักให้คนของข้าติดต่อกลับไป พร้อมกับข้อเสนอที่น่าสนใจโดยเร็ว (เช่น ‘0891234567’)");
                context.Wait(this.MobileReceivedAsync);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
            }
        }

        //private async Task OnMobileSelected(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {
        //        string optionSelected = await result;
        //        switch (optionSelected)
        //        {
        //            case "ใช่":
        //                await context.PostAsync($"เบอร์โทรของ{this.user.genderThai} เบอร์อะไรเจ้าคะ (เช่น '0891234567')?");
        //                context.Wait(this.MobileReceivedAsync);
        //                break;

        //            case "ไม่":
        //                await context.PostAsync($"ข้าเสียใจยิ่ง แต่ถึงอย่างไรออเจ้าคุยกับข้าได้ทุกเมื่อหนา");
        //                context.Done(this.user);
        //                break;
        //        }
        //    }
        //    catch (TooManyAttemptsException ex)
        //    {
        //        context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
        //    }
        //}

        private async Task MobileReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text != null && Regex.IsMatch(message.Text.Trim(), @"\+?[0-9]{10}"))
            {
                this.user.mobile = message.Text.Trim();
                await context.PostAsync($"ข้าขอชื่อของ{this.user.genderThai}ด้วยเถิดหนา");
                context.Wait(this.NameReceivedAsync);
            }
            else
            {
                --mobileAttempts;
                if (mobileAttempts > 0)
                {
                    await context.PostAsync($"ข้าไม่เข้าใจ เบอร์โทรของ{this.user.genderThai} เบอร์อะไรเจ้าคะ (เช่น '0891234567')");
                    context.Wait(this.MobileReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("ข้อความที่ออเจ้าส่งมาไม่ใช่เบอร์โทรที่ถูกต้องนะเจ้าคะ"));
                }
            }
        }

        private async Task NameReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text != null)
            {
                this.user.name = message.Text.Trim();
                await context.PostAsync($"ขอบน้ำใจ{this.user.genderThai} ที่สละเวลาให้กับข้า {this.user.genderThai}สามารถติดต่อข้าได้ทุกเวลาหนา");

                var str = "เฉลยนะออเจ้า จากรูปถ่ายใบหน้าของออเจ้า ข้าสามารถรู้ข้อมูลออเจ้าดังนี้";
                str += $"\n1.สีผม : {this.user.hairColor}";
                str += $"\n2.ยิ้ม : {this.user.smile}";
                str += $"\n3.เพศ : {this.user.gender}";
                str += $"\n4.อายุ : {this.user.age}";
                str += $"\n5.หนวด : {this.user.moustache}, เครา : {this.user.beard}";
                str += $"\n6.ใส่แว่น : {this.user.glasses}";
                str += $"\n7.แต่งตา : {this.user.eyeMakeup}, ทาปาก : {this.user.lipMakeup}";
                str += $"\n8.อารมณ์ : {this.user.emotion.Replace("\"", "")}";
                await context.PostAsync(str);

                await context.PostAsync($"หาก{this.user.genderThai}สนใจบริการ Commercial Chat Bot ของ MSC กรุณาติดต่อได้ที่ มีลาภ โสขุมา meelasok@metrosystems.co.th Mobile:(+668) 19095487");
                context.Done(this.user);
            }
            else
            {
                --nameAttempts;
                if (nameAttempts > 0)
                {
                    await context.PostAsync($"ข้าไม่เข้าใจ {this.user.genderThai}ชื่ออะไรนะเจ้าคะ");
                    context.Wait(this.NameReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("ข้อความที่ออเจ้าส่งมาไม่ใช่ชื่อที่ถูกต้องนะเจ้าคะ"));
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

                                this.user.likedCar = this.user.suggestCar3;
                                var outMessage = context.MakeMessage();
                                outMessage.Attachments = new List<Attachment>()
                                {
                                    new HeroCard
                                    {
                                        Title = this.user.suggestCar3,
                                        Text = $"จากรูปรถของ{this.user.genderThai} รถยนต์ที่เหมาะสมและใกล้เคียงกับความต้องการของ{this.user.genderThai} คือ {this.user.suggestCar3}",
                                        Images = new List<CardImage> { new CardImage(this.user.suggestImage3) },
                                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ข้อมูล", value: this.user.suggestUrl3) }
                                    }.ToAttachment()
                                };
                                await context.PostAsync(outMessage);
                                
                                var quiz = $"{this.user.genderThai}ชอบรถยนต์คันนี้หรือไม่";
                                PromptDialog.Choice(context, this.OnLikeVisionCarSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);

                                break;
                            }
                        }
                    }
                    else
                    {
                        await context.PostAsync($"ข้าว่านี่ไม่ใช่รถนะเจ้าคะ โปรดส่งรูปรถให้ข้าเถิดเจ้าค่ะ");
                        context.Wait(CarPhotoReceivedAsync);
                    }
                }
                else
                {
                    await context.PostAsync($"ข้าว่านี่ไม่ใช่รูปนะเจ้าคะ โปรดส่งรูปรถให้ข้าเถิดเจ้าค่ะ");
                    context.Wait(CarPhotoReceivedAsync);
                }
            }
            else
            {
                await context.PostAsync($"ข้าไม่เห็นรูปอะไรเลยเจ้าค่ะ");
                context.Wait(CarPhotoReceivedAsync);
            }
        }

        private async Task OnLikeVisionCarSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var quiz = "";
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "ใช่":
                        quiz = $"ข้ายินดียิ่งนักที่{this.user.genderThai}สนใจรถของข้า ข้ามีโปรโมชั่นพิเศษ รถเก่าแลกรถใหม่ {this.user.genderThai}สนใจข้อเสนอพิเศษนี้หรือไม่";
                        PromptDialog.Choice(context, this.OnSellInSelected, yesNoOptions, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                        break;

                    case "ไม่":
                        await context.PostAsync($"หาก{this.user.genderThai}มีรถคันโปรดอีก ได้โปรดนำรูปรถส่งมาให้ข้าเถิดเจ้าค่ะ");
                        context.Wait(CarPhotoReceivedAsync);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
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
