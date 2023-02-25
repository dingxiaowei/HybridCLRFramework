using System;

public class TimeUtils
{

    readonly static DateTime DateTime_1970_01_01_08_00_00 = new DateTime(1970, 1, 1, 8, 0, 0);

    //自 1970 年 1 月 1 日午夜 12:00:00 经过的毫秒数
    public static double GetTotalMillisecondsSince1970()
    {
        DateTime nowtime = DateTime.Now.ToLocalTime();
        return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalMilliseconds;
    }

    public static double GetTotalSecondsSince1970()
    {
        DateTime nowtime = DateTime.Now.ToLocalTime();
        return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalSeconds;
    }
}
