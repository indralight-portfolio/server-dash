using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using Dash.Model.Rdb;
using MessagePack;
#if Common_Server
using StackExchange.Redis;
#endif

namespace Dash.Model.Cache
{
    [MessagePackObject()]
    public class WorldMissionRankModel
    {
        [Key(0)]
        public int Rank;
        [Key(1)]
        public ulong OidAccount;
        [Key(2)]
        public int Score;
        public static string GetRedisKey() => "WorldMissionRank";
        public static string GetRankMember(ulong oidAccount, DateTime updateTime)
        {
            DateTime maxValue = DateTime.MaxValue.Truncate();
            updateTime = updateTime.Truncate();
            return $"{(maxValue - updateTime).Ticks}_{oidAccount}";
        }
        public static string GetRankMember(WorldMissionScore model)
        {
            return GetRankMember(model.OidAccount, model.UpdateTime);
        }
#if Common_Server
        public static WorldMissionRankModel Convert(SortedSetEntry sortedSetEntry, int rank)
        {
            var splitElement = sortedSetEntry.Element.ToString().Split("_");
            if (splitElement.Length < 2)
            {
                return null;
            }
            if (ulong.TryParse(splitElement[1], out var oidAccount) == false)
            {
                return null;
            }
            WorldMissionRankModel model = new WorldMissionRankModel
            {
                OidAccount = oidAccount,
                Rank = rank,
                Score = (int)sortedSetEntry.Score,
            };
            return model;

        }
#endif
    }
}
