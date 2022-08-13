using Common.StaticInfo;
using Common.Utility;
using Dash.Model.Rdb;
using System;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    // 20ÀÚ Á¦ÇÑ
    public enum PeriodType
    {
        Undefined,
        Episode,
        Product,
        SeasonPass,
        Gacha,
        Event,
        Conquest,
    }

    public struct Period
    {
        public DateTime Start;
        public DateTime End;

        private DateTime Start_Origin;
        private DateTime End_Origin;

        public void Init()
        {
            if (End == DateTime.MinValue)
                End = DateTime.MaxValue.Truncate();
            Start_Origin = Start;
            End_Origin = End;
        }

        public void Override(DateTime start, DateTime end)
        {
            if (end == DateTime.MinValue)
                end = DateTime.MaxValue.Truncate();
            Start = start;
            End = end;
        }

        public void Reset()
        {
            Start = Start_Origin;
            End = End_Origin;
        }

        public bool IsEmpty() => Start == DateTime.MinValue && End == DateTime.MaxValue.Truncate();

        public bool InPeriod(DateTime dateTime)
        {
            return dateTime >= Start && dateTime <= End;
        }
    }

    public struct PeriodSpan
    {
        public TimeSpan Start;
        public TimeSpan End;
    }

    public static class PeriodHelper
    {
        private static List<IHasPeriod> _periodInfos = new List<IHasPeriod>();
#if Common_Server
        public static List<PeriodOverride> PeriodOverrides { get; private set; }
#endif

        public static IdKeyData GetInfo(this PeriodType type, int id)
        {
            switch (type)
            {
                case PeriodType.Episode:
                    StaticInfo.StaticInfo.Instance.EpisodeInfo.TryGet(id, out var episodeInfo);
                    return episodeInfo;
                case PeriodType.Product:
                    StaticInfo.StaticInfo.Instance.ProductInfo.TryGet(id, out var productInfo);
                    return productInfo;
                case PeriodType.SeasonPass:
                    StaticInfo.StaticInfo.Instance.SeasonPassInfo.TryGet(id, out var seasonPassInfo);
                    return seasonPassInfo;
                case PeriodType.Gacha:
                    StaticInfo.StaticInfo.Instance.GachaInfo.TryGet(id, out var gachaInfo);
                    return gachaInfo;
                case PeriodType.Event:
                    StaticInfo.StaticInfo.Instance.EventInfo.TryGet(id, out var eventInfo);
                    return eventInfo;
                case PeriodType.Conquest:
                    StaticInfo.StaticInfo.Instance.ConquestSeasonInfo.TryGet(id, out var conquestSeasonInfo);
                    return conquestSeasonInfo;
                default:
                    return null;
            }
        }

        public static void SetOverride(List<PeriodOverride> periodOverrides)
        {
            foreach (var hasPeriod in _periodInfos)
            {
                hasPeriod.PeriodReset();
            }
            lock (_periodInfos)
            {
                _periodInfos.Clear();
                foreach (var periodOverride in periodOverrides)
                {
                    var periodInfo = periodOverride.PeriodType.GetInfo(periodOverride.Id) as IHasPeriod;
                    if (periodInfo == null) continue;
                    periodInfo.PeriodOverride(periodOverride.Start, periodOverride.End);
                    _periodInfos.Add(periodInfo);
                }
            }
#if Common_Server
            PeriodOverrides = periodOverrides;
#endif
        }

        public static PeriodSpan GetUtcPeriodSpan(this PeriodSpan periodSpan, TimeSpan offset)
        {
            var start = (periodSpan.Start - offset).TuncateDay();
            var end = (periodSpan.End - offset).TuncateDay();
            //if(end < start)
            //    end = end + TimeSpan.FromDays(1);

            return new PeriodSpan
            {
                Start = start,
                End = end,
            };
        }
    }
}