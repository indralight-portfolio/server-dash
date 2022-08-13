using Common.Locale;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dash.Hive
{
#if Common_Server
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ZoneType
    {
        SANDBOX,
        REAL,
    }
#endif

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerZoneType
    {
        UNDEFINED = 0,
        QA = 11,
        [EnumMember(Value = "QA-2")]
        QA2 = 12,
        [EnumMember(Value = "QA-3")]
        QA3 = 13,
        GLOBAL = 21,
        KR = 71,
        US = 81,
        TEST = 127,
        [EnumMember(Value = "TEST-2")]
        TEST2 = 128,
    }

    public static class HiveEnumHelper
    {
        public static string toString(this ServerZoneType serverZone)
        {
            FieldInfo fi = serverZone.GetType().GetField(serverZone.ToString());
            EnumMemberAttribute[] attributes = (EnumMemberAttribute[])fi.GetCustomAttributes(typeof(EnumMemberAttribute), false);

            if ((attributes?.Length ?? 0) > 0)
                return attributes[0].Value;
            else
                return serverZone.ToString();
        }

        public static int toInt(this ServerZoneType serverZone)
        {
            return Math.Min((int)serverZone, 127);
        }

        public static string GetName(this ServerZoneType serverZone)
        {
            switch (serverZone)
            {
                case ServerZoneType.KR:
                    return "Asia";
                case ServerZoneType.GLOBAL:
                    return "Global";
                default:
                    return serverZone.toString();
            }
        }
    }
}