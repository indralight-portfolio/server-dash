using System;
using Common.StaticInfo;
namespace Dash.Types
{
    public enum AddAbilityReasonType
    {
        Undefined = 0,
        LevelUp,
        Angel,
        Devil,
        Cheat,
    }

    public enum SkillCategoryType
    {
        Undefined = 0,
        MainAttribute, // TODO: 이름? (주 특성)
        SubAttribute, // TODO: 이름? (보조 특성)
    }

    public enum SkillModifyType
    {
        Undefined = 0,
        [Comment("곱연산 (0.2f = 20%)")]
        Percent,
        [Comment("고정 수치로 변경")]
        Fixed,
        [Comment("증감")]
        PlusMinus,
    }

    public enum SkillInfoTabType
    {
        Undefined = 0,
        ActionGroup,
        RankAbility,
    }
}