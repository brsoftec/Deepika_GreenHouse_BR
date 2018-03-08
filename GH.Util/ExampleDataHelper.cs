using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Util
{
    public static class ExampleDataHelper
    {

       
        private static List<string> GetListRandom(List<string> list ,int count) {
            List<string> listrandom = new List<string>();
            string value = "";
            do
            {
                int intradom = 0;
                do
                {
                     intradom = new Random().Next(0, list.Count );
                }
                while (listrandom.Contains(list[intradom]));
               
                value = list[intradom];
                listrandom.Add(value);
            }
            while ( listrandom.Count< count);

            return listrandom;
        }
        public static List<object> GetRandomObjectFromList(int countrandom, List<object> list)
        {

            List<int> listindex = new List<int>();
            List<object> listreturn = new List<object>();
            do
            {
                int intradom = 0;
                do
                {
                    intradom = new Random().Next(0, list.Count);
                }
                while (listindex.Contains(intradom));

                listindex.Add(intradom);
                listreturn.Add(list[intradom]);

            }
            while (listreturn.Count < countrandom);

            return listreturn;
        }
        public static List<object> GetRandomFieldsforcampaign(int countrandom, List<object> list)
        {

            List<string> listindex= new List<string>();
            List<object> listreturn = new List<object>();
            do
            {
                int intradom = 0;
                do
                {
                    intradom = new Random().Next(0, list.Count - 1);
                }
                while (listindex.Contains(intradom.ToString()));


                listreturn.Add(list[intradom]);
            }
            while (listreturn.Count < countrandom);

            return listreturn;
        }
        public static string GetRandomImageForCampaign()
        {

            var imagespaths = new string[] { "/Content/UploadImages/04e12376-ae2d-43c0-ac1c-72dd4e8197b1Campaign20161011172749.PNG",
                "/Content/UploadImages/04e12376-ae2d-43c0-ac1c-72dd4e8197b1Campaign20161011172653.jpg", "/Content/UploadImages/04e12376-ae2d-43c0-ac1c-72dd4e8197b1Campaign20161011172457.jpg" };
            return imagespaths.ToList()[new Random().Next(0, 3)];
        }

        public static string GetRandomLocation() {

            var locations = new string[] { "Sai Gon", "Quang Ngai", "Ha Noi" };
            return locations.ToList()[new Random().Next(0, 3)];
        }

        public static string GetRandomPhoneNumber(string code)
        {
            string phone = code + new Random().Next(10000, 99999) + new Random().Next(10000, 99999);
            return phone;
        }

        public static int Alls=0;
        public static int Males = 0;
        public static int Females = 0;
        
        public static string GetRandomGender(int countusers=200)
        {
           
            int MaxMale = countusers * 40 / 100;
            int MaxFemale = countusers * 40 / 100;
            int MaxAll = countusers-(MaxMale+ MaxFemale);
            var genders = new string[]{ "", "Male", "Female" };
            string value = genders.ToList()[new Random().Next(0, 3)];

            if (MaxAll <= Alls)
            {
                while (value == "")
                {
                    value = genders.ToList()[new Random().Next(0, 3)];
                }
            }
            if (MaxMale <= Males)
            {

                while (value == "Male")
                {
                    value = genders.ToList()[new Random().Next(0, 3)];
                }
            }
            if (MaxFemale <= Females)
            {

                while (value == "Female")
                {
                    value = genders.ToList()[new Random().Next(0, 3)];
                }
            }
            if (value == "Male")
            {
                Males++;

            }
            else if (value == "Female")
            {
                Females++;
            }
            else if (value == "")
            {
                Alls++;
            }

            return value;
        }

        public static string GetRandomCampaignType()
        {
            var types = new string[] { "Event", "Registration", "Advertising" };
            return types.ToList()[new Random().Next(0, 3)];
        }

        public static string GetRandomDateDOB()
        {

            string day = new Random().Next(1, 28).ToString();
            if (day.Length == 1)
                day = "0" + day;
            string month = new Random().Next(1, 12).ToString();
            if (month.Length == 1)
                month = "0" + month;
            string year = new Random().Next(1930,2003).ToString();
            if (year.Length == 1)
                year = "0" + year;

            return string.Format("{0}-{1}-{2}", day, month, year);
           
        }

        public static DateTime GetRandomDateFromstartDate(int countdays)
        {
            DateTime dtnow = DateTime.Now;
            return dtnow.AddDays((new Random().Next(0, countdays))- countdays );
        }

        public static List<string> GetRandomListKeywordsForCampaign(int countkeywords)
        {

            var colours = new[] {
                          "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenRod", "DarkGray", "DarkGrey", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "Darkorange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DimGrey", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "GoldenRod", "Gray", "Grey", "Green", "GreenYellow", "HoneyDew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenRodYellow", "LightGray", "LightGrey", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon", "MediumAquaMarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenRod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen"
                        };
            var musics = new[] {
                          "Pop", "Rock", "Blue", "R&B", "Country", "Dance", "Traditional", "Classical"
                        };
            var listfood = new[] {
                           "Hamburger",
                           "Hot dog",
                           "Fried chicken",
                           "French steak",
                           "Caviar",
                           "Pizza",
                           "Spaghetti",
                           "Macaroni",
                            "Pasta",
                            "Tofu",
                           "Wanton",
                            "Roast duck",
                            "Sushi",
                            "Ramen",
                            "Kimchi",
                            "Cocoa pot",
                       };
            var holidays = new[] {
                          "New Year", "Christmas", "Women\'s Day", "Independence Day", "Halloween", "Lunar New Year", "Mid Autumn"
                        };


            return GetRandomObjectFromList(countkeywords, colours.Concat(musics).Concat(listfood).Concat(holidays).ToList<object>()).Select(x => x.ToString()).ToList();

        }



        public static List<string> GetRandomListKeywords(string type, int countkeywords)
        {
            switch (type)
            {
                case "food": {
                        var listfood = new[] {
                           "Hamburger",
                           "Hot dog",
                           "Fried chicken",
                           "French steak",
                           "Caviar",
                           "Pizza",
                           "Spaghetti",
                           "Macaroni",
                            "Pasta",
                            "Tofu",
                           "Wanton",
                            "Roast duck",
                            "Sushi",
                            "Ramen",
                            "Kimchi",
                            "Cocoa pot",
                       };
                        return GetListRandom(listfood.ToList(), countkeywords);
                    }
                case "colour":
                    {
                        var colours = new[] {
                          "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenRod", "DarkGray", "DarkGrey", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "Darkorange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DimGrey", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "GoldenRod", "Gray", "Grey", "Green", "GreenYellow", "HoneyDew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenRodYellow", "LightGray", "LightGrey", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon", "MediumAquaMarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenRod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen"           
                        };
                        return GetListRandom(colours.ToList(), countkeywords);
                    }

                case "holiday":
                    {
                        var holidays = new[] {
                          "New Year", "Christmas", "Women\'s Day", "Independence Day", "Halloween", "Lunar New Year", "Mid Autumn"
                        };
                        return GetListRandom(holidays.ToList(), countkeywords);
                    }
                case "music":
                    {
                        var musics = new[] {
                          "Pop", "Rock", "Blue", "R&B", "Country", "Dance", "Traditional", "Classical"
                        };
                        return GetListRandom(musics.ToList(), countkeywords);
                    }

            }

            return null;

        }

    }
}
