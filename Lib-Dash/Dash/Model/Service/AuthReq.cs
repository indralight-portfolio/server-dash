using Dash.Types;
using Newtonsoft.Json;
using System;

namespace Dash.Model.Service
{
    [Serializable]
    [MessagePack.MessagePackObject()]
    public class AuthReq
    {
        [MessagePack.Key(0)]
        public string AuthId { get; set; }
        [MessagePack.Key(1)]
        public AuthType AuthType { get; set; }
        [MessagePack.Key(2)]
        public sbyte TimeOffset { get; set; }
        [MessagePack.Key(3)]
        public string Country { get; set; }

        public AuthReq() { }

        public AuthReq(string authId)
        {
            AuthId = authId;
#if UNITY_EDITOR
            AuthType = AuthType.UnityEditor;
#elif HIVE
            AuthType = AuthType.Hive;
#elif UNITY_IOS
            AuthType = AuthType.IOS;
#else
            AuthType = authId.StartsWith("D:") ? AuthType.Dummy : AuthType.UnityEditor;
#endif
        }

        public bool Equals(AuthReq other)
        {
            return AuthType.Equals(other.AuthType)
                   && AuthId.Equals(other.AuthId, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return $"{AuthType} / {AuthId}";
        }
    }
}