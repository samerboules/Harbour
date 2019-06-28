using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Utilities
{
    public class TcpServer : IServer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(10);
        private readonly SemaphoreSlim _syncSend = new SemaphoreSlim(1, 1);
        private readonly object _syncLock = new object();
        private readonly TcpListener _listener;
        private int _nextId = 0;
        private ConcurrentDictionary<int, IConnection> _connections = new ConcurrentDictionary<int, IConnection>();
        private Task _connectionAcceptor = Task.CompletedTask;

        public event ClientConnectedEvent ClientConnected;
        public event ClientDisconnectedEvent ClientDisconnected;
        public event ClientMessageReceivedEvent ClientMessageReceived;

        public TcpServer(int port = 0)
        {
            _listener = new TcpListener(
                new IPEndPoint(
                    new IPAddress(0),
                    port));
        }

        public int LocalPort => Running ? ((IPEndPoint)_listener.LocalEndpoint).Port : -1;

        public bool Running => !_connectionAcceptor.IsCompleted;

        public void Start()
        {
            lock (_syncLock)
            {
                if (!Running)
                {
                    _listener.Start();
                    _connectionAcceptor = AcceptConnections();
                    log.Info($"Listening on port [{LocalPort}]");
                }
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                if (Running)
                {
                    _listener.Stop();
                    foreach (var pair in _connections)
                    {
                        pair.Value.DisconnectAsync();
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public async Task SendBroadcastAsync(byte[] body)
        {
            try
            {
                await _syncSend.WaitAsync();

                var tasks = new ConcurrentDictionary<int, Task>();
                using (var timeoutCancellation = new CancellationTokenSource(DEFAULT_TIMEOUT))
                {
                    Parallel.ForEach(_connections.ToArray(), pair =>
                    {
                        try
                        {
                            tasks[pair.Key] = pair.Value.SendAsync(body, timeoutCancellation.Token);
                        }
                        catch (SystemException)
                        {
                            TryRemoveDisconnectedClient(pair.Key);
                        }
                    });

                    foreach (var pair in tasks)
                    {
                        try
                        {
                            await pair.Value;
                        }
                        catch (SystemException)
                        {
                            TryRemoveDisconnectedClient(pair.Key);
                        }
                    }
                }
            }
            finally
            {
                _syncSend.Release();
            }
        }

        public async Task<bool> TrySendAsync(int connectionId, byte[] body)
        {
            try
            {
                await _syncSend.WaitAsync();

                if (_connections.TryGetValue(connectionId, out var connection))
                {
                    try
                    {
                        using (var timeoutCancellation = new CancellationTokenSource(DEFAULT_TIMEOUT))
                        {
                            await connection.SendAsync(body, timeoutCancellation.Token);
                        }
                        return true;
                    }
                    catch (SystemException)
                    {
                        TryRemoveDisconnectedClient(connectionId);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                _syncSend.Release();
            }
        }

        private async Task AcceptConnections()
        {
            log.Info("Server started");

            while (true)
            {
                try
                {
                    int connectionId = Interlocked.Increment(ref _nextId);
                    IConnection newConnection = new TcpConnection(connectionId, await _listener.AcceptTcpClientAsync());
                    _connections[connectionId] = newConnection;

                    newConnection.Disconnected += Connection_Disconnected;
                    newConnection.MessageReceived += Connection_MessageReceived;

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            ClientConnected?.Invoke(connectionId);
                        }
                        catch (SystemException ex)
                        {
                            log.Error(ex.Message, ex);
                        }
                    });
                }
                catch (ObjectDisposedException)
                {
                    log.Info("Server stopped");
                    break;
                }
                catch (SystemException ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }

        private void Connection_MessageReceived(byte[] message)
        {
            _ = Task.Run(() =>
            {
                ClientMessageReceived?.Invoke(message);
            });
        }

        private void Connection_Disconnected(IConnection connection)
        {
            _ = Task.Run(() => ClientDisconnected?.Invoke(connection.ConnectionId));
            TryRemoveDisconnectedClient(connection.ConnectionId);
        }

        private void TryRemoveDisconnectedClient(int connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connection))
            {
                connection.Disconnected -= Connection_Disconnected;
                connection.MessageReceived -= Connection_MessageReceived;
                connection.Close();
            }
        }
    }
}
