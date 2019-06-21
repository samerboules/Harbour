using System;
using System.Threading;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Utilities
{
    public delegate void DisconnectedEvent(IConnection connection);
    public delegate void MessageReceivedEvent(byte[] message);

    public interface IConnection : IDisposable
    {
        event DisconnectedEvent Disconnected;
        event MessageReceivedEvent MessageReceived;
        int ConnectionId { get; }
        void Close();
        void DisconnectAsync();
        Task SendAsync(byte[] message, CancellationToken token = default(CancellationToken));
    }
}