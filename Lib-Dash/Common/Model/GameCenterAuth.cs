namespace Common.Model
{
    [MessagePack.MessagePackObject]
    public class GameCenterAuth
    {
        [MessagePack.Key(0)]
        public string PlayerId { get; set; }
        [MessagePack.Key(1)]
        public string BundleId { get; set; }
        [MessagePack.Key(2)]
        public string PublicKeyUrl { get; set; }
        [MessagePack.Key(3)]
        public string Signature { get; set; }
        [MessagePack.Key(4)]
        public string Salt { get; set; }
        [MessagePack.Key(5)]
        public ulong Timestamp { get; set; }

        public override string ToString()
        {
            return $"{nameof(PlayerId)}:{PlayerId}, " +
                $"{nameof(BundleId)}:{BundleId}, " +
                $"{nameof(PublicKeyUrl)}:{PublicKeyUrl}, " +
                $"{nameof(Signature)}:{Signature}, " +
                $"{nameof(Salt)}:{Salt}, " +
                $"{nameof(Timestamp)}:{Timestamp}";
        }
    }
}