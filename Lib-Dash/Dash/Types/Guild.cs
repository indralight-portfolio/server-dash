

using Common.StaticInfo;

namespace Dash.Types
{
    public enum GuildMemberGrade
    {
        Undefined = 0,
        Normal,
        Manager,
        Master,
    }
    public enum GuildSystemChat
    {
        RequestJoin,//xx님이 가입 신청하였습니다.
        JoinNewMember,//xx 님이 길드에 가입했습니다.
        LeaveMember,
        KickMember,
    }
}