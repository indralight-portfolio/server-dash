namespace Dash.Types
{
    public static class EnumExtensions
    {
        public static bool IsSet(this AttackAttribute self, AttackAttribute flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this AuthorityType self, AuthorityType flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this ActorAttribute self, ActorAttribute flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this ColliderOptionType self, ColliderOptionType flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this AreaTriggerIgnoreType self, AreaTriggerIgnoreType flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this PushOption self, PushOption flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this GeneratorFlag self, GeneratorFlag flag)
        {
            return (self & flag) == flag;
        }

        public static bool IsSet(this AliveFlag self, AliveFlag flag)
        {
            return (self & flag) == flag;
        }
    }
}