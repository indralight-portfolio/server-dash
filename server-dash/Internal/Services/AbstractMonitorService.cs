using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Common.Log;
using Dash.Model;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Model;
using MessagePack;
using Microsoft.Extensions.Hosting;
using server_dash.AWS;
using server_dash.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace server_dash
{
    public class ServerUUID
    {
        public readonly string Value;
        public ServerUUID()
        {
            Value = Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}

namespace server_dash.Internal.Services
{
    public abstract class AbstractMonitorService<TDesc> : BackgroundService
        where TDesc : ServerDesc, new()
    {
        private static readonly NLog.Logger _logger = NLogUtility.GetCurrentClassLogger();

        private IMemCache _monitorCache;
        private TDesc _serverDesc;

        private List<Task> _subTasks = new List<Task>();
        private ConcurrentQueue<Action<TDesc>> _updateDescQueue = new ConcurrentQueue<Action<TDesc>>();

        private readonly ProcessMonitor _processMonitor = new ProcessMonitor();
        private readonly NetCoreProcessProfiler _netCoreProcessProfiler = new NetCoreProcessProfiler();
        private readonly float _intervalSeconds = 10.0f;

        protected AbstractMonitorService(string uuid, int port)
        {
            _monitorCache = MonitorCache.Instance.GetMemCache();

            _serverDesc = new TDesc
            {
                UUID = uuid,
                HostName = ServerIPManager.Instance.HostName,
                IP = ServerIPManager.Instance.IP,
                Endpoint = $"{ServerIPManager.Instance.Endpoint}:{port}",
                Version = BuildVersion.Version,
                RevServer = BuildVersion.RevServer,
                RevDash = BuildVersion.RevDash,
                RevData = BuildVersion.RevData,
            };
        }

        protected bool IsSubTasksEnded() => _subTasks.TrueForAll(t => t.IsCompleted);
        protected void AddSubTask(Task task) => _subTasks.Add(task);
        protected void QueueUpdateDesc(Action<TDesc> updateAction) => _updateDescQueue.Enqueue(updateAction);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info($"{this.GetType().Name} Started.");

            OnServiceStart(stoppingToken);

            while (stoppingToken.IsCancellationRequested == false || IsSubTasksEnded() == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            _logger.Info($"{this.GetType().Name} Stopped.");
        }

        protected virtual void OnServiceStart(CancellationToken stoppingToken)
        {
            AddSubTask(
                Task.Factory.StartNew(() => _processMonitor.Run(stoppingToken), TaskCreationOptions.LongRunning)
            );

            AddSubTask(_netCoreProcessProfiler.Run(stoppingToken));
            AddSubTask(TakeSnapshots(stoppingToken));

            var serverConfig = ConfigManager.Get<ServerConfig>(Config.Server);
            if (serverConfig.IsPrivate == false)
            {
                AddSubTask(UpdateDesc(stoppingToken));
                AddSubTask(RerpotGCEvent(stoppingToken));
            }
        }

        #region Tasks

        private async Task TakeSnapshots(CancellationToken stoppingToken)
        {
            TimeSpan logInterval = TimeSpan.FromHours(1);
            TimeSpan refreshInterval = TimeSpan.FromSeconds(10);
            TimeSpan logElapsed = logInterval;
            while (stoppingToken.IsCancellationRequested == false)
            {
                await Task.Delay(refreshInterval, stoppingToken);
                if (_processMonitor.IsSnapshotReady == false)
                {
                    continue;
                }

                var snapshots = _processMonitor.GetAverageSnapshots();
                logElapsed += refreshInterval;
                if (logElapsed > logInterval)
                {
                    logElapsed = default;
                    _logger.Info($"[ProcessMonitor] Min : {snapshots.Min} Hour : {snapshots.Hour} Day : {snapshots.Day}");
                }
                QueueUpdateDesc((desc) =>
                {
                    desc.ProcessSnapshotsRaw =
                        MessagePackSerializer.Serialize(new[] { snapshots.Min, snapshots.Hour, snapshots.Day });
                });
            }
        }

        private async Task UpdateDesc(CancellationToken stoppingToken)
        {
            TimeSpan interval = TimeSpan.FromSeconds(_intervalSeconds);
            TimeSpan keyExpire = TimeSpan.FromSeconds(_intervalSeconds + 1);
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    // apply requested Desc update.
                    while (_updateDescQueue.TryDequeue(out var updateAction) == true)
                    {
                        updateAction(_serverDesc);
                    }

                    // redis update
                    _serverDesc.SetUpdated(DateTime.UtcNow);
                    var key = _serverDesc.GetMainKey();
                    var hashEntries = ModelConverter<TDesc>.ToHashEntries(_serverDesc);
                    await _monitorCache.HashSet(key, hashEntries);
                    await _monitorCache.KeyExpire(key, keyExpire);
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex.Message);
                    _logger.Fatal(ex.StackTrace);
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task RerpotGCEvent(CancellationToken stoppingToken)
        {
            TimeSpan interval = TimeSpan.FromSeconds(10);
            while (stoppingToken.IsCancellationRequested == false)
            {
                //_logger.Info($"HostName: {_serverDesc.HostName}, ServiceType: {_serverDesc.GetServiceAreaType()}");

                var cwClient = CloudWatchClientFactory.Instance.GetClient();
                var dimensions = new List<Dimension> {
                    new Dimension { Name = "InstancId", Value = _serverDesc.HostName },
                    new Dimension { Name = "Type", Value = _serverDesc.GetServiceAreaType().ToString() },
                };

                var data = GCEventTracer.GetLastData();
                var now = DateTime.UtcNow;

                Func<string, uint, MetricDatum> getMetricDatum = (name, value) =>
                {
                    return new MetricDatum { MetricName = name, Value = value, Dimensions = dimensions, Unit = StandardUnit.Megabytes, TimestampUtc = now };
                };

                var metricData = new List<MetricDatum>
                {
                    getMetricDatum(nameof(GCHeapStatsData.Gen0SizeMiB), data.Gen0SizeMiB),
                    getMetricDatum(nameof(GCHeapStatsData.Gen0PromotedMiB), data.Gen0PromotedMiB),
                    getMetricDatum(nameof(GCHeapStatsData.Gen1SizeMiB), data.Gen1SizeMiB),
                    getMetricDatum(nameof(GCHeapStatsData.Gen1PromotedMiB), data.Gen1PromotedMiB),
                    getMetricDatum(nameof(GCHeapStatsData.Gen2SizeMiB), data.Gen2SizeMiB),
                    getMetricDatum(nameof(GCHeapStatsData.Gen2SurvivedMiB), data.Gen2SurvivedMiB),
                    getMetricDatum(nameof(GCHeapStatsData.LohSizeMiB), data.LohSizeMiB),
                    getMetricDatum(nameof(GCHeapStatsData.LohSurvivedMiB), data.LohSurvivedMiB),
                };

                var request = new PutMetricDataRequest
                {
                    MetricData = metricData,
                    Namespace = "Allm/Dash",
                };

                cwClient.PutMetricDataAsync(request);

                await Task.Delay(interval, stoppingToken);
            }
        }
        #endregion
    }
}
