using log4net;
using QSim.ConsoleApp.Messages;
using QSim.ConsoleApp.Utilities;
using QSim.ConsoleApp.DataTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using QSim.ConsoleApp.Utilities.Clipper;
using System.Linq;
using QSim.ConsoleApp.Messages.Visualization;

namespace QSim.ConsoleApp.Middleware
{
    public delegate void BroadcastMessageEvent(JsonMessage message);

    public sealed class VisualizationBridge : IDisposable
    {
        private static readonly Lazy<VisualizationBridge> lazy = new Lazy<VisualizationBridge>(() => new VisualizationBridge());
        private readonly ILog _log;
        private TcpServer _server;
        private readonly ConcurrentDictionary<string, VisualizationObject> _visualizationObjects = new ConcurrentDictionary<string, VisualizationObject>(); 

        public event BroadcastMessageEvent BroadcastMessage;

        public static VisualizationBridge Instance { get { return lazy.Value; } }

        private VisualizationBridge()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public void Dispose()
        {
            _server.ClientConnected -= Server_ClientConnected;
            _server.ClientDisconnected -= Server_ClientDisconnected;
            _server.Dispose();
        }

        public void StartServer(TcpServer server)
        {
            _server = server;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.Start();
        }

        private async Task Broadcast(JsonMessage message)
        {
            var serializeMessage = Task.Run(() => JsonConversion.SerializeMessage(message));
            UpdateCache(message);
            OnBroadcastMessage(message);
            _ = _server.SendBroadcastAsync(await serializeMessage);
        }

        private void OnBroadcastMessage(JsonMessage message)
        {
            BroadcastMessage?.Invoke(message);
        }

        private async void Server_ClientConnected(int connectionId)
        {
            _log.Info($"client [{connectionId}] connected");

            //spam specific client with messages to get it up-to-date
            int numberOfMessages = await UpdateClientToCurrentState(connectionId);
            if (numberOfMessages > 0)
            {
                _log.Info($"client [{connectionId}] updated with {numberOfMessages} messages");
            }
        }

        private void Server_ClientDisconnected(int connectionId)
        {
            _log.Info($"client [{connectionId}] disconnected");
        }

        #region Message methods
        public async Task Update(string id, ObjectType objectType, Position position, int deltaTime = 10)
        {
            await Update(id, objectType, position, new Dimension(1, 1, 1), deltaTime);
        }

        public async Task Update(string id, ObjectType objectType, Location location, int deltaTime = 10)
        {
            await Update(id, objectType, PositionProvider.GetPosition(location), new Dimension(1, 1, 1), deltaTime);
        }

