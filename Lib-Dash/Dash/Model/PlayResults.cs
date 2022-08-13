using Common.Utility;
using Dash.StaticData;
using MessagePack;
using System.Collections.Generic;

namespace Dash.Model
{
    [MessagePackObject()]
    public class GameResult
    {
        [Key(0)] public Types.ArenaEndType EndType;
        [Key(1)] public int ClearedStageCount;
        [Key(2)] public int LastStageId;
        [Key(3)] public float PlaySeconds;
        [Key(4)] public float RemainBossHpRatio;
        [Key(5)] public int BossGotHitDamageAmount;//보스 피해량
    }

    [MessagePackObject()]
    public class PlayerResult
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public ulong OidAccount;
        [Key(2)] public PlayerStatisticsInfo PlayerStatisticsInfo = new PlayerStatisticsInfo() { LowestHpRatio = 1.0f };
        //[Key(3)] public RewardInfo RewardInfo = new RewardInfo();
        //[Key(4)] public float GoldBonusRate;
        [Key(4)] public List<StageStatisticsInfo> StageStatisticsInfos = new List<StageStatisticsInfo>();
        [Key(5)] public Dictionary<byte/*ActionGroupType*/, int/*Count*/> SkillStatistics = new Dictionary<byte, int>();

        public void AddHash(HashCodeBuilder builder)
        {
            builder.Add(PlayerId).Add(OidAccount);
            PlayerStatisticsInfo.AddHash(builder);
            //RewardInfo.AddHash(builder);
        }
    }

    [MessagePackObject()]
    public struct PlayerStatisticsInfo
    {
        [Key(0)] public int DeadCount;
        [Key(1)] public int ReviveCount;
        [Key(2)] public int TicketReviveCount;
        [Key(3)] public int CoopReviveCount; // 내가 남을 부활시킨 횟수
        [Key(4)] public int Attack; // 오버데미지 포함 X
        [Key(5)] public int Damage; // 실 데미지
        [Key(6)] public int Heal; // 오버힐 포함 X
        [Key(7)] public int Kill; // 죽인 몬스터 수
        [Key(8)] public int Barrier;//배리어로 막은 피해
        [Key(9)] public int Shield; //실드로 막은 피해
        [Key(10)] public int TileMoveCount; //이동한 거리 (타일 수)
        [Key(11)] public float LowestHpRatio;
        [Key(12)] public int HitCount;//피격 카운트

        public void AddHash(HashCodeBuilder builder)
        {
            builder.Add(ReviveCount).Add(TicketReviveCount);
        }
    }

    [MessagePackObject()]
    public struct StageStatisticsInfo
    {
        [Key(0)] public int StageId;
        [Key(1)] public int StageStep;
        [Key(2)] public int PlaySeconds; // 클리어 / 실패시까지 걸린 시간(클리어 이후 시간 포함 X)
        [Key(3)] public int DeadCount;
    }
}