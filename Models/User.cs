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
            this.BudgetOption1 = "500k - 600k";
            this.BudgetOption2 = "600k - 800k";
            this.BudgetOption3 = "800k - 1.2m";
            this.BudgetOption4 = "1.2m - 1.5m";
            this.BudgetOption5 = "More than 1.5m";
        }
    }
}