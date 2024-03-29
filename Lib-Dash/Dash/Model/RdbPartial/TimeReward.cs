﻿// This code is generated by EntityTypeGenerator. Dot not Edit!
using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.Types;
using System;
using System.Collections.Generic;

#nullable disable

namespace Dash.Model.Rdb
{
    public static class TimeRewardHelper
    {
        public static List<TimeReward> Merge(this List<TimeReward> timeRewards, TimeReward timeReward)
        {
            if (timeReward == null) return timeRewards;
            timeRewards ??= new List<TimeReward>();
            timeRewards.AddOrUpdate(timeReward, (e => e.RewardType == timeReward.RewardType));
            return timeRewards;
        }

        public static ErrorCode CheckAndInc(Account account, string rewardType, TimeRewardInfo timeRewardInfo, ref TimeReward timeReward, ServerTime serverTime)
        {
            if (timeReward == null)
            {
                timeReward = new TimeReward
                {
                    OidAccount = account.OidAccount,
                    RewardType = rewardType,
                    RewardTime = DateTime.UtcNow
                };
            }
            else
            {
                var errorcode = Check(timeRewardInfo, ref timeReward, serverTime);
                if (errorcode != ErrorCode.Success)
                    return errorcode;
            }
            timeReward.Count++;
            return ErrorCode.Success;
        }

        public static ErrorCode Check(TimeRewardInfo timeRewardInfo, ref TimeReward timeReward, ServerTime serverTime)
        {
            if (timeReward == null)
                return ErrorCode.Success;
            else
            {
                var resetConfig = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get().Reset;
                LocalTimeComparer localTimeComparer;
                switch (timeRewardInfo.ResetType)
                {
                    case TimeRewardResetType.Daily:
                        localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetHour);
                        break;
                    case TimeRewardResetType.Weekly:
                        localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetDayOfWeek, resetConfig.ResetHour);
                        break;
                    case TimeRewardResetType.Monthly:
                        localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetDayOfMonth, resetConfig.ResetHour);
                        break;
                    default:
                        return ErrorCode.InvalidData;
                }
                if (localTimeComparer.IsAfterResetTime(serverTime.Utc, timeReward.RewardTime) == true)
                    timeReward.Count = 0;
                timeReward.RewardTime = serverTime.Utc;

                if (timeReward.Count >= timeRewardInfo.Count)
                {
                    return ErrorCode.OverLimit;
                }
            }
            return ErrorCode.Success;
        }
        public static bool IsAfterResetTime(TimeRewardInfo timeRewardInfo, TimeReward timeReward, ServerTime serverTime)
        {
            var resetConfig = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get().Reset;
            LocalTimeComparer localTimeComparer;
            switch (timeRewardInfo.ResetType)
            {
                case TimeRewardResetType.Daily:
                    localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetHour);
                    break;
                case TimeRewardResetType.Weekly:
                    localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetDayOfWeek, resetConfig.ResetHour);
                    break;
                case TimeRewardResetType.Monthly:
                    localTimeComparer = new LocalTimeComparer(serverTime.Offset, resetConfig.ResetDayOfMonth, resetConfig.ResetHour);
                    break;
                default:
                    return false;
            }
            return localTimeComparer.IsAfterResetTime(serverTime.Utc, timeReward.RewardTime);
        }
    }
}
