#if Common_NetCore
using DotNetty.Common.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dash.Net;
using Dash.Types;
using server_dash.Net.Handlers;

namespace server_dash.Execution.Runnable
{
    public class ServiceExecuteMultiplexer
    {
        private static readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public ServiceExecutor PrimaryExecutor { get; }
        public UnsafeBufferPool PrimaryUnsafeBufferPool { get; }

        private readonly ServiceExecutor[] _executors;
        private UnsafeBufferPool[] _unsafeBufferPools;
        private readonly int _coreCount = 0;

        public ServiceExecuteMultiplexer(ServiceAreaType serviceAreaType, MessageHandlerProvider provider, int coreCount = 0)
        {
            _coreCount = coreCount;
            if (_coreCount <= 0)
            {
                // core * 2 + 1 이 최적화라는데 일단은 논리 processor * 2 갯수로 설정.
                _coreCount = Environment.ProcessorCount * 2;
            }

            string alias = $"{serviceAreaType.ToString()[0]}:";

            PrimaryExecutor = new ServiceExecutor($"{alias}P", provider);
            _executors = Enumerable.Range(0, _coreCount)
                .Select(x => new ServiceExecutor($"{alias}{x}", provider))
                .ToArray();

            PrimaryUnsafeBufferPool = new UnsafeBufferPool();
            _unsafeBufferPools = Enumerable.Range(0, _coreCount)
                .Select(x => new UnsafeBufferPool())
                .ToArray();

            _logger.Info($"[ServiceExecuteMultiplexer] {_coreCount} Executors ready.");
        }

        public void Execute(int id, IRunnableMessage runnable)
        {
            GetExecutor(id).Execute(id, runnable);
        }

        public ServiceExecutor GetExecutor(IServiceExecutionContext context)
        {
            return GetExecutor(context.ServiceExecutorId);
        }

        public ServiceExecutor GetExecutor(int id)
        {
            return _executors[id % _coreCount];
        }

        public UnsafeBufferPool GetUnsafeBufferPool(int id)
        {
            return _unsafeBufferPools[id % _coreCount];
        }

        public async Task ShutdownGracefullyAsync()
        {
            List<Task> terminateTask = new List<Task>();
            foreach (var executor in _executors)
            {
                terminateTask.Add(executor.ShutdownGracefully());
            }

            await Task.WhenAll(terminateTask.ToArray());
            Console.WriteLine("ServiceExecuteMultiplexer Terminated.");
        }
    }
}
#endif