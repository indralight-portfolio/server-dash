using System;

namespace Dash.Net
{
    public static class Constant
    {
#if DEBUG
        public static readonly TimeSpan AliveTimeout = TimeSpan.FromSeconds(60);
#else
        public static readonly TimeSpan AliveTimeout = TimeSpan.FromSeconds(30);
#endif

#if UNITY_EDITOR
        public const int WarningThresholdMs = 5000;
#else
        public const int WarningThresholdMs = 5000;
#endif

#if UNITY_EDITOR
        public const int ErrorThresholdMs = 30000;
#else
        public const int ErrorThresholdMs =30000;
#endif
    }
}