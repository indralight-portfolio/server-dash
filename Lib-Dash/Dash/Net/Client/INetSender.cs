using System.Threading.Tasks;

namespace Dash.Net.Client
{
    public interface INetSender
    {
        void Send<T>(T message, bool isFlush = true) where T : Dash.Protocol.IProtocol;
        void Flush();
        Task<ConnectResult> Connect(string host, int port);
        Task Cleanup();
        bool IsConnected { get; }
        bool ManuallyDisconnected { get; }
    }
}