using Common.Utility;
using Dash.Model.Rdb;
using Dash.Server.Statistics;
using server_dash.Net.Sessions;
using server_dash.Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dash.Protocol;

namespace server_dash
{
    public class LogAndProgress
    {
        public ulong OidAccount { get; private set; }
        public int AccountLevel { get; private set; }
        private List<string> _gameLogs = new List<string>();
        private Dictionary<string, ulong> _progressChanges = new Dictionary<string, ulong>();
        public LogAndProgress(Account account)
        {
            OidAccount = account.OidAccount;
            AccountLevel = account.Level;
        }
        public LogAndProgress(PlayerSession session)
        {
            OidAccount = session.Player.OidAccount;
            AccountLevel = session.Player.AccountLevel;
        }

        public LogAndProgress(PartyMember partyMember)
        {
            OidAccount = partyMember.OidAccount;
            AccountLevel = partyMember.AccountLevel;
        }

        public void AddGameLog<T>(CommandType commandType, T payload, DateTime? now = null) where T : IPayload
        {
            DateTime _now = now ?? DateTime.UtcNow;
            GameLogWriter<T> writer = new GameLogWriter<T>()
            {
                OidAccount = OidAccount,
                Time = _now,
                CommandType = commandType,
                AccountLevel = AccountLevel,
                PayLoad = payload,
            };

            _gameLogs.Add(writer.ToJson());
        }
        public void AddGameLog(string gameLog)
        {
            _gameLogs.Add(gameLog);
        }
        public void AddGameLogs(List<string> gameLogs)
        {
            _gameLogs.AddRange(gameLogs);
        }
        public void AddChangeProgress(Dictionary<string, ulong> progress)
        {
            _progressChanges.Put(progress);
        }
        public void AddChangeProgress(string key, ulong value)
        {
            _progressChanges.Put(key, value);
        }
        public Dictionary<string, ulong> GetProgressChanges()
        {
            return _progressChanges;
        }
        public async Task Execute()
        {
            await Internal.Services.ProgressService.SetAll(OidAccount, _progressChanges);
            GameLogger.WriteLogs(_gameLogs);
        }
    }
}
