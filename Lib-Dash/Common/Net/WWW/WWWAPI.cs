namespace Common.Net.WWW
{
    public enum RequestPolicyType
    {
        Undefined = 0,
        None,
        AfterResponse,
    }

    public enum PathVariableType
    {
        Undefined = 0,
        None,
        OidAccount,
        OidClan,
    }

    public enum ResponseContentType
    {
        Json = 0,
        MessagePack,
    }

    public class WWWAPI
    {
        public string Path { get; }
        public PathVariableType PathVariableType { get; }
        public ResponseContentType ResponseContentType { get; }
        public RequestPolicyType RequestPolicyType { get; }

        public WWWAPI(string path, PathVariableType pathVariableType,
            ResponseContentType responseContentType = ResponseContentType.MessagePack,
            RequestPolicyType requestPolicyType = RequestPolicyType.AfterResponse)
        {
            Path = path;
            PathVariableType = pathVariableType;
            ResponseContentType = responseContentType;
            RequestPolicyType = requestPolicyType;
        }

        public static implicit operator string(WWWAPI api)
        {
            return api.Path;
        }

        public override string ToString() => Path;
    }
}