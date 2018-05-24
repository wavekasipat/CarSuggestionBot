using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleEchoBot.Models;

namespace SimpleEchoBot.Utils
{
    public class DBUser
    {
        public static async void addUser(User user)
        {
            CarSuggestionBotSQLEntities db = new CarSuggestionBotSQLEntities();

            user newUser = new user();
            newUser.name = user.name;
            newUser.gender = user.gender;
            newUser.age = user.age;
            newUser.budget = user.budget;
            newUser.married = user.married;
            newUser.kids = user.kids;
            newUser.mobile = user.mobile;
            newUser.smile = user.smile;
            newUser.anger = user.anger;
            newUser.happiness = user.happiness;
            newUser.eyeMakeup = user.eyeMakeup;
            newUser.lipMakeup = user.lipMakeup;
            newUser.glasses = user.glasses;
            newUser.hair = user.hair;
            newUser.bald = user.bald;
            newUser.hairColor = user.hairColor;
            newUser.moustache = user.moustache;
            newUser.beard = user.beard;
            newUser.emotion = user.emotion;
            newUser.makeupStr = user.makeupStr;
            newUser.smileStr = user.smileStr;
            newUser.angerStr = user.angerStr;
            newUser.sellInCar = user.sellInCar;
            newUser.sellInOriginPrice = user.sellInOriginPrice;
            newUser.sellInPrice = user.sellInPrice;
            newUser.sellInYear = user.sellInYear;
            newUser.likedCar = user.likedCar;
            newUser.photoUrl = user.photoUrl;

            db.users.Add(newUser);

            db.SaveChanges();
        }
    }
}