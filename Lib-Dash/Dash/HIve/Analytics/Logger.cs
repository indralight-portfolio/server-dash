#if Common_Server
using Common.Log;
using Common.Net.WWW;
using Dash.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Dash.Server.HiveConfig;

namespace Dash.Hive.Analytics
{
    public static class Logger
    {
        private static readonly NLog.Logger _logger = NLogUtility.GetCurrentClassLogger();
        private static NLog.Logger _fileLogger = NLog.LogManager.GetLogger("GameLogger");

        public static ConcurrentQueue<Payload> _queue = new ConcurrentQueue<Payload>();
        private const int FlushCount = 100;
        public static HiveConfig HiveConfig { get; private set; }
        public static LoggerConfig LoggerConfig => HiveConfig.Logger;

        public static void Init(HiveConfig hiveConfig)
        {
            HiveConfig = hiveConfig;
        }

        public static void Add<T>(T payload) where T : Payload
        {
            if (HiveConfig == null) return;
            if (LoggerConfig.Type == LoggerConfig.WriterType.Hive)
            {
                _queue.Enqueue(payload);
                _logger.Info($"[Hive][Logger] Add");
                if (_queue.Count >= FlushCount)
                {
                    AsyncTaskWrapper.Call(Flush());
                }
            }
            else if (LoggerConfig.Type == LoggerConfig.WriterType.File && _fileLogger != null)
            {
                _fileLogger?.Info(JsonConvert.SerializeObject(payload));
                return;
            }
        }

        public static async Task Flush()
        {
            _logger.Info($"[Hive][Logger] Flush");
            List<Payload> payloads1 = new List<Payload>();
            List<Payload> payloads2 = new List<Payload>();
            while (_queue.TryDequeue(out var payload))
            {
                if (payload.appId == Constant.APP_ID_AP)
                    payloads2.Add(payload);
                else
                    payloads1.Add(payload);
            }
            await Send(Constant.APP_ID_GO, payloads1);
            await Send(Constant.APP_ID_AP, payloads2);
        }

        private static async Task Send(string appId, List<Payload> payloads)
        {
            if (HiveConfig == null) return;
            if (payloads.Count == 0) return;

            JObject logData = new JObject();
            logData.Add("appId", appId);
            logData.Add("logBody", JArray.FromObject(payloads));

            string url;
            switch (HiveConfig.Zone)
            {
                case ZoneType.REAL:
                    url = Constant.Analytics.REAL_URL;
                    break;
                case ZoneType.SANDBOX:
                default:
                    url = Constant.Analytics.SANDBOX_URL;
                    break;
            }
            var headers = new Dictionary<string, string>()
            {
                //{ HeaderConstants.ContentTypeHeader, "application/json; charset=utf8" },
            };

            try
            {
                using HttpClient httpClient = new HttpClient();
                var result = await httpClient.PostStringAsync(url, logData, headers);
                _logger.Info($"[Hive][Logger] Send : {result}");
            }
            catch (Exception e)
            {
                _logger.Error($"[Hive][Logger] {e.Message}");
            }
        }
    }
}
#endif