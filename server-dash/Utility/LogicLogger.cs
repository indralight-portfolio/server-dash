using System;
using Dash;
using Dash.Types;
using server_dash.Net.Sessions;

namespace server_dash.Utility
{
    // NLog.GlobalDiagnosticsContext 을 사용하는 방법도 있는데... 이 wrapper에서는 한줄만 사용할 거라서 사용하지 않음
    public class LogicLogger
    {
        NLog.Logger _logger;
        public LogicLogger(NLog.Logger logger)
        {
            _logger = logger;
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Info(ServiceAreaType areaType, ISession session, string message)
        {
            _logger.Info($"[{areaType}]" + Prefix(session) + message);
        }
        public void Info(ISession session, string message)
        {
            _logger.Info(Prefix(session) + message);
        }

        public void Info(ILoggable loggable, string message)
        {
            _logger.Info(loggable.LogStr + message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }
        public void Error(ServiceAreaType areaType, ISession session, string message)
        {
            _logger.Error($"[{areaType}]" + Prefix(session) + message);
        }
        public void Error(ISession session, string message)
        {
            _logger.Error(Prefix(session) + message);
        }

        public void Error(ILoggable loggable, string message)
        {
            _logger.Error(loggable.LogStr + message);
        }

        public void Fatal(Exception exception)
        {
            _logger.Fatal(exception);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(ServiceAreaType areaType, ISession session, string message)
        {
            _logger.Fatal($"[{areaType}]" + Prefix(session) + message);
        }
        public void Fatal(ISession session, string message)
        {
            _logger.Fatal(Prefix(session) + message);
        }

        public void Fatal(ISession session, Exception e)
        {
            _logger.Fatal(Prefix(session) + e + e.StackTrace);
        }

        public void Fatal(ILoggable loggable, string message)
        {
            _logger.Fatal(loggable.LogStr + message);
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Trace(ServiceAreaType areaType, ISession session, string message)
        {
            _logger.Trace($"[{areaType}]" + Prefix(session) + message);
        }
        public void Trace(ISession session, string message)
        {
            _logger.Trace(Prefix(session) + message);
        }

        public void Trace(ILoggable loggable, string message)
        {
            _logger.Trace(loggable.LogStr + message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(ServiceAreaType areaType, ISession session, string message)
        {
            _logger.Debug($"[{areaType}]" + Prefix(session) + message);
        }

        public void Debug(ISession session, string message)
        {
            _logger.Debug(Prefix(session) + message);
        }

        public void Debug(ILoggable loggable, string message)
        {
            _logger.Debug(loggable.LogStr + message);
        }

        // ISession이 ILoggable 상속받게하고, 하위 타입에서 override 해서 사용해도 될 듯.
        public static string Prefix(ISession session)
        {
            if(session.AreaType == ServiceAreaType.Client)
            {
                return $"[area:{session.AreaType}]" +
                session.OidAccount.LogOid() +
                $"[room:{session.ServiceExecutorId}] ";
            }
            else
            {
                return $"[area:{session.AreaType}]" +
                session.OidAccount.LogOid() +
                $"[executor:{session.ServiceExecutorId}] ";
            }
        }
    }
}
