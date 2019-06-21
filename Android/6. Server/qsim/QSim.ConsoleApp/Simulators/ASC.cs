using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities;
using System;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Simulators
{
    public class ASC : Simulator
    {
        private const int SPREADER_SPEED = 1;
        private const int TROLLEY_SPEED = 2;
        private const int GANTRY_SPEED = 3;
        private const int TROLLEY_MIN = -600;
        private const int TROLLEY_MAX = 25000;
        private const int TWISTLOCK_DELAY = 5000;
        private const int TELESCOPE_DELAY = 4000;
        private const int SPREADER_MAX = Container.DefaultHeight * 6;

        public ASC(int id)
        {
            Id = PositionProvider.IndexToId("ASC", id);
            NumericId = PositionProvider.IndexToNumericId(id);

            EquipmentWidth = 18000;
            EquipmentLength = 30250;

            _lastPosition = PositionProvider.GetEquipmentPosition(Id);
            _lastSpreaderPosition = new Position(0, TROLLEY_MIN, SPREADER_MAX, 0);
            _ = MoveSpreaderTo(_lastSpreaderPosition);
            _ = MoveTo(_lastPosition);
        }

        private int CalculateDelay(int requestedPos, int previousPos, int speed)
        {
            int delta = previousPos - requestedPos;
            return (int)(Math.Abs(delta) / (speed * _multiplier));
        }

        private int LocationToSpreaderYPosition(Location l)
        {
            if (l.locationType == LocationType.WSTP)
            {
                Position wL1Pos = PositionProvider.GetPosition(new Location(LocationType.WSTP, NumericId, 1, 1, 0));
                Position p = PositionProvider.GetPosition(l);
                return (wL1Pos.x + 2300) - p.x;
            }
            else if (l.locationType == LocationType.YARD)
            {
                Position ya1Pos = PositionProvider.GetPosition(new Location(LocationType.YARD, NumericId, 1, 1, 0));
                Position p = PositionProvider.GetPosition(l);
                return ya1Pos.x - p.x;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Other Location types not implemented");
            }
        }

        private async Task MoveTo(Position position, int delay = 10)
        {
            await _bridge.Update(Id, ObjectType.ASC, position, delay);
            Status.Drive(_lastPosition.DistanceTo(position), delay);
            _lastPosition = position;
        }

        private async Task MoveSpreaderTo(Position position, int delay = 10)
        {
            await _bridge.SpreaderUpdate(Id, position, delay);
            _lastSpreaderPosition = position;
        }

        public async override Task<bool> DriveTo(Position position)
        {
            await RaiseSpreaderToSafeTravelHeight();

            int delay = CalculateDelay(position.y, _lastPosition.y, GANTRY_SPEED);

            await MoveTo(new Position(_lastPosition.x, position.y, _lastPosition.z, _lastPosition.phi), delay);
            await Task.Delay(delay);

            return true;
        }

        public async override Task MoveSpreader(int trolley, int height = -1)
        {
            height = (height == -1) ? _lastSpreaderPosition.z : height;
            trolley = Math.Clamp(trolley, TROLLEY_MIN, TROLLEY_MAX);

            //Cache last spreader position
            int lastSpreaderX = _lastSpreaderPosition.y;
            int lastSpreaderZ = _lastSpreaderPosition.z;
            if (lastSpreaderX != trolley)
            {
                int delay = CalculateDelay(trolley, lastSpreaderX, TROLLEY_SPEED);
                await MoveSpreaderTo(new Position(0, trolley, _lastSpreaderPosition.z, 0), delay);
                await Task.Delay(delay);
            }
            if (lastSpreaderZ != height)
            {
                int delay = CalculateDelay(height, lastSpreaderZ, SPREADER_SPEED);
                await MoveSpreaderTo(new Position(0, _lastSpreaderPosition.y, height, 0), delay);
                await Task.Delay(delay);
            }
        }

        public async Task RaiseSpreaderToSafeTravelHeight()
        {
            await MoveSpreader(_lastSpreaderPosition.y, _stacking.GetSafeHoistHeight(NumericId, LocationType.YARD, SPREADER_MAX));
        }

        public override async Task<bool> PickUp(Location location, Container container)
        {
            Position position = PositionProvider.GetPosition(location);
            _log.Debug($"{Id} picking up {container.Number} from {location}");
            await SetSpreaderSize(container.GetSpreaderSize(), (int)(TELESCOPE_DELAY / _multiplier));
            await DriveTo(new Position(0, position.y, 0, 0));
            await MoveSpreader(LocationToSpreaderYPosition(location), position.z + Container.DefaultHeight);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PickUp(Id, container.Number);
            _stacking.PickContainer(location, container.Number);

            await RaiseSpreaderToSafeTravelHeight();

            return true;
        }

        public override async Task<bool> PutDown(Location location, Container container)
        {
            Position position = PositionProvider.GetPosition(location);
            _log.Debug($"{Id} putting down {container.Number} at {location}");
            await DriveTo(new Position(0, position.y, 0, position.phi));
            await MoveSpreader(LocationToSpreaderYPosition(location), position.z + Container.DefaultHeight);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PutDown(Id, container.Number, position);
            _stacking.PutContainer(location, container.Number);
            Status.PutDownContainer();

            await RaiseSpreaderToSafeTravelHeight();

            return true;
        }
    }
}
