using System.Globalization;

namespace ZakathApp.API.Helpers
{
    public class HijriDateHelper
    {
        private readonly UmAlQuraCalendar _hijriCalendar;

        public HijriDateHelper()
        {
            _hijriCalendar = new UmAlQuraCalendar();
        }

        public string ConvertToHijri(DateTime gregorianDate)
        {
            try
            {
                int hijriYear = _hijriCalendar.GetYear(gregorianDate);
                int hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
                int hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);

                string monthName = GetHijriMonthName(hijriMonth);

                return $"{hijriDay} {monthName} {hijriYear}";
            }
            catch
            {
                return null;
            }
        }

        public DateTime? ConvertToGregorian(int hijriYear, int hijriMonth, int hijriDay)
        {
            try
            {
                return _hijriCalendar.ToDateTime(hijriYear, hijriMonth, hijriDay, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }

        public string GetHijriMonthName(int month)
        {
            return month switch
            {
                1 => "Muharram",
                2 => "Safar",
                3 => "Rabi' al-Awwal",
                4 => "Rabi' al-Thani",
                5 => "Jumada al-Awwal",
                6 => "Jumada al-Thani",
                7 => "Rajab",
                8 => "Sha'ban",
                9 => "Ramadan",
                10 => "Shawwal",
                11 => "Dhu al-Qi'dah",
                12 => "Dhu al-Hijjah",
                _ => ""
            };
        }

        public string GetHijriMonthNameArabic(int month)
        {
            return month switch
            {
                1 => "محرم",
                2 => "صفر",
                3 => "ربيع الأول",
                4 => "ربيع الثاني",
                5 => "جمادى الأولى",
                6 => "جمادى الآخرة",
                7 => "رجب",
                8 => "شعبان",
                9 => "رمضان",
                10 => "شوال",
                11 => "ذو القعدة",
                12 => "ذو الحجة",
                _ => ""
            };
        }

        public int GetDaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).Days;
        }

        public DateTime AddHijriMonths(DateTime date, int months)
        {
            return _hijriCalendar.AddMonths(date, months);
        }

        public DateTime AddHijriYears(DateTime date, int years)
        {
            return _hijriCalendar.AddYears(date, years);
        }

        public bool IsHawlComplete(DateTime startDate, DateTime endDate)
        {
            // Hawl period is 354 days (lunar year)
            int daysDifference = GetDaysBetweenDates(startDate, endDate);
            return daysDifference >= 354;
        }

        public DateTime GetHawlEndDate(DateTime startDate)
        {
            // Add 354 days for lunar year
            return startDate.AddDays(354);
        }
    }
}
