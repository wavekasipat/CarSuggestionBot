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
            await context.PostAsync($"ข้าอยากเห็นหน้าออเจ้า ออเจ้าส่งรูปถ่ายใบหน้าของออเจ้ามาให้ข้าด้วยเถิดหนา");
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

                        List<string> options = new List<string>() { "ใช่", "ไม่" };
                        var quiz = $"ข้าเห็นหน้าออเจ้าแล้ว ออเจ้าเป็น{this.user.genderThai} ใช่หรือไม่";
                        PromptDialog.Choice(context, this.OnGenderSelected, options, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                    }
                    else
                    {
                        --attempts;
                        await context.PostAsync($"ออเจ้าไม่ได้ส่งรูปใบหน้าของออเจ้ามา ส่งรูปหน้าออเจ้ามาให้ข้าด้วยเถิด");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    --attempts;
                    await context.PostAsync($"ออเจ้าไม่ได้ส่งรูปใบหน้าของออเจ้ามา ส่งรูปหน้าออเจ้ามาให้ข้าด้วยเถิด");
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                --attempts;
                await context.PostAsync($"ส่งรูปหน้าออเจ้ามาให้ข้าด้วยเถิด");
                context.Wait(MessageReceivedAsync);
            }

            if (attempts <= 0)
            {
                context.Fail(new TooManyAttemptsException("รูปที่ออเจ้าส่งมาไม่ถูกต้อง"));
            }
        }

        private async Task OnGenderSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "ใช่":
                        break;

                    case "ไม่":
                        if (this.user.gender == "male")
                        {
                            this.user.gender = "female";
                            this.user.genderThai = "แม่หญิง";
                        }
                        else
                        {
                            this.user.gender = "male";
                            this.user.genderThai = "ท่านหมื่น";
                        }
                        await context.PostAsync($"ข้าเสียใจ ข้าจักจดจำว่าออเจ้าเป็น{this.user.genderThai}หนา");
                        break;
                }

                List<string> options = new List<string>() {
                    this.user.BudgetOption1,
                    this.user.BudgetOption2,
                    this.user.BudgetOption3,
                    this.user.BudgetOption4,
                    this.user.BudgetOption5,
                };
                var quiz = this.user.gender + $"ได้โปรดระบุงบที่ต้องการซื้อรถด้วยนะเจ้าคะ";
                PromptDialog.Choice(context, this.OnBudgetSelected, options, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException($"ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
            }
        }

        private async Task OnBudgetSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.user.budget = await result;

                List<string> options = new List<string>() { "ใช่", "ไม่" };
                var quiz = this.user.gender + $"ออเจ้าแต่งงานแล้วใช่หรือไม่เจ้าคะ";
                PromptDialog.Choice(context, this.OnMarriedSelected, options, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException($"ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
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

                        List<string> options = new List<string>() { "ใช่", "ไม่" };
                        var quiz = this.user.gender + $"ออเจ้ามีลูกหรือไม่เจ้าคะ";
                        PromptDialog.Choice(context, this.OnKidsSelected, options, quiz, "ออเจ้าเลือกไม่ถูกต้อง", 3);
                        break;

                    case "No":
                        this.user.married = false;
                        context.Done(this.user);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
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
                context.Fail(new TooManyAttemptsException("ข้าเสียใจยิ่ง ระบบขัดข้อง ลองเริ่มกันใหม่นะเจ้าคะ"));
            }
        }
    }
}