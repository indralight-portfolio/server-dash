namespace Dash
{
    public readonly struct CoreConfig
    {
        public CoreConfig(
            bool useComponentPooling,
            bool useEntityPooling,
            bool headlessMode,
            bool cheatEnable,
            bool replayMode)
        {
            UseComponentPooling = useComponentPooling;
            UseEntityPooling = useEntityPooling;
            HeadlessMode = headlessMode;
            CheatEnable = cheatEnable;
            ReplayMode = replayMode;
        }

        public readonly bool UseComponentPooling;
        // 현재 MonsterEntity만 해당 Option으로 Pooling이 되는데, MonsterEntity의 크기가 작아서 별로 의미가 없다.
        public readonly bool UseEntityPooling;
        public readonly bool HeadlessMode;
        public readonly bool CheatEnable;
        public readonly bool ReplayMode;
    }
}