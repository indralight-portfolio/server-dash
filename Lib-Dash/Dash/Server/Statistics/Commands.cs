#if Common_Server
using System;
using System.Collections.Generic;
using Common.Utility;

namespace Dash.Server.Statistics
{
    /// <summary>
    /// 아래 Commands._values에도 값 추가 필요.
    /// </summary>
    public enum CommandType
    {
        Undefined = 0,
#region Service
        Exp,
        Gain_Spend_Start = 100,
        Gold,
        Jewel,
        Stamina,
        Gain_Spend_End = 1000,
        Money,
        Equipment,
        Consume,
        Character,
        Login,
        Create,
        Reset,
        SeasonPass,
#endregion
#region Shop
        BoxOpen,
        Shop_Buy,
#endregion

#region Battle
        ArenaCreate,
        ArenaStart,
        ArenaEnd,
        StageEnd,
        SkillPick,
        ConquestScoreUpdate,
        WorldMissionScoreUpdate,
#endregion

#region AntiHack
        InvalidUndoneGame,
        PlayerHackReport,
#endregion

        HiveDelete,
    }

    public static class Commands
    {
        const string GAIN = "Gain";
        const string SPEND = "Spend";

        public static IReadOnlyDictionary<CommandType, string> Values => _values;
        private static Dictionary<CommandType, string> _values;

        static Commands()
        {
            _values = new Dictionary<CommandType, string>();
            foreach (var commandType in EnumInfo<CommandType>.GetValues())
            {
                if (commandType == CommandType.Gain_Spend_Start || commandType == CommandType.Gain_Spend_End)
                {
                    continue;
                }

                _values.Add(commandType, commandType.ToString());
            }
        }

        public static string GetValue(CommandType type)
        {
            return _values[type];
        }

        public static string GetValueWithDelta(CommandType type, int delta)
        {
            int intValue = (int)type;
            if ((int)CommandType.Gain_Spend_Start >= intValue ||
                (int)CommandType.Gain_Spend_End <= intValue)
            {
                throw new Exception($"{type} wrong range!");
            }

            return $"{GetValue(type)}:{(delta > 0 ? GAIN : SPEND)}";
        }

        public static List<string> GetCommandList()
        {
            var list = new List<string>();
            foreach (var v in _values)
            {
                CommandType commandType = v.Key;
                if ((int)commandType > (int)CommandType.Gain_Spend_Start && (int)commandType < (int)CommandType.Gain_Spend_End)
                {
                    list.Add(commandType.ToString() + ":" + GAIN);
                    list.Add(commandType.ToString() + ":" + SPEND);
                }
                else
                {
                    list.Add(commandType.ToString());
                }
            }
            return list;
        }
    }
}
#endif