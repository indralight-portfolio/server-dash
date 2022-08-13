#if Common_Server
using Dash.Model.Service;
using System;
using TimeZoneConverter;

namespace Dash.Server
{
    public static class ServerTimeHelper
    {
        static TimeZoneInfo _tz;
        public static void Init(string timeZoneName)
        {
            _tz = TZConvert.GetTimeZoneInfo(timeZoneName);
        }        

        public static ServerTime GetServerTime()
        {
            var utcNow = DateTime.UtcNow;
            var utcOffset = _tz.GetUtcOffset(utcNow);

            return new ServerTime(utcNow, utcOffset);
        }
    }
}
#endif