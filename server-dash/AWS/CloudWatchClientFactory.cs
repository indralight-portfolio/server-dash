using Amazon.CloudWatch;
using Amazon.Extensions.NETCore.Setup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace server_dash.AWS
{
    public interface ICloudWatchClientFactory : IDisposable
    {
        ICloudWatchClientFactory AddClient(string key, AWSOptions option);
        IAmazonCloudWatch GetClient(string key);
    }

    public class CloudWatchClientFactory : ICloudWatchClientFactory
    {
        public static CloudWatchClientFactory Instance { get; private set; }
        public static ICloudWatchClientFactory Init()
        {
            var factory = new CloudWatchClientFactory();
            Instance = factory;
            return factory;
        }

        private bool _disposed = false;
        private IDictionary<string, IAmazonCloudWatch> _container = null;

        private CloudWatchClientFactory()
        {
            _container = new Dictionary<string, IAmazonCloudWatch>();
        }

        public ICloudWatchClientFactory AddClient(string key, AWSOptions option)
        {
            var factory = Instance;
            factory._container.Add(key, option.CreateServiceClient<IAmazonCloudWatch>());
            return factory;
        }

        public IAmazonCloudWatch GetClient(string key = "Default")
        {
            return _container[key];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_container != null && _container.Any())
                    {
                        foreach (var client in _container)
                        {
                            if (client.Value != null)
                            {
                                client.Value.Dispose();
                            }
                        }
                    }
                    _disposed = true;
                }
            }
        }
    }
}
