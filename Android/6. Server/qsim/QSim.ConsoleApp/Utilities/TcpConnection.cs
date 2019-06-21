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
    public class TcpConnection : IConnection
    {
        public event DisconnectedEvent Disconnected;
        public event MessageReceivedEvent MessageReceived;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(10);
        private readonly TcpClient _tcpClient;

        public int ConnectionId { get; }

        public TcpConnection(int connectionId, TcpClient tcpClient)
        {
            ConnectionId = connectionId;
            _tcpClient = tcpClient;

            _ = ListenForData(tcpClient);
        }

        public static async Task<IConnection> ConnectAsync(IPEndPoint endPoint, TimeSpan? timeout = null)
        {
            var client = new TcpClient();

            var task = client.ConnectAsync(endPoint.Address, endPoint.Port);

            if (task != await Task.WhenAny(task, Task.Delay(timeout ?? DEFAULT_TIMEOUT)))
            {
                client.Close();
                throw new TimeoutException($"Connection attempt to [{endPoint}] timed out after waiting for {(int)DEFAULT_TIMEOUT.TotalSeconds} seconds.");
            }

            return new TcpConnection(-1, client);
        }

        public void Close()
        {
            _tcpClient.Close();
        }

        public void Dispose()
        {
            Close();
        }

        public void DisconnectAsync()
        {
            _tcpClient.Client.DisconnectAsync(new SocketAsyncEventArgs() { DisconnectReuseSocket = false });
        }

        public Task SendAsync(byte[] body, CancellationToken token = default(CancellationToken))
        {
            byte[] message = PrependHeader(body);

            return _tcpClient.GetStream().WriteAsync(message, token).AsTask();
        }

        private static byte[] PrependHeader(byte[] body)
        {
            byte[] header = Encoding.ASCII.GetBytes($"{body.Length}:");
            byte[] message = new byte[header.Length + body.Length];
            header.CopyTo(message, 0);
            body.CopyTo(message, header.Length);
            return message;
        }

        private async Task ListenForData(TcpClient tcpClient)
        {
            var interpretingTask = Task.CompletedTask;

            try
            {
                var stream = tcpClient.GetStream();
                byte[] bytes = new byte[46750];
                var preBufferQueue = new ConcurrentQueue<byte[]>();
                var buffer = new List<byte>();
                int length;

                while ((length = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    log.Debug($"Connection[{ConnectionId}]: Read {length}B from stream.");
                    preBufferQueue.Enqueue(bytes.Take(length).ToArray());

                    if (interpretingTask.IsCompleted)
                        interpretingTask = Task.Run(() =>
                        {
                            while (preBufferQueue.TryDequeue(out byte[] packet))
                            {
                                buffer.AddRange(packet);
                            }

                            while (InterpretTcpData(buffer, out byte[] message))
                            {
                                _ = Task.Run(() =>
                                {
                                    MessageReceived?.Invoke(message);
                                });
                            }
                        });
                }
            }
            catch (SocketException)
            {
                //Client disconnected 
            }
            catch (SystemException ex)
            {
                log.Error(ex.Message, ex);
            }
            finally
            {
                DisconnectAsync();
                Disconnected?.Invoke(this);
            }
        }

        /// <summary>
        /// Returns true if there may be more data to be interpreted. 
        /// The buffer may be modified by this function.
        /// </summary>
        private static bool InterpretTcpData(List<byte> buffer, out byte[] message)
        {
            const byte ASCII_COLON = (byte)':';
            message = null;

            int msgSizeEndIndex = buffer.IndexOf(ASCII_COLON);
            if (msgSizeEndIndex < 1)
            {
                //no colon found, check for unexpected characters
                RemoveNonDigitCharacters(buffer);
                return buffer.Count > 0;
            }

            int msgStartIndex = msgSizeEndIndex + 1;
            string msgSizeString = Encoding.ASCII.GetString(buffer.ToArray(), 0, msgSizeEndIndex);
            if (int.TryParse(msgSizeString, out int msgSize))
            {
                if (buffer.Count >= msgStartIndex + msgSize)
                {
                    message = buffer.Skip(msgStartIndex).Take(msgSize).ToArray();
                    buffer.RemoveRange(0, msgStartIndex + msgSize);
                    //full message retrieved, perhaps more to be interpreted
                    return true;
                }
                else
                {
                    //full message length not received yet, keep reading the stream
                    return false;
                }
            }
            else
            {
                log.Debug($"Stream has an invalid message size: " + msgSizeString);

                //remove the invalid message size and interpret more data
                buffer.RemoveRange(0, msgStartIndex);
                return buffer.Count > 0;
            }
        }

        /// <summary>
        /// Returns true if at least one non-digit character was found and removed from the buffer.
        /// </summary>
        private static bool RemoveNonDigitCharacters(List<byte> buffer)
        {
            for (int index = 0; index < buffer.Count; index++)
            {
                if (!char.IsDigit((char)buffer[index]))
                {
                    log.Debug($"Received unexpected characters: " + buffer.ToString());

                    //remove all unexpected characters
                    while (index < buffer.Count && !char.IsDigit((char)buffer[index]))
                        index++;
                    buffer.RemoveRange(0, index);
                    return true;
                }
            }
            return false;
        }
    }
}