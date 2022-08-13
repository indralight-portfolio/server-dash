#if Common_Unity
using System;

namespace Common.Unity.Net.WWW
{
    public interface IHttpMessageHandler
    {
        void Handle(string rawMessage, int elapsedMs);
        void Handle(ReadOnlyMemory<byte> rawMessage, int elapsedMs);
    }
}
#endif