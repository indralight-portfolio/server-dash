using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Dash.Model;
using server_dash.AWS;
using server_dash.Internal.Services;
using server_dash.Net.Handlers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace server_dash.Battle.Services
{
    /// <summary>
    /// TODO: 현재 존재하는 Arena 갯수.
    /// </summary>
    public class MonitorService : AbstractMonitorService<BattleDesc>
    {
        private ChannelManager _channelManager;

        public MonitorService(string uuid, ChannelManager channelManager, BattleServerConfig config) : base(uuid, config.Port)
        {
            _channelManager = channelManager;
        }

        private void UpdateThread(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                Thread.Sleep(1000);
                base.QueueUpdateDesc((desc) =>
                {
                    desc.CCU = _channelManager.GetLoggedInChannelsCount() +
                          _channelManager.GetNotLoggedInChannelsCount();

                });
                A();
            }
        }

        protected override void OnServiceStart(CancellationToken stoppingToken)
        {
            base.OnServiceStart(stoppingToken);
            AddSubTask(
                Task.Factory.StartNew(() => UpdateThread(stoppingToken), TaskCreationOptions.LongRunning)
            );
        }

        private void A()
        {
            var cwClient = CloudWatchClientFactory.Instance.GetClient();

            var request = new PutMetricDataRequest
            {
                MetricData = new List<MetricDatum>{new MetricDatum{
                    MetricName = "TestCount",
                    Dimensions = new List<Dimension> { new Dimension{Name = "HOSTNAME", Value="INDRA-ALLM" } },
                    Unit = StandardUnit.Count,
                    Value = Common.Utility.Random.Range(10, 10000),
                    TimestampUtc = DateTime.UtcNow,
                }},
                Namespace = "allm/dash",
            };

            cwClient.PutMetricDataAsync(request);
        }
    }
}