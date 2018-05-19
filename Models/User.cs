using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class User
    {
        public string gender { get; set; }
        public decimal age { get; set; }
        public string budget { get; set; }
        public Boolean married { get; set; }
        public Boolean kids { get; set; }
    }
}