#if Common_NetCore
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Utility;
using NLog.LayoutRenderers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using NLog;

namespace Common.Log
{
    public static class NLogUtility
    {
        public static string MakeSimplifiedName(string fullStr)
        {
            var names = fullStr.Split('.');
            names = names.Select((x, i) => i < names.Length - 1 ? x.Substring(0, 1) : x).ToArray();
            string name = string.Join('.', names);
            return name;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NLog.Logger GetCurrentClassLogger(int skipFrame = 2)
        {
            string name = MakeSimplifiedName(GetCurrentClassFullname(skipFrame));
            var logger = LogManager.GetLogger(name);
            return logger;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCurrentClassFullname(int skipFrame)
        {
            var stackFrame = new StackFrame(skipFrame, false);
            string str = stackFrame.GetMethod()?.DeclaringType?.FullName;
            if (string.IsNullOrEmpty(str) == true)
            {
                throw new Exception("Make class name failed!");
            }

            return str;
        }

        public static void SetUp(string nlogConfigPath)
        {
            LayoutRenderer.Register("level", typeof(LevelLayoutRenderer));
            LayoutRenderer.Register("logger", typeof(LoggerNameLayoutRenderer));
            LayoutRenderer.Register("threadid", typeof(ThreadLayoutRenderer));

            LogManager.LoadConfiguration(nlogConfigPath);
        }
    }

    [LayoutRenderer("threadid")]
    public class ThreadLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var thread = Thread.CurrentThread;
            string str = $"{thread.ManagedThreadId} {thread.Name}";

            // 최적화를 위해.
            if (str.Length < 2)
            {
                builder.Append(' ', 2);
                builder.Append(str);
            }
            else if (str.Length < 3)
            {
                builder.Append(' ');
                builder.Append(str);
            }
            else
            {
                builder.Append(str);
            }
        }
    }

    [LayoutRenderer("level")]
    public class LevelLayoutRenderer : LayoutRenderer
    {
        private static readonly Dictionary<LogLevel, string> _upperStrs = new Dictionary<LogLevel, string>()
        {
            {LogLevel.Trace, "TRACE"},
            {LogLevel.Debug, "DEBUG"},
            {LogLevel.Info, " INFO"},
            {LogLevel.Warn, " WARN"},
            {LogLevel.Error, "ERROR"},
            {LogLevel.Fatal, "FATAL"},
            {LogLevel.Off, "  OFF"}
        };

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            LogLevel level = GetValue(logEvent);
            builder.Append(_upperStrs[level]);
        }

        private LogLevel GetValue(LogEventInfo logEvent)
        {
            return logEvent.Level;
        }
    }

    [LayoutRenderer("logger")]
    public class LoggerNameLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string name = logEvent.LoggerName;
            if (name.Length < 30)
                name = (new string(' ', 30) + name).Right(30);
            else
            {
                int space = (10 - name.Length % 10) % 10;
                name = new string(' ', space) + name;
            }

            builder.Append(name);
        }
    }
}
#endif