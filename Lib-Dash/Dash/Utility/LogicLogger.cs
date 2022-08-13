using Dash.Types;
using System;
using System.Runtime.CompilerServices;
using Common.Log;

namespace Dash.Utility
{
    // NLog.GlobalDiagnosticsContext 을 사용하는 방법도 있는데... 이 wrapper에서는 한줄만 사용할 거라서 사용하지 않음
    public class LogicLogger
    {
        Common.Log.ILogger _logger;
        #if !Common_NetCore
        public LogicLogger()
        {
            _logger = Common.Log.Logger.Instance;
        }
        #else
        [MethodImpl(MethodImplOptions.NoInlining)]
        public LogicLogger()
        {
            _logger = new Common.Log.NLogLogger(NLogUtility.GetCurrentClassLogger(3));
        }
        #endif

        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Info(ServiceAreaType areaType, ILoggable loggable, string message)
        {
            _logger.Info($"[{areaType}]" + loggable?.LogStr + message);
        }

        public void Info(ILoggable loggable, string message)
        {
            _logger.Info(loggable?.LogStr + message);
        }

        public void Warn(string message)
        {
            _logger.Warning(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }
        public void Error(ServiceAreaType areaType, ILoggable loggable, string message)
        {
            _logger.Error($"[{areaType}]" + loggable?.LogStr + message);
        }

        public void Error(ILoggable loggable, string message)
        {
            _logger.Error(loggable?.LogStr + message);
        }

        public void Fatal(Exception exception)
        {
            _logger.Fatal(exception);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(ServiceAreaType areaType, ILoggable loggable, string message)
        {
            _logger.Fatal($"[{areaType}]" + loggable?.LogStr + message);
        }

        public void Fatal(ILoggable loggable, Exception e)
        {
            _logger.Fatal(loggable?.LogStr + e + e.StackTrace);
        }

        public void Fatal(ILoggable loggable, string message)
        {
            _logger.Fatal(loggable?.LogStr + message);
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Trace(ServiceAreaType areaType, ILoggable loggable, string message)
        {
            _logger.Trace($"[{areaType}]" + loggable?.LogStr + message);
        }

        public void Trace(ILoggable loggable, string message)
        {
            _logger.Trace(loggable?.LogStr + message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(ServiceAreaType areaType, ILoggable loggable, string message)
        {
            _logger.Debug($"[{areaType}]" + loggable?.LogStr + message);
        }

        public void Debug(ILoggable loggable, string message)
        {
            _logger.Debug(loggable?.LogStr + message);
        }
    }
}
