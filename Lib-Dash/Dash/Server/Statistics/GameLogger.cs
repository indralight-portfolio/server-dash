#if Common_Server
using Dash.Server.Statistics.GameLogWriter;
using System;
using System.Collections.Generic;

namespace Dash.Server.Statistics
{
    public interface IGameLogWriter
    {
        void Write(GameLogContext log);
        void Write(List<GameLogContext> logs);
    }
    
    public interface IHasDBContext
    {
        void Init(Dao.LogDBContext logDBContext);
    }

    public static class GameLogger
    {
        private static GameLogConfig _config;
        private static IGameLogWriter _logWriter;
        public static IGameLogWriter LogWriter;
        public static void Init(GameLogConfig config)
        {
            _config = config;
            _logWriter = CreateWriter();
        }
#if Admin_Server
        public static void InitDBContext(Dao.LogDBContext logDBContext)
        {
            if(_logWriter is IHasDBContext hasDBContext)
            {
                hasDBContext.Init(logDBContext);
            }
        }
#endif
        static IGameLogWriter CreateWriter()
        {
            switch (_config.Type)
            {
                case GameLogConfig.WriterType.MySql:
                    return new MySqlGameLogWriter();
                case GameLogConfig.WriterType.File:
                    return new FileGameLogWriter();
                default:
                    return new NullGameLogWriter();
            }
        }

        public static void Write(ulong oidAccount, int accountLevel, CommandType commandType, IPayload payload, DateTime? now = null)
        {
            DateTime _now = now ?? DateTime.UtcNow;
            GameLogContext log = new GameLogContext()
            {
                OidAccount = oidAccount,
                Time = _now,
                CommandType = commandType,
                AccountLevel = accountLevel,
                PayLoad = payload
            };
            Write(log);
        }

        public static void Write(GameLogContext log)
        {
            if (log != null)
                _logWriter?.Write(log);
        }

        public static void Write(List<GameLogContext> logs)
        {
            if (logs?.Count > 0)
                _logWriter?.Write(logs);
        }
    }
}
#endif