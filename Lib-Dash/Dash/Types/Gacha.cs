namespace Dash.Types
{
    //Db size 이슈로 10자 이내로 정해야 한다.
    public enum PityCategoryType
    {
        Undefined,
        Normal,
        Limited,
        LimitedWeapon,
        Newbie,
    }

    public enum FakeType
    {
        Undefined,
        None,
        //Epic
        RareToEpic,
        RareToEpicToEpic,
        EpicToEpic,
        //Legendary
        RareToLegendary,
        RareToEpicToLegendary,
        EpicToLegendary,
    }

    public enum FakeFlow
    {
        Undefined,
        None,
        Once,
        Step,
        StepRepeat,
        Repeat,
    }
}
