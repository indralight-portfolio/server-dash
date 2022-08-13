using Dash.Model;
using Dash.StaticData;
using Dash.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using server_dash.Battle.Services.Entities;
using server_dash.Battle.Services.GamePlay;
using System.Collections.Generic;
using System.Linq;

namespace server_dash.Statistics
{
    public class PlayerNotAlivePayload : IPayload
    {
        public int ArenaSerial;
        public int PlayerId;
        public int Count;
    }

    public class PlayerDroppedPayload : IPayload
    {
        public int ArenaSerial;
        public int PlayerId;
    }

    public class ArenaPlayerPayload : IPayload
    {
        public ulong OidAccount;
        public int HeroId;
        public int HeroLevel;
        public int EmblemId;

        public static ArenaPlayerPayload From(PlayerEntity playerEntity)
        {
            ArenaPlayerPayload result = new ArenaPlayerPayload();
            result.OidAccount = playerEntity.OidAccount;
            result.HeroId = playerEntity.Model.HeroId;
            result.HeroLevel = playerEntity.Model.HeroLevel;
            result.EmblemId = playerEntity.Model.EmblemId;

            return result;
        }
    }

    public class ArenaRewardPayload : IPayload
    {
        public int Gold;
        public int Consume;
        public int Equipment;
        public int Hero;

        public static ArenaRewardPayload From(RewardInfo rewardInfo)
        {
            ArenaRewardPayload result = new ArenaRewardPayload();
            result.Gold = rewardInfo.Gold;
            result.Consume = rewardInfo.Consumes?.Count ?? 0;
            result.Equipment = rewardInfo.Equipments?.Count ?? 0;
            result.Hero = rewardInfo.Heroes?.Count ?? 0;

            return result;
        }
    }

    public class ArenaPlayerStatsPayload : IPayload
    {
        [JsonProperty("Revive")]
        public int ReviveCount;
        [JsonProperty("JewelRevive")]
        public int TicketReviveCount;
        public int Attack;
        public int Damage;
        public int Heal;

        public static ArenaPlayerStatsPayload From(PlayerStatisticsInfo playerStatisticsInfo)
        {
            ArenaPlayerStatsPayload result = new ArenaPlayerStatsPayload();
            result.ReviveCount = playerStatisticsInfo.ReviveCount;
            result.TicketReviveCount = playerStatisticsInfo.TicketReviveCount;
            result.Attack = playerStatisticsInfo.Attack;
            result.Damage = playerStatisticsInfo.Damage;
            result.Heal = playerStatisticsInfo.Heal;

            return result;
        }
    }

    public class ArenaCreatePayload : IPayload
    {
        public int Serial;
        public int ChapterId;
        public int PlayerCnt;

        public static ArenaCreatePayload From(ArenaContext arenaContext)
        {
            ArenaCreatePayload result = new ArenaCreatePayload();
            result.Serial = arenaContext.Arena.Serial;
            result.ChapterId = arenaContext.Arena.ChapterId;
            result.PlayerCnt = arenaContext.Arena.Players.Count;

            return result;
        }
    }


    public class ArenaStartPayload : IPayload
    {
        public int Serial;
        public int ChapterId;
        public int PlayerCnt;

        public ArenaPlayerPayload MyInfo;

        public static ArenaStartPayload From(ArenaContext arenaContext, ulong oidAccount)
        {
            ArenaStartPayload result = new ArenaStartPayload();
            result.Serial = arenaContext.Arena.Serial;
            result.ChapterId = arenaContext.Arena.ChapterId;
            result.PlayerCnt = arenaContext.Arena.Players.Count;

            PlayerEntity myEntity = arenaContext.Players.FirstOrDefault(p => p.OidAccount == oidAccount);
            if (myEntity != null)
            {
                result.MyInfo = ArenaPlayerPayload.From(myEntity);
            }
            return result;
        }

    }

    public class ArenaQuitPayload : IPayload
    {
        public int Serial;
        public int ChapterId;

        public ArenaPlayerPayload MyInfo;

        public static ArenaQuitPayload From(ArenaContext arenaContext, ulong oidAccount)
        {
            ArenaQuitPayload result = new ArenaQuitPayload();
            result.Serial = arenaContext.Arena.Serial;
            result.ChapterId = arenaContext.Arena.ChapterId;

            PlayerEntity myEntity = arenaContext.Players.FirstOrDefault(p => p.OidAccount == oidAccount);
            if (myEntity != null)
            {
                result.MyInfo = ArenaPlayerPayload.From(myEntity);
            }
            return result;
        }
    }

