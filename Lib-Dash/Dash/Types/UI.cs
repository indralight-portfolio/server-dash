using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.StaticInfo;
using Dash.Types;

namespace Dash.Types
{
    public enum StatCalcType
    {
        [Comment("###미설정###")] Undefined = 0,
        [Comment("계산X_%표기")] None_Percent,
        [Comment("계산X_숫자만 표기")] JustNumer,
        [Comment("StatusDeltaValue - 1_%표기")] StatusDeltaMultiply_Percent,
        [Comment("절대값표기")] ABSNumber,
    }

    public enum StatValueType
    {
        Undefined = 0,
        Int,
        Float,
    }
}

namespace Dash
{
    public static class StatHelper
    {
        public static readonly ReadOnlyCollection<StatCalcType> PercentTypes;
        static StatHelper()
        {
            PercentTypes = new ReadOnlyCollection<StatCalcType>(new List<StatCalcType>()
            {
                StatCalcType.None_Percent,
                StatCalcType.StatusDeltaMultiply_Percent,
            });
        }

        public static bool IsPercentType(StatCalcType type)
        {
            return PercentTypes.Contains(type);
        }
    }
}