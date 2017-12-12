using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImgServer.Net
{
    public class MyDateTime
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public int MSec;

        public MyDateTime(DateTime dt)
        {
            Year = dt.Year;
            Month = dt.Month;
            Day = dt.Day;
            Hour = dt.Hour;
            Minute = dt.Minute;
            Second = dt.Second;
            MSec = dt.Millisecond;
        }

        public MyDateTime()
        {
            Year = DateTime.Now.Year;
            Month = DateTime.Now.Month;
            Day = DateTime.Now.Day;
            Hour = DateTime.Now.Hour;
            Minute = DateTime.Now.Minute;
            Second = DateTime.Now.Second;
            MSec = DateTime.Now.Millisecond;
        }
        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day, Hour, Minute, Second, MSec);
        }
        public static DateTime GetDefaultBidTime()
        {
            MyDateTime myTime = new MyDateTime();
            myTime.Hour = 11;
            myTime.Minute = 29;
            myTime.Second = 47;
            myTime.MSec = 0;
            return myTime.ToDateTime();
        }

        public static DateTime GetDefaultCommitTime1()
        {
            MyDateTime myTime = new MyDateTime();
            myTime.Hour = 11;
            myTime.Minute = 29;
            myTime.Second = 51;
            myTime.MSec = 0;
            return myTime.ToDateTime();
        }

        public static DateTime GetDefaultCommitTime2()
        {
            MyDateTime myTime = new MyDateTime();
            myTime.Hour = 11;
            myTime.Minute = 29;
            myTime.Second = 54;
            myTime.MSec = 0;
            return myTime.ToDateTime();
        }

        public static DateTime GetDefaultCommitTime400()
        {
            MyDateTime myTime = new MyDateTime();
            myTime.Hour = 11;
            myTime.Minute = 29;
            myTime.Second = 54;
            myTime.MSec = 0;
            return myTime.ToDateTime();
        }

        public static DateTime GetDefaultCommitTime500()
        {
            MyDateTime myTime = new MyDateTime();
            myTime.Hour = 11;
            myTime.Minute = 29;
            myTime.Second = 54;
            myTime.MSec = 500;
            return myTime.ToDateTime();
        }
    }
}
