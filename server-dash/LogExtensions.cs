using DotNetty.Transport.Channels;

namespace server_dash
{
    public static class LogExtensions
    {
        public static string Log(this IChannelId channelId)
        {
            return $"[ch:@{channelId}]";
        }

        public static string Log(this IChannel channel)
        {
            return Log(channel?.Id);
        }
    }
}