    public class ArenaEndPayload : IPayload
    {
        public int Serial;
        public int ChapterId;
        [JsonConverter(typeof(StringEnumConverter))]
        public ArenaEndType EndType;
        [JsonProperty("PlayerCnt")]
        public int PlayerCnt;
        public int StageStep;
        public int PlayTime;

        public ArenaPlayerPayload MyInfo;
        public ArenaRewardPayload Reward;
        public ArenaPlayerStatsPayload Stats;

        public ArenaPlayerPayload[] Players;

        public static ArenaEndPayload From(ArenaContext arenaContext, ArenaResult arenaResult, ulong oidAccount)
        {
            ArenaEndPayload result = new ArenaEndPayload();
            result.Serial = arenaContext.Arena.Serial;
            result.ChapterId = arenaContext.Arena.ChapterId;
            result.PlayerCnt = arenaContext.Players.Count;
            result.PlayTime = (int)arenaContext.ElapsedMillisecondsSafe / 1000;

            int playerId = 0;
            PlayerEntity myEntity = arenaContext.Players.FirstOrDefault(p => p.OidAccount == oidAccount);
            if (myEntity != null)
            {
                playerId = myEntity.Id;
                result.MyInfo = ArenaPlayerPayload.From(myEntity);
            }
            result.Players = arenaContext.Players.Where(p => p.OidAccount != oidAccount).Select(p => ArenaPlayerPayload.From(p)).ToArray();

            if (arenaResult != null)
            {
                result.EndType = arenaResult.GameResult.EndType;
                result.StageStep = arenaResult.GameResult.ClearedStageCount;

                if (arenaResult.PlayerResults.TryGetValue(playerId, out PlayerResult playerResult))
                {
                    result.Reward = ArenaRewardPayload.From(playerResult.RewardInfo);
                    result.Stats = ArenaPlayerStatsPayload.From(playerResult.PlayerStatisticsInfo);
                }
            }
            else
            {
                result.EndType = ArenaEndType.Undefined;
            }

            return result;
        }
    }

    public struct StageEndPlayload : IPayload
    {
        public int Serial;
        public int ChapterId;
        public int StageId;
        public int StageStep;
        public int PlayTime;
        public int DeadCnt;

        public static List<StageEndPlayload> ListFrom(ArenaContext arenaContext, ArenaResult arenaResult, ulong oidAccount)
        {
            List<StageEndPlayload> result = null;
            int playerId = 0;
            PlayerEntity myEntity = arenaContext.Players.FirstOrDefault(p => p.OidAccount == oidAccount);
            if (myEntity != null)
            {
                playerId = myEntity.Id;
                if (arenaResult != null && arenaResult.PlayerResults.TryGetValue(playerId, out PlayerResult playerResult))
                {
                    result = playerResult.StageStatisticsInfos.Select(e =>
                        new StageEndPlayload
                        {
                            Serial = arenaContext.Arena.Serial,
                            ChapterId = arenaContext.Arena.ChapterId,
                            StageId = e.StageId,
                            StageStep = e.StageStep,
                            PlayTime = e.PlaySeconds,
                            DeadCnt = e.DeadCount,
                        }).ToList();
                }
            }

            return result;
        }
    }

    public struct SkillPickPayload : IPayload
    {
        public int SkillId;
        public int Count;

        public static List<SkillPickPayload> ListFrom(ArenaContext arenaContext, ArenaResult arenaResult, ulong oidAccount)
        {
            List<SkillPickPayload> result = null;
            int playerId = 0;
            PlayerEntity myEntity = arenaContext.Players.FirstOrDefault(p => p.OidAccount == oidAccount);
            if (myEntity != null)
            {
                playerId = myEntity.Id;
                if (arenaResult != null && arenaResult.PlayerResults.TryGetValue(playerId, out PlayerResult playerResult))
                {
                    result = playerResult.SkillStatistics.Select(e =>
                        new SkillPickPayload
                        {
                            SkillId = e.Key,
                            Count = e.Value,
                        }).ToList();
                }
            }

            return result;
        }
    }

    public class PlayerHackReportPayload : IPayload
    {
        public int CaseCount;
        public int ReportCount;
    }
}