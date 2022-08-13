using Dash.Types;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace server_dash
{
    public class ServiceStateValidator
    {
        private CheckConfig _checkConfig;
        private ServiceState serviceState = ServiceState.Close;
        public ServiceState ServiceState
        {
            get { return serviceState; }
            set { serviceState = value; }
        }

        public ServiceStateValidator(CheckConfig checkConfig)
        {
            _checkConfig = checkConfig;
        }

        public bool Validate(string clientIP)
        {
            if (serviceState == ServiceState.Open)
                return true;
            else if (serviceState == ServiceState.TesterOnly)
            {
                // TesterOnly 일 경우 IP체크
                bool check = _checkConfig.WhiteClientList.Contains(clientIP)
                    || _checkConfig.WhiteClientList.Contains("all")
                    //|| Common.Utility.NetUtility.IsLocalIP(clientIP)
                    ;

                if (check)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class ServiceStateResolveService : BackgroundService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private readonly float _intervalSeconds = 5.0f;

        private ServiceStateValidator _serviceStateValidator;
        private CheckConfig _checkConfig;
        private HttpClient httpClient;

        public ServiceStateResolveService(CheckConfig checkConfig, ServiceStateValidator serviceStateValidator)
        {
            _checkConfig = checkConfig;
            _serviceStateValidator = serviceStateValidator;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("ServiceStateResolveService Started.");
            TimeSpan interval = TimeSpan.FromSeconds(_intervalSeconds);
            while (stoppingToken.IsCancellationRequested == false)
            {
                var url = _checkConfig.ServiceStateUrl;
                try
                {
                    var httpResponse = await httpClient.GetAsync(url);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                        dynamic dynObj = JsonConvert.DeserializeObject(rawResponse);

                        _serviceStateValidator.ServiceState = dynObj.ServiceState;
                    }
                }
                catch { }

                await Task.Delay(interval, stoppingToken);
            }

            _logger.Info("ServiceStateResolveService Stopped.");
        }
    }
}