using System;

namespace Babu
{
    public class TimeUtils
    {
        public static int Min = 60;
        public static int Hour = 60 * Min;
        public static int Day = 24 * Hour;
        public static int Month = 30 * Day;

        private static DateTime utcStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime localStartTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);


        /// <summary>
        /// 本地系统秒级时间戳
        /// 非必要情况（未登录获取时间差等）请尽可能使用服务器时间:Utils.DataConvUtil.ServerTime
        /// </summary>
        public static long Now()
        {
            return (long)(DateTime.UtcNow - utcStartTime).TotalSeconds;
        }

        /// <summary>
        /// 本地系统毫秒级时间戳
        /// 非必要情况（未登录获取时间差等）请尽可能使用服务器时间:Utils.DataConvUtil.ServerTimeEx
        /// </summary>
        public static long NowEx()
        {
            return (long)(DateTime.UtcNow - utcStartTime).TotalMilliseconds;
        }

        public static long ToUnixStamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - utcStartTime).TotalSeconds;
        }

        public static long ToUnixStampEx(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - utcStartTime).TotalMilliseconds;
        }

        /// <summary>
        /// 配置表中的时间戳通常是没算过时区的，所以需要加上时区
        /// </summary>
        public static DateTime ToDateTime(long ts)
        {
            return localStartTime.AddSeconds(ts);
        }

        public static DateTime ToDateTimeEx(long ts)
        {
            return localStartTime.AddMilliseconds(ts);
        }

        public static DateTime ToUtcDateTime(long ts)
        {
            return utcStartTime.AddSeconds(ts);
        }

        public static DateTime ToUtcDateTimeEx(long ts)
        {
            return utcStartTime.AddMilliseconds(ts);
        }

        // 今天0点时间
        public static long TimeOfToday()
        {
            return ToUnixStamp(DateTime.Today);
        }

        // 本周日0点时间
        public static long TimeOfWeek()
        {
            int diff = DateTime.Today.DayOfWeek - DayOfWeek.Sunday;
            return ToUnixStamp(DateTime.Today.AddDays(-diff));
        }

        public static string GetTimeString(long time)
        {
            return TimeSpan.FromSeconds(time).ToString();
        }

        public static string GetUnixTimeString(long time, string format = "yyyy/MM/dd HH:mm:ss")
        {
            DateTime date;
            //ENd
            date = utcStartTime.AddSeconds(time).ToLocalTime();

            return date.ToString(format);
        }

        // 显示格式 00:00:00
        public static string GetTimeSpanString(TimeSpan time)
        {
            return $" {(int)time.TotalHours:d2}:{time.Minutes.ToString("d2")}:{time.Seconds.ToString("d2")}";
        }

        #region 时间格式化

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"12小时34分56秒"格式的时间字符串</returns>
        public static string FormatLeftTimeWithHourCn(int leftTime)
        {
            if (leftTime < 0) return "00:00";
            int hour = leftTime / 3600;
            string hourStr = Zerofill(hour);
            int min = leftTime / 60 % 60;
            string minStr = Zerofill(min);
            int sec = leftTime % 60;
            string secStr = Zerofill(sec);
            if (hour > 0)
            {
                return $"{hourStr}小时{minStr}分{secStr}秒";
            }
            else
            {
                return $"{minStr}分{secStr}秒";
            }
        }
        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"12:34:56"格式的时间字符串</returns>
        public static string FormatLeftTimeWithHour(int leftTime)
        {
            if (leftTime < 0) return "00:00";
            int hour = leftTime / 3600;
            string hourStr = Zerofill(hour);
            int min = leftTime / 60 % 60;
            string minStr = Zerofill(min);
            int sec = leftTime % 60;
            string secStr = Zerofill(sec);
            if (hour > 0)
            {
                return $"{hourStr}:{minStr}:{secStr}";
            }
            else
            {
                return $"{minStr}:{secStr}";
            }
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"12:34"格式的时间字符串</returns>
        public static string FormatLeftTime(int leftTime)
        {
            if (leftTime <= 0) return "00:00";
            int min = leftTime / 60;
            string minStr = Zerofill(min);
            int sec = leftTime % 60;
            string secStr = Zerofill(sec);
            return minStr + ":" + secStr;
        }
        public static string Zerofill(int num)
        {
            if (num < 0) return "00";
            if (num < 10) return "0" + num.ToString();
            return num.ToString();
        }

        public static string FormatHourTime(int secs)
        {
            return $"{(secs / 3600)}小时";
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="millisecond">剩余时间，单位为毫秒</param>
        /// <returns>"12:34:98"格式的时间字符串(分：秒：厘秒)</returns>
        public static string FormatLeftTimeWithMillisecond(int millisecond)
        {
            if (millisecond <= 0) return "00:00:00";
            int min = millisecond / 1000 / 60;
            string minStr = Zerofill(min);
            int sec = millisecond / 1000 % 60;
            string secStr = Zerofill(sec);
            int endSec = millisecond / 10 % 100;
            string endSecStr = Zerofill(endSec);
            return minStr + ":" + secStr + ":" + endSecStr;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"7天5小时"格式的时间字符串</returns>
        public static string FormatLeftTimeWithDayCn(int leftTime)
        {
            if (leftTime < 0) return "0天0小时";
            int hour = leftTime / 3600;
            int day = hour / 24;
            int leftHour = hour - day * 24;

            string endStr = "";
            if (day < 0)
            {
                endStr = leftHour + "小时";
            }
            else
            {
                endStr = day + "天";
                if (leftHour > 0) endStr += leftHour + "小时";
            }
            return endStr;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"7天05:12:34"格式的时间字符串,不足一天不显示天数</returns>
        public static string FormatLeftTimeWithDayCnOtherEn(int leftTime)
        {
            if (leftTime < 0) return "00:00:00";
            int daySec = 24 * 3600;
            int day = leftTime / daySec;
            int leftSec = leftTime - (day * daySec);
            string other = FormatLeftTimeWithHour(leftSec);
            if (day <= 0)
            {
                return other;
            }
            else
            {
                return day + "天" + other;
            }
        }

        #endregion
    }
}
