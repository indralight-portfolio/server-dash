namespace Common.Model
{
    [MessagePack.MessagePackObject()]
    public class SessionKey
    {
        [MessagePack.Key(0)]
        public string Key;
        [MessagePack.Key(1)]
        public System.DateTime Expiry;
    }
}