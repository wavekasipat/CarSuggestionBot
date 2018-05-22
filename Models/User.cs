using System;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class User
    {
        public string gender { get; set; }
        public string genderThai { get; set; }
        public decimal age { get; set; }
        public string budget { get; set; }
        public Boolean married { get; set; }
        public Boolean kids { get; set; }
        public string mobile { get; set; }
        public double smile { get; set; }
        public double anger { get; set; }
        public double happiness { get; set; }
        public Boolean eyeMakeup { get; set; }
        public Boolean lipMakeup { get; set; }
        public string glasses { get; set; }

        public string hair { get; set; }
        public double bald { get; set; }
        public string hairColor { get; set; }
        public double moustache { get; set; }
        public double beard { get; set; }
        public string emotion { get; set; }

        public string makeupStr { get; set; }
        public string smileStr { get; set; }
        public string angerStr { get; set; }

        public string BudgetOption1 { get; set; }
        public string BudgetOption2 { get; set; }
        public string BudgetOption3 { get; set; }
        public string BudgetOption4 { get; set; }
        public string BudgetOption5 { get; set; }

        public string suggestCar { get; set; }
        public string suggestUrl { get; set; }
        public string suggestImage { get; set; }

        public string suggestCar2 { get; set; }
        public string suggestUrl2 { get; set; }
        public string suggestImage2 { get; set; }

        public string suggestCar3 { get; set; }
        public string suggestUrl3 { get; set; }
        public string suggestImage3 { get; set; }

        public string sellInCar { get; set; }
        public decimal sellInOriginPrice { get; set; }
        public decimal sellInPrice { get; set; }
        public int sellInYear { get; set; }

        public User()
        {
            this.BudgetOption1 = "5แสน ถึง 6แสน";
            this.BudgetOption2 = "6แสน ถึง 8แสน";
            this.BudgetOption3 = "8แสน ถึง 1.2ล้าน";
            this.BudgetOption4 = "1.2ล้าน ถึง 1.5ล้าน";
            this.BudgetOption5 = "มากกว่า 1.5ล้าน";
        }
    }
}