using System;
using Dash.Net.Client;
using Dash.Types;

namespace Dash.Net
{
    public class NetworkAliveChecker
    {
        public event Action OnFine;
        public event Action OnWarning;
        public event Action OnError;       

        private INetSender _netSender;

        private bool _started;
        private int _lastAliveTime;
        private bool _warningAlarmed;
        private bool _errorAlarmed;
        private ServiceAreaType _serviceAreaType;
        public NetworkAliveChecker(INetSender netSender, ServiceAreaType serviceAreaType)
        {
            _netSender = netSender;
            _serviceAreaType = serviceAreaType;
        }
        public void Reconnect(INetSender netSender)
        {
            Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker]{_serviceAreaType} Reconnect");
            _netSender = netSender;
            OnMarkAlive();
        }


        public void Start()
        {
            Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker]{_serviceAreaType} Start");
            if (_started == true)
            {
                return;
            }
            _started = true;
            _lastAliveTime = Environment.TickCount;
        }

        public void OnMarkAlive()
        {
            
            if (_started == false)
            {
                Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker]{_serviceAreaType} OnMarkAlive Started false");
                return;
            }
            _lastAliveTime = Environment.TickCount;
            if (_warningAlarmed == true || _errorAlarmed == true)
            {
                OnFine?.Invoke();
            }

            _warningAlarmed = false;
            _errorAlarmed = false;
        }

        public void Update(float deltaTime)
        {
            if (_started == false)
            {
                Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker] Not Started");
                return;
            }

            if (_netSender.ManuallyDisconnected == true)
            {
                Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker] ManuallyDisconnected");
                return;
            }

            if (_netSender.IsConnected == false)
            {
                _warningAlarmed = true;
                _errorAlarmed = true;
                OnError?.Invoke();
            }
            
            int elapsedSinceAlive = Environment.TickCount - _lastAliveTime;

            if (elapsedSinceAlive > Constant.WarningThresholdMs && _warningAlarmed == false)
            {
                Common.Log.Logger.Instance.Debug($"[NetworkAliveChecker]{_serviceAreaType} Update {elapsedSinceAlive}, {_lastAliveTime}");
                _warningAlarmed = true;
                OnWarning?.Invoke();
            }

            if (elapsedSinceAlive > Constant.ErrorThresholdMs && _errorAlarmed == false)
            {
                _errorAlarmed = true;
                OnError?.Invoke();
            }
        }
    }
}