namespace Dash.Types
{
    public enum BridgeEstablishErrorType
    {
        Success = 0,
        NetworkError,
    }

    public enum ReturnToLobbyReasonType
    {
        Undefined = 0,
        InternalError,
        InvalidVersion,
        InvalidSession,
        DuplicatedSession,
        AccountNotExists,
        LackOfCost,
        CreateArenaFailed,
        StartUndoneGameFailed,
        MatchNotFound,
        NetworkError,
        ServerLogicError,
        OutOfService,
    }

    public enum JoinArenaResponseType
    {
        Undefined = 0,
        Wait,
        ArenaEnded,
        MaxJoinCount,
    }
}