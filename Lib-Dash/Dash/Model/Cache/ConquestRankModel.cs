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
    public class ConquestRankModel
    {
        [Key(0)]
        public int Rank;
        [Key(1)]
        public ulong OidAccount;
        [Key(2)]
        public int Score;
        public static string GetRedisKey() => "ConquestRank";
        public static string GetConquestRankMember(ulong oidAccount, DateTime updateTime)
        {
            DateTime maxValue = DateTime.MaxValue.Truncate();
            updateTime = updateTime.Truncate();
            return $"{(maxValue - updateTime).Ticks}_{oidAccount}";
        }
        public static string GetConquestRankMember(ConquestScore conquestScore)
        {
            return GetConquestRankMember(conquestScore.OidAccount, conquestScore.UpdateTime);
        }
#if Common_Server
        public static ConquestRankModel Convert(SortedSetEntry sortedSetEntry, int rank)
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
            ConquestRankModel model = new ConquestRankModel
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
