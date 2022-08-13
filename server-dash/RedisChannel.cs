using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Dash.Server.Dao.Cache;
using server_dash.Protocol;
using StackExchange.Redis;

namespace server_dash
{
    // Channel 전용 Redis instance를 구성해야 할 수도 있음. 부하 체크 필요.
    public abstract class RedisChannel
    {
        public const string BattleToMatchServer = "B->M";
        public const string MatchToBattleServer = "M->B";

        public readonly string ListenChannelId;
        public readonly string BroadcastChannelId;

        private ISubscriber subscriber;
        private Dictionary<int, List<Action<IJsonProtocol>>> _onMessage = new Dictionary<int, List<Action<IJsonProtocol>>>();

        protected abstract Logger logger { get; }


        public RedisChannel(string listenChannelId, string broadcastChannelId)
        {
            ListenChannelId = listenChannelId;
            BroadcastChannelId = broadcastChannelId;
            subscriber = DaoCache.Instance.RedisClient.GetSubscriber();
            subscriber.Subscribe(listenChannelId, (channel, message) =>
            {
                OnRawMessage(message);
            });
        }

        public void AddListener<T>(Action<T> protocol) where T : class, IJsonProtocol, new()
        {
            int typeHash = new T().GetTypeCode();
            if (_onMessage.ContainsKey(typeHash) == false)
            {
                _onMessage.Add(typeHash, new List<Action<IJsonProtocol>>());
            }

            _onMessage[typeHash].Add((message) => protocol(message as T));
        }

        protected void OnRawMessage(RedisValue rawMessage)
        {
            IJsonProtocol message = JsonProtocolSerializer.Deserialize(rawMessage.ToString());
            OnMessage(message);
        }

        protected virtual void OnMessage(IJsonProtocol jsonProtocol)
        {
            if (_onMessage.ContainsKey(jsonProtocol.GetTypeCode()) == true)
            {
                var callbacks = _onMessage[jsonProtocol.GetTypeCode()];
                for (int i = 0; i < callbacks.Count; ++i)
                {
                    callbacks[i](jsonProtocol);
                }
            }
            else
            {
                logger.Info($"[{ListenChannelId}] Protocol handler not registered, {jsonProtocol}");
            }
        }

        public void Send<T>(T message) where T : IJsonProtocol
        {
            subscriber.Publish(BroadcastChannelId, JsonProtocolSerializer.Serialize(message));
        }
    }
}