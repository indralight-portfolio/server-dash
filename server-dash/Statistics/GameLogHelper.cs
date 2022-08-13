using Dash.Model.Rdb;
using Dash.Server.Statistics;
using Dash.Types;
using Org.BouncyCastle.Crypto.Engines;
using System;

namespace server_dash.Statistics
{
    public static class GameLogHelper
    {
        public static string GetChangeValueString(Account account, CommandType commandType, ChangeReason reason, int newValue)
        {
            int oldValue;
            switch (commandType)
            {
                case CommandType.Gold:
                    oldValue = account.Gold;
                    break;
                case CommandType.Jewel:
                    oldValue = account.Jewel;
                    break;
                case CommandType.Stamina:
                    oldValue = account.Stamina;
                    break;
                default:
                    return string.Empty;
            }

            int delta = newValue - oldValue;
            GameLogWriter<ChangeValuePayload> writer = new GameLogWriter<ChangeValuePayload>
            {
                OidAccount = account.OidAccount,
                Time = DateTime.UtcNow,
                CommandType = commandType,
                AccountLevel = account.Level,
                PayLoad = new ChangeValuePayload
                {
                    Reason = reason.ToString(),
                    Delta = delta,
                    OldValue = oldValue,
                    NewValue = newValue,
                },
            };
            return writer.ToJson();
        }

        public static string GetChangeExpString(Account account, ChangeReason reason, int newExp, int newLevel)
        {
            int oldLevel = account.Level;
            int oldExp = account.Exp;
            int delta = newExp - oldExp;
            GameLogWriter<ChangeExpPayload> writer = new GameLogWriter<ChangeExpPayload>
            {
                OidAccount = account.OidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.Exp,
                AccountLevel = account.Level,
                PayLoad = new ChangeExpPayload
                {
                    Reason = reason.ToString(),
                    DeltaExp = delta,
                    OldExp = oldExp,
                    NewExp = newExp,
                    OldLevel = oldLevel,
                    NewLevel = newLevel
                },
            };
            return writer.ToJson();
        }
    }
}