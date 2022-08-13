using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace server_dash.AWS
{
    public interface IS3ClientFactory : IDisposable
    {
        IS3ClientFactory AddClient(string key, AWSOptions option);
        IAmazonS3 GetClient(string key);
    }

    public class S3ClientFactory : IS3ClientFactory
    {
        public static S3ClientFactory Instance { get; private set; }
        public static IS3ClientFactory Init()
        {
            var factory = new S3ClientFactory();
            Instance = factory;
            return factory;
        }

        private bool _disposed = false;
        private IDictionary<string, IAmazonS3> _container = null;

        private S3ClientFactory()
        {
            _container = new Dictionary<string, IAmazonS3>();
        }

        public IS3ClientFactory AddClient(string key, AWSOptions option)
        {
            var factory = Instance;
            factory._container.Add(key, option.CreateServiceClient<IAmazonS3>());
            return factory;
        }

        public IAmazonS3 GetClient(string key = "Default")
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
