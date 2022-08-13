using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Common.Log;

namespace server_dash.Utility
{
    public readonly struct GCHeapStatsData
    {
        private const int MiBAsBytes = 1024 * 1024;
        public GCHeapStatsData(ulong _1, ulong _2, ulong _3, ulong _4, ulong _5, ulong _6, ulong _7, ulong _8, DateTime time)
        {
            Gen0SizeMiB = (uint)(_1 / MiBAsBytes);
            Gen0PromotedMiB = (uint)(_2 / MiBAsBytes);
            Gen1SizeMiB = (uint)(_3 / MiBAsBytes);
            Gen1PromotedMiB = (uint)(_4 / MiBAsBytes);
            Gen2SizeMiB = (uint)(_5 / MiBAsBytes);
            Gen2SurvivedMiB = (uint)(_6 / MiBAsBytes);
            LohSizeMiB = (uint)(_7 / MiBAsBytes);
            LohSurvivedMiB = (uint)(_8 / MiBAsBytes);
            Time = time;
        }

        public readonly uint Gen0SizeMiB;
        public readonly uint Gen0PromotedMiB;
        public readonly uint Gen1SizeMiB;
        public readonly uint Gen1PromotedMiB;
        public readonly uint Gen2SizeMiB;
        public readonly uint Gen2SurvivedMiB;
        public readonly uint LohSizeMiB;
        public readonly uint LohSurvivedMiB;
        public readonly DateTime Time;

        private static void MiBLog(in StringBuilder sb, string name, uint value, char suffix)
        {
            sb.Append(name);
            sb.Append(':');
            sb.Append(value);
            sb.Append("MiB");
            sb.Append(suffix);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[GCHeapStats][");
            sb.Append(Time);
            sb.Append("][");

            MiBLog(sb, nameof(Gen0SizeMiB), Gen0SizeMiB, '|');
            MiBLog(sb, nameof(Gen0PromotedMiB), Gen0PromotedMiB, '|');
            MiBLog(sb, nameof(Gen1SizeMiB), Gen1SizeMiB, '|');
            MiBLog(sb, nameof(Gen1PromotedMiB), Gen1PromotedMiB, '|');
            MiBLog(sb, nameof(Gen2SizeMiB), Gen2SizeMiB, '|');
            MiBLog(sb, nameof(LohSizeMiB), LohSizeMiB, '|');
            MiBLog(sb, nameof(LohSurvivedMiB), LohSurvivedMiB, ']');

            return sb.ToString();
        }
    }

    public static class GCEventTracer
    {
        public static long HeapStatsOccurCount => Interlocked.Read(ref _heapStatsCount);
        private const int DataMaxCount = 100;

        private static ConcurrentQueue<GCHeapStatsData> _datas = new ConcurrentQueue<GCHeapStatsData>();
        private static readonly NLog.Logger _logger = NLogUtility.GetCurrentClassLogger();

        private static long _heapStatsCount = 0;

        public static void ProcessHeapStats(EventWrittenEventArgs eventData)
        {
            Interlocked.Increment(ref _heapStatsCount);
            // https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events#gcheapstats_v1-event
            var data = new GCHeapStatsData((ulong) eventData.Payload[0], (ulong) eventData.Payload[1],
                (ulong) eventData.Payload[2], (ulong) eventData.Payload[3], (ulong) eventData.Payload[4],
                (ulong) eventData.Payload[5], (ulong) eventData.Payload[6], (ulong) eventData.Payload[7],
                DateTime.UtcNow);

            

            if (_datas.Count >= DataMaxCount)
            {
                _datas.TryDequeue(out _);
            }

            while (_datas.Count >= DataMaxCount)
            {
                _datas.TryDequeue(out _);
            }

            _datas.Enqueue(data);

            _logger.Info(data.ToString());
        }

        public static GCHeapStatsData GetLastData()
        {
            if (_datas.TryPeek(out var data) == true)
            {
                return data;
            }

            return default;
        }
    }
}