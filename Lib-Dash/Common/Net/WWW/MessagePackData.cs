using Newtonsoft.Json;

namespace Common.Net.WWW
{
    public class MessagePackData
    {
        private object _object;
        private byte[] _data;
        public byte[] Data => _data;
        public string JsonString => JsonConvert.SerializeObject(_object);

        private MessagePackData()
        {
        }

        public static MessagePackData Make<T>(T obj)
        {
            var data = MessagePack.MessagePackSerializer.Serialize(obj);
            var messagePackData = new MessagePackData
            {
                _object = obj,
                _data = data,
            };
            return messagePackData;
        }
    }
}