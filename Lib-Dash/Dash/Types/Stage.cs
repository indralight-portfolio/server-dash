namespace Dash.Types
{
    public enum SectorType
    {
        Undefined = 0,
        Normal,
        Angel,
        Boss,
        Wave,
        Clear,
        ChangeNextDeck,
    }

    public enum StageSize
    {
        Undefined = 0,
        Size_11 = 11,
        Size_13 = 13,
        Size_15 = 15,
        Size_17 = 17,
        Size_21 = 21,
        Size_31 = 31,
    }

    public enum StageWidth
    {
        Undefined = 0,
        Width_11 = 11,
        Width_13 = 13,
        Width_15 = 15,
        Width_17 = 17,
        Width_21 = 21,
        Width_31 = 31,
    }

    public enum StageType
    {
        Undefined = 0,
        Empty,          //  tile 1 summon 0
        Normal,         //  tile 1 summon 1
        Boss,           //  tile 1 summon 1
        Wave,           //  tile 1 summon list
        BossRandom,     //  tile list summon list
    }

    public enum NpcType
    {
        Undefined = 0,
        Angel,
        GoldChest,
        RewardChest,
    }

    public enum TileType
    {
        Undefined = 0,
        Field,
        Gate,
        Wall,
        Wall3,
        Water,
        Thorn,
        Portal,
        Rail,
        Laser,
        Saw,
        WaveTrigger,
    }
    public enum SystemTileType
    {
        Undefined = 0,
        Left,
        Right,
        Bottom,
        TopSecond,
        TopLeft,
        TopRight,
        TopGateBlock,
    }

    public enum StageEventTimingType
    {
        Undefined = 0,
        StageEnter,
        StageReady,
        StageClear,
    }

    public enum StageEventType
    {
        Undefined = 0,
        ActorIntro,
    }

    public enum BlockNextStageReasonType
    {
        Undefined = 0,
        AbilitySelect,
        Angel,
        Devil,
        GoldChest,
        RewardChest,
        Editor,
    }
}