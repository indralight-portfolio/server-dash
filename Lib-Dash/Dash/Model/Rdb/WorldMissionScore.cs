﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    [MessagePack.MessagePackObject()]
    public partial class WorldMissionScore : Common.Model.IModel
    {
        public static bool IsMultipleDbModel => false;
        public static string GetMainKeyName() { return nameof(OidAccount); }
        public static List<KeyValuePair<string, object>> MakeSubKeysWithName(params object[] keys)
        {
            return null;
        }

        public string GetMainKey() { return OidAccount.ToString(); }
        public List<string> GetSubKeys()
        {
            return null;
        }
        public bool IsAutoIncKeysValid()
        {
            return true;
        }

        public WorldMissionScore() { }
        public WorldMissionScore(WorldMissionScore other)
        {
            OidAccount = other.OidAccount;
            CoopRewardedScore = other.CoopRewardedScore;
            CoopScore = other.CoopScore;
            Data = other.Data;
            EventId = other.EventId;
            IsRankingRewarded = other.IsRankingRewarded;
            RewardedScore = other.RewardedScore;
            Score = other.Score;
            UpdateTime = other.UpdateTime;
        }


        [MessagePack.Key(0)]
        [Column]
        [KeyColumn]
        public ulong OidAccount { get; set; }
        [MessagePack.Key(4)]
        [Column]
        public int EventId { get; set; }
        [MessagePack.Key(7)]
        [Column]
        public uint Score { get; set; }
        [MessagePack.Key(2)]
        [Column]
        public uint CoopScore { get; set; }
        [MessagePack.Key(5)]
        [Column]
        public bool IsRankingRewarded { get; set; }
        [MessagePack.Key(6)]
        [Column]
        public uint RewardedScore { get; set; }
        [MessagePack.Key(1)]
        [Column]
        public ulong CoopRewardedScore { get; set; }
        [MessagePack.Key(3)]
        [Column]
        public byte[] Data { get; set; }
        [MessagePack.Key(8)]
        [Column]
        public DateTime UpdateTime { get; set; }
    }
}
