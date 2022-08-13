using Amazon.Extensions.NETCore.Setup;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace server_dash.AWS
{
    public interface IDynamoDBClientFactory : IDisposable
    {
        IDynamoDBClientFactory AddClient(string key, AWSOptions option);
        IAmazonDynamoDB GetClient(string key);
    }

    public class DynamoDBClientFactory : IDynamoDBClientFactory
    {
        public static DynamoDBClientFactory Instance { get; private set; }
        public static IDynamoDBClientFactory Init()
        {
            var factory = new DynamoDBClientFactory();
            Instance = factory;
            return factory;
        }

        private bool _disposed = false;
        private IDictionary<string, IAmazonDynamoDB> _container = null;

        private DynamoDBClientFactory()
        {
            _container = new Dictionary<string, IAmazonDynamoDB>();
        }

        public IDynamoDBClientFactory AddClient(string key, AWSOptions option)
        {
            var factory = Instance;
            factory._container.Add(key, option.CreateServiceClient<IAmazonDynamoDB>());
            return factory;
        }

        public IAmazonDynamoDB GetClient(string key = "Default")
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
