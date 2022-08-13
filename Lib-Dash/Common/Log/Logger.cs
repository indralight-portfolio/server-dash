#if Common_Unity
using UnityEngine;
#elif Common_NetCore
using NLog;
#endif

namespace Common.Log
{
    public interface ILogger
    {
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Fatal(string message);
        void Fatal(System.Exception exception);
    }

    public static class Logger
    {
        public static ILogger Instance => _logger;
        private static ILogger _logger;

        static Logger()
        {
            #if Common_Unity
            _logger = new UnityLogger();
            #elif Common_NetCore
            _logger = new NLogLogger(null);
            #else
             _logger = new ConsoleLogger();
            #endif
        }

        public static void SetInstance(ILogger logger)
        {
            _logger = logger;
        }

        public static void Debug(string message)
        {
            _logger.Debug(message);
        }

        public static void Info(string message)
        {
            _logger.Info(message);
        }

        public static void Warning(string message)
        {
            _logger.Warning(message);
        }        

        public static void Error(string message)
        {
            _logger.Error(message);
        }

        public static void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public static void Fatal(System.Exception exception)
        {
            _logger.Fatal(exception.Message);
            _logger.Fatal(exception.StackTrace);
        }
    }

#if Common_Unity
    public class UnityLogger : ILogger
    {
        public void Trace(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Fatal(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Fatal(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
#elif Common_NetCore
    public class NLogLogger : ILogger
    {
        private NLog.ILogger _logger = null;

        public NLogLogger(NLog.ILogger logger)
        {
            _logger = logger;
            if (_logger == null)
            {
                _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
            }
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);

        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(System.Exception exception)
        {
            _logger.Fatal(exception);
        }
    }
#else
    public class ConsoleLogger : ILogger
    {
        public void Trace(string message)
        {
        }

        public void Debug(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Info(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Error(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Fatal(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Fatal(System.Exception exception)
        {
            System.Console.WriteLine(exception);
        }
    }
#endif
    public class NullLogger : ILogger
    {
        public void Trace(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warning(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal(System.Exception exception)
        {
        }
    }
}