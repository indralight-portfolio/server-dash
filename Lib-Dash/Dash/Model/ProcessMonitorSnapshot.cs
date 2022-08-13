using System.Collections.Generic;
using System.Text;
using Common.Utility;
using MessagePack;

namespace Dash.Model
{
    public enum ProcessMonitorSnapShotMiscKey
    {
        Undefined = 0,
        GCHeapStatsPerMin,
    }

    [MessagePackObject()]
    public readonly struct ProcessMonitorSnapshot
    {
        // for mpack
        public ProcessMonitorSnapshot(
            int physicalMemoryUsageKiB,
            ushort cpuUsagePerTenThousand,
            ushort pendingWorkItemCount,
            Dictionary<string, string> miscs) : this()
        {
            PhysicalMemoryUsageKiB = physicalMemoryUsageKiB;
            CpuUsagePerTenThousand = cpuUsagePerTenThousand;
            PendingWorkItemCount = pendingWorkItemCount;
            Miscs = miscs;
        }

        public ProcessMonitorSnapshot(
            int physicalMemoryUsageKiB,
            ushort cpuUsagePerTenThousand,
            ushort pendingWorkItemCount,
            byte gcHeapStatsPerMin,
            Dictionary<string, string> miscs) : this(physicalMemoryUsageKiB, cpuUsagePerTenThousand, pendingWorkItemCount, miscs)
        {
            _gcHeapStatsPerMin = gcHeapStatsPerMin;
        }

        [Key(0)]
        public readonly int PhysicalMemoryUsageKiB;
        [Key(1)]
        public readonly ushort CpuUsagePerTenThousand;
        [Key(2)]
        public readonly ushort PendingWorkItemCount;
        [Key(3)]
        public readonly Dictionary<string, string> Miscs;

        [IgnoreMember]
        private readonly byte _gcHeapStatsPerMin;

        public override string ToString()
        {
            var sb = new StringBuilder(50);
            sb.Append("[PMemory, CpuP, PWIC, GCStatOccur:Min]");

            sb.Append(PhysicalMemoryUsageKiB / 1024);
            sb.Append("MiB ");

            sb.Append($"{(double)CpuUsagePerTenThousand / 10000:P2} ");

            sb.Append(PendingWorkItemCount);
            sb.Append(' ');

            sb.Append(_gcHeapStatsPerMin);

            return sb.ToString();
        }

        public static ProcessMonitorSnapshot Decode(in string input)
        {
            return MessagePackSerializer.Deserialize<ProcessMonitorSnapshot>(System.Text.Encoding.UTF8.GetBytes(input));
        }

        public static ProcessMonitorSnapshot MakeAverage(in Queue<ProcessMonitorSnapshot> list, Dictionary<string, string> miscs)
        {
            if (list.Count == 0)
            {
                return default;
            }

            // linq.Average를 사용하면 좋지만.. IEnumerator 박싱 최적화가 안될 여지가 있어서..
            long totalPhysicalMemoryUsageKiB = 0;
            double totalCpuUsagePercent = 0;
            long totalPendingWorkItemCount = 0;
            long totalGCHeapStatsPerMin = 0;

            if (miscs == null)
            {
                miscs = new Dictionary<string, string>();
            }

            foreach (ProcessMonitorSnapshot snapshot in list)
            {
                totalPhysicalMemoryUsageKiB += snapshot.PhysicalMemoryUsageKiB;
                totalCpuUsagePercent += snapshot.CpuUsagePerTenThousand;
                totalPendingWorkItemCount += snapshot.PendingWorkItemCount;
                totalGCHeapStatsPerMin += snapshot._gcHeapStatsPerMin;
            }

            if (miscs.TryGetValue(nameof(ProcessMonitorSnapShotMiscKey.GCHeapStatsPerMin), out _) == false)
            {
                miscs.Add(nameof(ProcessMonitorSnapShotMiscKey.GCHeapStatsPerMin),
                    (totalGCHeapStatsPerMin / list.Count).ToString());
            }

            return new ProcessMonitorSnapshot(
                (int) (totalPhysicalMemoryUsageKiB / list.Count),
                (ushort) (totalCpuUsagePercent / list.Count),
                (ushort) (totalPendingWorkItemCount / list.Count),
                (byte) (totalGCHeapStatsPerMin / list.Count),
                miscs
            );
        }
    }
}