using System;

namespace Helpers
{
    public static class DateHelper
    {
        //default 5 day to check
        public static int CheckTime = 7200;
        public static int StringDateToInt(string dateString)
        {
            switch (dateString)
            {
                case "30 минут":
                    return 30;
                case "1 час":
                    return 60;
                case "2 часа":
                    return 120;
                case "6 часов":
                    return 360;
                case "12 часов":
                    return 720;
                case "1 день":
                    return 1440;
                case "2 дня":
                    return 2880;
                case "3 дня":
                    return 4320;
                case "4 дня":
                    return 5760;
                case "5 дней":
                    return 7200;
                case "6 дней":
                    return 8640;
                case "7 дней":
                    return 10080;
                case "10 дней":
                    return 14400;
                case "15 дней":
                    return 21600;
                case "20 дней":
                    return 28800;
                case "30 дней":
                    return 43200;
                default:
                    return 0;
            }
        }
    }
}