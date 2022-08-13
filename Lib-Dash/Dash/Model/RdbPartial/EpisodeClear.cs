using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.StaticData.Episode;
using Dash.StaticData.Tower;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class EpisodeClear
    {
        public static readonly int MissionCount = EpisodeMissionInfo.MissionCount;

        [IgnoreMember, JsonIgnore]
        public EpisodeInfo Info { get { StaticInfo.Instance.EpisodeInfo.TryGet(EpisodeId, out var info); return info; } }

        private BitArray missions;

#if Common_Server
        [NotMapped]
#endif
        [IgnoreMember, JsonIgnore]
        public BitArray Missions
        {
            get
            {
                if (missions == null)
                {
                    missions = new BitArray(MissionCount);
                    for (int i = 0; i < MissionCount; ++i)
                    {
                        missions[i] = (MissionBits & 1 << i) > 0;
                    }
                }
                return missions;
            }
            set
            {
                if (value != null)
                {
                    MissionBits = 0;
                    for (int i = 0; i < value.Length; ++i)
                    {
                        MissionBits |= (byte)(value[i] ? 1 << i : 0);
                    }
                    missions = value;
                }
            }
        }

#if Common_Server
        [NotMapped]
#endif
        [IgnoreMember, JsonIgnore]
        public BitArray NewMissions = new BitArray(MissionCount);

        [IgnoreMember, JsonIgnore]
        public int CompletedStar => Missions.CountTrue();

        public void SetClear(BitArray clears)
        {
            if (clears.Length < MissionCount)
                return;

            var missions = new BitArray(MissionCount);
            for (int i = 0; i < MissionCount; ++i)
            {
                NewMissions[i] = !Missions[i] & clears[i];
                missions[i] = Missions[i] | clears[i];
            }
            Missions = missions;
        }

        public void Compare(EpisodeClear old)
        {
            if (old == null)
            {
                for (int i = 0; i < MissionCount; ++i)
                {
                    NewMissions[i] = true;
                }
            }
            else
            {
                for (int i = 0; i < MissionCount; ++i)
                {
                    NewMissions[i] = !old.NewMissions[i] & NewMissions[i];
                }
            }
        }
        public bool IsRemoveSeasonEpisodeClear(ServerTime serverTime, ConquestSeasonInfo conquestSeasonInfo, TowerSeasonInfo towerSeasonInfo)
        {
            DateTime? seasonStartTime;
            if (Info.GroupType == Types.EpisodeGroupType.Conquest)
            {
                seasonStartTime = conquestSeasonInfo?.TotalPeriod.Start ?? null;
            }
            else if(Info.GroupType == Types.EpisodeGroupType.Tower)
            {
                seasonStartTime = towerSeasonInfo?.OpenPeriod.Start ?? null;
            }
            else if(Info.GroupType == Types.EpisodeGroupType.WorldMission)
            {
                seasonStartTime = StaticData.Event.WorldMissionEventInfo.GetCurrentEventInfo(serverTime.Utc)?.OpenPeriod.Start ?? null;
            }
            else
            {
                return false;
            }
            return IsRemoveSeasonEpisodeClear(serverTime, seasonStartTime);
        }
        private bool IsRemoveSeasonEpisodeClear(ServerTime serverTime, DateTime? currentSeasonStartTime)
        {
            //토벌, 시련의 탑이 아니면 지울 필요 없다
            if (Info.GroupType != Types.EpisodeGroupType.Conquest && Info.GroupType != Types.EpisodeGroupType.Tower)
            {
                return false;
            }
            //현재 시즌이 시작할때 지움
            if (currentSeasonStartTime == null)
            {
                return false;
            }
            return (UpdateTime ?? serverTime.Utc) < currentSeasonStartTime;
        }
    }
}