        public async Task Update(string objectId, ObjectType objectType, Position position, Dimension dimension, int deltaTime = 10, long color = 0)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.UPDATE,
                content = new UpdateMessage()
                {
                    id = objectId,
                    type = objectType,
                    x = position.x,
                    y = position.y,
                    z = position.z,
                    p = (int)Math.Round(position.phi * 1000),
                    l = dimension.length,
                    w = dimension.width,
                    h = dimension.height,
                    dt = deltaTime,
                    c = color
                },
            });
        }

        public async Task Delete(string objectId)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.DELETE,
                content = new DeleteMessage()
                {
                    id = objectId,
                },
            });
        }

        public async Task SpreaderUpdate(string objectId, Position position, int deltaTime = 10)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.SPREADER,
                content = new SpreaderUpdate()
                {
                    id = objectId,
                    x = position.x,
                    y = position.y,
                    z = position.z,
                    p = (int)Math.Round(position.phi * 1000),
                    dt = deltaTime
                },
            });
        }

        public async Task SpreaderSizeUpdate(string objectId, SpreaderSize spreaderSize, int deltaTime = 10)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.SPREADER_SIZE,
                content = new SpreaderSizeMessage()
                {
                    equipId = objectId,
                    spreaderSize = spreaderSize,
                    dt = deltaTime
                },
            });
        }

        public async Task PickUp(string objectId, string containerId, int deltaTime = 10)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.PICKUP,
                content = new PickupMessage()
                {
                    equipId = objectId,
                    containerId = containerId
                },
            });
        }

        public async Task PutDown(string objectId, string containerId, Position putdownPosition, ContainerLength containerLength = ContainerLength.LENGTH_40)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.PUTDOWN,
                content = new PutDownMessage()
                {
                    equipId = objectId,
                    containerId = containerId,
                    containerX = putdownPosition.x,
                    containerY = putdownPosition.y,
                    containerZ = putdownPosition.z,
                    containerPhi = (int)Math.Round(putdownPosition.phi * 1000)
                },
            });

            //update Container Position
            await Update(containerId, containerLength == ContainerLength.LENGTH_20 ? ObjectType.CONTAINER_20 : ObjectType.CONTAINER_40, putdownPosition);
        }

        public async Task Claim(string objectId, long color, List<IntPoint> claim)
        {
            var claimPoints = new List<JsonIntPoint>();
            claimPoints.AddRange(claim.Select(p => new JsonIntPoint((int)p.X, (int)p.Y)));
            await Broadcast(new JsonMessage()
            {
                type = MessageType.CLAIM,
                content = new ClaimMessage()
                {
                    id = objectId,
                    color = color,
                    points = claimPoints
                }
            });
        }

        public async Task Status(string objectId, string status)
        {
            await Broadcast(new JsonMessage()
            {
                type = MessageType.STATUS,
                content = new StatusMessage()
                {
                    id = objectId,
                    status = status
                }
            });
        }

        private async Task<int> UpdateClientToCurrentState(int connectionId)
        {
            int numberOfMessages = 0;
            foreach (var pair in _visualizationObjects)
            {
                var updateMessage = new JsonMessage()
                {
                    type = MessageType.UPDATE,
                    content = new UpdateMessage()
                    {
                        id = pair.Key,
                        type = pair.Value.Type,
                        w = pair.Value.Dimension.width,
                        l = pair.Value.Dimension.length,
                        h = pair.Value.Dimension.height,
                        x = pair.Value.Position.x,
                        y = pair.Value.Position.y,
                        z = pair.Value.Position.z,
                        p = (int)Math.Round(pair.Value.Position.phi * 1000),
                        c = pair.Value.Color,
                        dt = 0
                    }
                };

                if (!await _server.TrySendAsync(connectionId, JsonConversion.SerializeMessage(updateMessage))) return -1;
                numberOfMessages++;

                if (pair.Value.SpreaderPosition.x != 0 ||
                    pair.Value.SpreaderPosition.y != 0 ||
                    pair.Value.SpreaderPosition.z != 0 ||
                    pair.Value.SpreaderPosition.phi != 0)
                {
                    var spreaderMessage = new JsonMessage()
                    {
                        type = MessageType.SPREADER,
                        content = new SpreaderUpdate()
                        {
                            id = pair.Key,
                            x = pair.Value.SpreaderPosition.x,
                            y = pair.Value.SpreaderPosition.y,
                            z = pair.Value.SpreaderPosition.z,
                            p = (int)Math.Round(pair.Value.SpreaderPosition.phi * 1000),
                            dt = 0
                        }
                    };

                    if (!await _server.TrySendAsync(connectionId, JsonConversion.SerializeMessage(spreaderMessage))) return -1;
                    numberOfMessages++;
                }

                if (pair.Value.SpreaderContent != "")
                {
                    var containerMessage = new JsonMessage()
                    {
                        type = MessageType.PICKUP,
                        content = new PickupMessage()
                        {
                            equipId = pair.Key,
                            containerId = pair.Value.SpreaderContent
                        }
                    };

                    if (!await _server.TrySendAsync(connectionId, JsonConversion.SerializeMessage(containerMessage))) return -1;
                    numberOfMessages++;
                }
            }
            return numberOfMessages;
        }

        private void UpdateCache(JsonMessage message)
        {
            VisualizationObject visualizationObject;
            switch (message.type)
            {
                case MessageType.UPDATE:
                    {
                        if (!(message.content is UpdateMessage updateContent)) break;
                        var updatedPosition = new Position(updateContent.x, updateContent.y, updateContent.z, updateContent.p / 1000.0);
                        var updatedDimension = new Dimension(updateContent.w, updateContent.l, updateContent.h);

                        if (_visualizationObjects.TryGetValue(updateContent.id, out visualizationObject))
                        {
                            visualizationObject.Position = updatedPosition;
                            visualizationObject.Dimension = updatedDimension;
                        }
                        else
                        {
                            _log.Debug($"Adding {updateContent.id}");
                            _visualizationObjects.TryAdd(
                                updateContent.id,
                                new VisualizationObject(updateContent.type)
                                {
                                    Position = updatedPosition,
                                    Dimension = updatedDimension,
                                    Color = updateContent.c
                                });
                        }
                    }
                    break;
                case MessageType.SPREADER:
                    {
                        if (!(message.content is SpreaderUpdate spreaderContent)) break;
                        if (_visualizationObjects.TryGetValue(spreaderContent.id, out visualizationObject))
                        {
                            visualizationObject.SpreaderPosition = new Position(
                                (int)spreaderContent.x,
                                (int)spreaderContent.y,
                                (int)spreaderContent.z,
                                (int)spreaderContent.p);
                        }
                    }
                    break;
                case MessageType.SPREADER_SIZE:
                    if (!(message.content is SpreaderSizeMessage spreaderSizeContent)) break;
                    if (_visualizationObjects.TryGetValue(spreaderSizeContent.equipId, out visualizationObject))
                    {
                        visualizationObject.SpreaderSize = spreaderSizeContent.spreaderSize;
                    }
                    break;
                case MessageType.PICKUP:
                    {
                        if (!(message.content is PickupMessage pickupContent)) break;
                        //add container to equipment
                        if (_visualizationObjects.TryGetValue(pickupContent.equipId, out visualizationObject))
                        {
                            visualizationObject.AddContainer(pickupContent.containerId);
                        }
                    }
                    break;
                case MessageType.PUTDOWN:
                    {
                        if (!(message.content is PutDownMessage putdownContent)) break;
                        //remove container from equipment
                        if (_visualizationObjects.TryGetValue(putdownContent.equipId, out visualizationObject))
                        {
                            visualizationObject.RemoveContainer();
                        }
                    }
                    break;
                case MessageType.DELETE:
                    if (!(message.content is DeleteMessage deleteContent)) break;
                    _visualizationObjects.TryRemove(deleteContent.id, out visualizationObject);
                    break;
            }
        }

        private class VisualizationObject
        {
            internal VisualizationObject(ObjectType type)
            {
                Type = type;
            }

            internal ObjectType Type { get; }
            internal Position Position { get; set; } = Position.Zero;
            internal Dimension Dimension { get; set; } = new Dimension(1, 1, 1);
            internal long Color { get; set; } = 0;

            internal Position SpreaderPosition { get; set; } = Position.Zero;
            internal string SpreaderContent { get; private set; } = "";
            internal SpreaderSize SpreaderSize { get; set; } = SpreaderSize.SPREADER_40;

            internal void AddContainer(string containerId)
            {
                SpreaderContent = containerId;
            }

            internal void RemoveContainer()
            {
                SpreaderContent = "";
            }
        }

        #endregion
    }
}
