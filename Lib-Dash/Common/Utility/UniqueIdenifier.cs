using System;

namespace Common.Utility
{
    public class UniqueIdenifier
    {
        public static string GenerateDummyDeviceId()
        {
            return $"D:{Random.NextString(14)}";
        }

        public static string GenerateDeviceId()
        {
            // 유니티 에디터에서만 생성하므로 앞에 접두어 "UE" 를 붙이자.
            return $"UE:{Random.NextString(13)}";
        }

        public static string GenerateSocialId()
        {
            // 유니티 에디터에서만 생성하므로 앞에 접두어 "US" 를 붙이자.
            return $"US:{Random.NextString(13)}";
        }

        public static string GenerateSessionKey(DateTime now)
        {
            return Random.Range(long.MinValue, long.MaxValue).ToString();
        }
    }
}