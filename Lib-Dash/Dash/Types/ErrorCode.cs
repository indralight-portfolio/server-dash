namespace Dash.Types
{
    public enum ErrorCode : int
    {
        Success = 0,

        // Http 관련 에러
        HttpError,
        AccessDenied,
        InvalidRequest,
        InvalidHeader,
        InvalidBody,
        InvalidSession,
        DuplicatedSession,
        InvalidVersion,

        OutOfService,

        // 로직 에러
        InvalidParameter,
        InternalError,
        DbError,
        InvalidData, // 잘못된 데이터로 요청했을때
        NoData,
        Duplicated,
        Expired,
        NotEnough,
        AlreadyProcessed,
        NoAuthorized,
        UnVerified,

        // BillingError
        CancelState,

        // Lobby 에러
        NoAuth,
        NoAccount,
        NotUsableNickname,
        GiveRewardFailed,

        // Match 관련 에러
        NoBattleServer,
        NoSocialServer,
        AlreadyInMatchQueue,
        StillInMatchQueue,
        ProcessParty,
        NotEnqueue,

        // Arena 관련 에러
        ArenaNoExist,

        // Game 관련 에러
        NotEnoughGold,
        NotEnoughJewel,
        LackOfCost,
        OverLimit,
        TimeOut,

        //EpisodeError
        NotInPeriod,
        NotInPeriod_Conquest,
        NotEnoughTicket,
        NoEpisodeLicense,
        NotEnoughStar,
        NotClearSweepEpisode,
        EpisodeEntryLimit,

        //PartyError
        PartyFull,
        PartySetTimer,
        AlreadyJoinParty,
        PartyNoExist,
        NotPartyJoined,
        AlreadyPlay,

        //ShopError
        GiveShopProductFailed,
        UnmatchedCondition,

        //MailErro
        AlreadyRewardReceived,
        //Friend
        InviterMaxFriendCount,
        InviterMaxInviteCount,
        InviteeMaxFriendCount,
        InviteeMaxInviteCount,
        AlreadyFriend,
    }

    public static class ErrorCodeExtension
    {
        public static bool IsSuccess(this ErrorCode errorCode)
        {
            return errorCode == ErrorCode.Success;
        }
    }
}