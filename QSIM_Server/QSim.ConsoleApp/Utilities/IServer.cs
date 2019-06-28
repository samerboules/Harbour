using System;
using System.Net;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Utilities
{
    public delegate void ClientConnectedEvent(int connectionId);
    public delegate void ClientDisconnectedEvent(int connectionId);
    public delegate void ClientMessageReceivedEvent(byte[] message);

    public interface IServer : IDisposable
    {
        event ClientConnectedEvent ClientConnected;
        event ClientDisconnectedEvent ClientDisconnected;
        event ClientMessageReceivedEvent ClientMessageReceived;
        void Start();
        void Stop();
        bool Running { get; }
        Task SendBroadcastAsync(byte[] message);
        Task<bool> TrySendAsync(int connectionId, byte[] message);
    }
}